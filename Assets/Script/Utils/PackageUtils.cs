using System;
using System.Text;
using Com.Iohao.Message;
using Google.Protobuf;
using UnityEngine;

public class PackageUtils
{
    /// <summary>
    /// 创建消息
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="subCmd"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static byte[] CreateMsg(int cmd, int subCmd, IMessage obj)
    {
        ExternalMessage message = new ExternalMessage()
        {
            CmdMerge = CmdMgr.getMergeCmd(cmd, subCmd),
            Data = obj.ToByteString(),
            ProtocolSwitch = 0,
            CmdCode = cmd
        };

        return message.ToByteArray();
    }

    // 打包消息
    public static byte[] PackMessage(byte[] byteMsg)
    {
        // 在数据前加4个字节 用来描述数据长度
        byte[] package = new byte[byteMsg.Length + 4];
        byte[] intArr = IntToArr(byteMsg.Length);
        intArr.CopyTo(package, 0);
        byteMsg.CopyTo(package, intArr.Length);
        return package;
    }

    // 接收服务器是java 需要特殊处理
    public static byte[] IntToArr(int num)
    {
        byte[] data = new byte[4];
        data[3] = (byte)(num & 0xff);
        data[2] = (byte)(num >> 8 & 0xff);
        data[1] = (byte)(num >> 16 & 0xff);
        data[0] = (byte)(num >> 24 & 0xff);
        return data;
    }

    // 做偏移 将数据转string
    public static byte[] UnPackMessage(byte[] packet, int offset, int length)
    {
        byte[] final = new byte[length];
        for (var i = 0; i < final.Length; i++)
        {
            final[i] = packet[i + offset];
        }

        return final;
    }

    /// <summary>
    /// 反序列化protobuf
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dataBytes"></param>
    /// <returns></returns>
    public static T DeserializeMsg<T>(byte[] dataBytes) where T : IMessage, new()
    {
        T msg = new T();
        msg = (T)msg.Descriptor.Parser.ParseFrom(dataBytes);
        return msg;
    }

    public static T DeserializeByByteArray<T>(byte[] dataBytes) where T : IMessage, new()
    {
        CodedInputStream stream = new CodedInputStream(dataBytes);
        T msg = new T();
        msg.MergeFrom(stream);
        return msg;
    }

    /// <summary>
    /// 发送心跳协议
    /// </summary>
    /// <returns></returns>
    public static byte[] SendPing()
    {
        var externalMessage = new ExternalMessage
        {
            CmdCode = 0
        };

        return externalMessage.ToByteArray();
    }
}