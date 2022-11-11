using DarkRift;
using DarkRift.Server;
using DarkriftSerializationExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSyncManager : MonoBehaviour
{
    public static ObjectSyncManager Instance;
    public List<ObjectSynchronizer> list;
    public ushort id = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        list = new List<ObjectSynchronizer>();
    }
    public void Init()
    {
        if (ClientManager.Instance != null && ClientManager.Instance.client)
        {
            ObjectResponse response = new ObjectResponse(0);
            ClientManager.Instance.SendMessage<ObjectResponse>(Tags.ObjectResponse, response, DarkRift.SendMode.Reliable);
            Debug.Log("Object Response");
        }
    }

    public void AddToList(ObjectSynchronizer obj)
    {
        list.Add(obj);
        obj.id = id;

        id++;
    }

    public bool IsContainingId(ushort searchId)
    {
        int i = 0;
        int size = list.Count;
        for (; i < size; i++)
        {
            if (list[i].id == searchId)
            {
                return true;
            }
        }
        return false;
    }
    public ObjectSynchronizer GetObjectSync(ushort searchId)
    {
        int i = 0;
        int size = list.Count;
        for (; i < size; i++)
        {
            if (list[i].id == searchId)
            {
                return list[i];
            }
        }
        return null;
    }

    private void OnDestroy()
    {
        if (ClientManager.Instance != null)
        {
            if (ClientManager.Instance.client)
            {

            }
                //ClientManager.Instance.client.MessageReceived -= OnClientMessageReceived;
        }
    }
}
