using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Com.Iohao.Message;
using Google.Protobuf;
using Protocol.Player;
using UnityEngine;

public static class NetManager
{
    // 缓冲区大小
    const int DEF_RECV_BUFFER_SIZE = 64 * 1024;

    // 定义套接字
    static Socket socket;

    // 接收缓冲区
    private static MemoryStream receiveBuffer = new MemoryStream(DEF_RECV_BUFFER_SIZE);

    // 解码缓冲区
    private static MemoryStream stream = new MemoryStream(DEF_RECV_BUFFER_SIZE);

    private static int readOffset = 0;

    // 是否正在连接
    static bool isConnecting = false;

    // 是否启用心跳
    public static bool isUsePing = true;

    // 心跳间隔时间
    public static int pingInterval = 4;

    //上一次发送PING的时间
    static float lastPingTime = 0;

    //上一次收到PONG的时间
    static float lastPongTime = 0;

    // 事件委托类型
    public delegate void EventListener(String err);

    // 事件监听列表
    private static Dictionary<NetEvent, EventListener> eventListeners = new Dictionary<NetEvent, EventListener>();

    // 委托类型
    public delegate void MsgListener(byte[] message);

    // 监听列表
    private static Dictionary<string, MsgListener> listeners = new Dictionary<string, MsgListener>();

    // 添加监听
    public static void AddListener(string cmd, MsgListener listener)
    {
        listeners[cmd] = listener;
    }

    // 事件
    public enum NetEvent
    {
        ConnectSucc = 1,
        ConnectFail = 2,
        Close = 3,
    }

    /// 连接
    public static void Connect(string ip, int port)
    {
        //Socket
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        IAsyncResult result = socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), port), ConnectCallback, socket);
        result.AsyncWaitHandle.WaitOne(1000);

        // 连接后 设置心跳包监听
        // AddListener("0", OnPong);
    }

    // Connect回调
    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            FireEvent(NetEvent.ConnectSucc, "");
            isConnecting = false;
            //开始接收
            // socket.BeginReceive(readBuff.bytes, readBuff.writeIdx,
            //     readBuff.remain, 0, ReceiveCallback, socket);
            //
            // socket.Receive(receiveBuffer.GetBuffer(), 0, receiveBuffer.Capacity, SocketFlags.None);
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket Connect fail " + ex.ToString());
            FireEvent(NetEvent.ConnectFail, ex.ToString());
            isConnecting = false;
        }
    }

    // 分发事件
    private static void FireEvent(NetEvent netEvent, String err)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent](err);
        }
    }

    /// 收到新消息
    private static bool ProcessRecv()
    {
        if (socket != null && socket.Poll(0, SelectMode.SelectRead))
        {
            int num = socket.Receive(receiveBuffer.GetBuffer(), 0, receiveBuffer.Capacity, SocketFlags.None);

            ReceiveData(receiveBuffer.GetBuffer(), 0, num);
        }

        return false;
    }

    /// <summary>
    /// 接收消息
    /// </summary>
    /// <param name="data"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    /// <exception cref="Exception"></exception>
    private static void ReceiveData(byte[] data, int offset, int count)
    {
        // 消息太多 缓冲区装不下 抛异常
        if (stream.Position + count > stream.Capacity)
        {
            throw new Exception("stream.Position + count > stream.Capacity");
        }

        // 写入缓存区
        stream.Write(data, offset, count);
        ParsePackage();
    }

    /// <summary>
    /// 拆包
    /// </summary>
    /// <exception cref="Exception"></exception>
    private static void ParsePackage()
    {
        // 解码区有足够的 byte[] 供解析了 至少需要4个字节也就是一个int 才进行解析
        if (readOffset + 4 < stream.Position)
        {
            // 返回数组中前4个字节的int数字
            int num = BitConverter.ToInt32(stream.GetBuffer(), readOffset);

            // 包有效
            if (num + readOffset + 4 <= stream.Position)
            {
                byte[] unPackMessage = PackageUtils.UnPackMessage(stream.GetBuffer(), readOffset + 4, num);

                if (unPackMessage == null)
                {
                    throw new Exception("解包失败！");
                }

                ExternalMessage externalMessage =
                    PackageUtils.DeserializeByByteArray<ExternalMessage>(unPackMessage);

                // 监听回调;
                if (listeners.ContainsKey(externalMessage.CmdMerge.ToString()))
                {
                    listeners[externalMessage.CmdMerge.ToString()](externalMessage.Data.ToByteArray());
                }

                // 记录缓冲区 最新位置
                readOffset += num + 4;
                ParsePackage();
            }
        }
    }

    // 发送
    public static void Send(byte[] sendData)
    {
        if (socket == null) return;
        if (!socket.Connected) return;
        socket.Send(PackageUtils.PackMessage(sendData));
    }

    // Update
    public static void Update()
    {
        ProcessRecv();
        PingUpdate();
    }
    
    // 发送PING协议
    private static void PingUpdate()
    {
        // 是否启用
        if (!isUsePing)
        {
            return;
        }
        
        // 发送PING
        if (Time.time - lastPingTime > pingInterval)
        {
            Send(PackageUtils.SendPing());
            lastPingTime = Time.time;
        }
        
        // 检测PONG时间
        // if (Time.time - lastPongTime > pingInterval)
        // {
        //     // Close();
        //     Debug.Log("心跳超时 关闭连接!");
        // }
    }

    // 心跳包回应
    private static void OnPong(byte[] message)
    {
        lastPongTime = Time.time;
    }
}