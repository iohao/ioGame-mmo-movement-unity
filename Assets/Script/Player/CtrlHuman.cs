using Protocol.Player;
using UnityEngine;

public class CtrlHuman : BaseHuman
{
    // Use this for initialization
    new void Start()
    {
        base.Start();

        // id = Mathf.Abs(Random.Range(1, 100)).ToString();
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Physics.Raycast(ray, out hit);
            if (hit.collider.tag == "Terrain")
            {
                MoveTo(hit.point);

                PlayerMoveProto playerMoveProto = new PlayerMoveProto()
                {
                    CharacterId = id,
                    X = CommonUtils.positionToFloat(hit.point.x),
                    Y = CommonUtils.positionToFloat(hit.point.y),
                    Z = CommonUtils.positionToFloat(hit.point.z),
                };
                
                NetManager.Send(PackageUtils.CreateMsg(2, 4, playerMoveProto));
            }
        }
    }
}