using System.Collections.Generic;
using Com.Iohao.Message;
using Google.Protobuf;
using Protocol.Character;
using Protocol.Player;
using UnityEngine;

public class PlayerMain : MonoBehaviour
{
    //人物模型预设
    public GameObject humanPrefab;

    //人物列表
    public BaseHuman myHuman;

    public Dictionary<string, BaseHuman> otherHumans = new Dictionary<string, BaseHuman>();

    void Update()
    {
        NetManager.Update();
    }

    void Start()
    {
        // 网络事件监听
        NetManager.AddListener(CmdMgr.getMergeCmd((int)ActionCmd.cmd, (int)ActionCmd.enterGame).ToString(), OnEnter);
        NetManager.AddListener(CmdMgr.getMergeCmd((int)ActionCmd.cmd, (int)ActionCmd.move).ToString(), OnMove);
        NetManager.AddListener(CmdMgr.getMergeCmd((int)ActionCmd.cmd, (int)ActionCmd.leaveMap).ToString(), OnLeave);
        NetManager.AddListener(CmdMgr.getMergeCmd((int)ActionCmd.cmd, (int)ActionCmd.syncPlayer).ToString(), OnList);

        NetManager.Connect("127.0.0.1", 10100);

        // 添加一个角色
        GameObject obj = Instantiate(humanPrefab);
        myHuman = obj.AddComponent<CtrlHuman>();
        myHuman.id = Mathf.Abs(Random.Range(1, 1000)).ToString();

        float x = Random.Range(-5, 5);
        float z = Random.Range(-5, 5);
        obj.transform.position = new Vector3(x, 0, z);

        Vector3 pos = myHuman.transform.position;
        Vector3 eul = myHuman.transform.eulerAngles;

        CharacterProto characterProto = new CharacterProto()
        {
            CharacterId = myHuman.id,
            MapId = 1,
            MapPostX = pos.x,
            MapPostY = pos.y,
            MapPostZ = pos.z,
            Orientation = eul.y,
        };

        // 进入游戏请求
        NetManager.Send(PackageUtils.CreateMsg((int)ActionCmd.cmd, (int)ActionCmd.enterGame, characterProto));
    }

    void OnEnter(byte[] message)
    {
        EnterMapProto enterMapProto = PackageUtils.DeserializeByByteArray<EnterMapProto>(message);

        // 不是自己 添加一个角色
        if (!enterMapProto.CharacterId.Equals(myHuman.id))
        {
            GameObject obj = (GameObject)Instantiate(humanPrefab);
            obj.transform.position =
                new Vector3(enterMapProto.MapPostX, enterMapProto.MapPostY, enterMapProto.MapPostZ);
            obj.transform.eulerAngles = new Vector3(0, enterMapProto.Orientation, 0);
            BaseHuman h = obj.AddComponent<SyncHuman>();
            otherHumans.Add(enterMapProto.CharacterId, h);
        }

        // 同步周围玩家请求
        NetManager.Send(PackageUtils.CreateMsg((int)ActionCmd.cmd, (int)ActionCmd.syncPlayer,
            new SyncMapPlayerProto()));
    }

    void OnMove(byte[] message)
    {
        // Debug.Log("OnMove" + message);

        PlayerMoveProto playerMoveProto = PackageUtils.DeserializeByByteArray<PlayerMoveProto>(message);

        if (!otherHumans.ContainsKey(playerMoveProto.CharacterId))
            return;

        Vector3 targetPos = new Vector3(playerMoveProto.X, playerMoveProto.Y, playerMoveProto.Z);
        otherHumans[playerMoveProto.CharacterId].MoveTo(targetPos);
    }

    void OnLeave(byte[] message)
    {
        Debug.Log("OnLeave" + message);

        LeaveMapProto leaveMapProto = PackageUtils.DeserializeMsg<LeaveMapProto>(message);

        if (!otherHumans.ContainsKey(leaveMapProto.CharacterId))
            return;
        BaseHuman h = otherHumans[leaveMapProto.CharacterId];
        Destroy(h.gameObject);
        otherHumans.Remove(leaveMapProto.CharacterId);
    }

    void OnList(byte[] message)
    {
        // Debug.Log("OnList" + message);

        ExternalMessage externalMessage = PackageUtils.DeserializeMsg<ExternalMessage>(message);
        SyncMapPlayerProto deserializeByByteArray =
            PackageUtils.DeserializeByByteArray<SyncMapPlayerProto>(externalMessage.Data.ToByteArray());

        foreach (CharacterProto characterMsg in deserializeByByteArray.PlayerCharacterList)
        {
            // 不是自己 添加一个角色
            if (!characterMsg.CharacterId.Equals(myHuman.id) && !otherHumans.ContainsKey(characterMsg.CharacterId))
            {
                GameObject obj = Instantiate(humanPrefab);
                obj.transform.position =
                    new Vector3(characterMsg.MapPostX, characterMsg.MapPostY, characterMsg.MapPostZ);
                obj.transform.eulerAngles = new Vector3(0, characterMsg.Orientation, 0);
                BaseHuman h = obj.AddComponent<SyncHuman>();
                h.id = characterMsg.CharacterId;
                otherHumans.Add(characterMsg.CharacterId, h);
            }
        }
    }
}