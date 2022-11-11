using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkObjectCheck : MonoBehaviour
{
    void Update()
    {
        //PositionRotationPayload positionPayload = new PositionRotationPayload(transform.position, transform.rotation);
        //ClientManager.Instance.SendMessage<PositionRotationPayload>(Tags.Position, positionPayload, DarkRift.SendMode.Reliable);
    }
}
