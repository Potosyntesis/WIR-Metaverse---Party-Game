using DarkRift;
using DarkRift.Client;
using DarkRift.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkriftSerializationExtensions;

public enum ObjectSyncTag
{
    SyncPositionAndRotation = 1000,
    SyncAnimation,
    Pause,
    Resume
}

public class ObjectSynchronizer : MonoBehaviour
{
    private Vector3 lastPosition;
    private Quaternion lastRotation;

    public bool isServer = false;

    public ushort id;

    public string prefabId;

    public Vector3 newPosition;
    public Quaternion newRotation = Quaternion.identity;
    public float smoothness;

    bool settingUpPosition = false;
    void Start()
    {


        if (ClientManager.Instance != null)
        {
            if (isServer)
            {
                Destroy(gameObject);
            }
        }
        if (ServerManager.Instance != null)
        {
            if (!isServer)
            {
                Destroy(gameObject);
            }
            else
            {
                ObjectSyncManager.Instance.AddToList(this);
            }
        }
        
    }

    public void SetUpPosition(Vector3 position, Quaternion rotation)
    {
        Debug.Log("set up position");

        transform.position = position;
        transform.rotation = rotation;

    }

    IEnumerator SetUpPositionRoutine(Vector3 position, Quaternion rotation)
    {

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        transform.position = position;
        transform.rotation = rotation;
        settingUpPosition = false;
    }


    private void OnDestroy()
    {
        if (ObjectSyncManager.Instance != null)
        {
            ObjectSyncManager.Instance.list.Remove(this);
        }

    }

    private void FixedUpdate()
    {
        if (ServerManager.Instance != null)
        {
            SendPosAndRotToClients();
            return;
        }
       // if (settingUpPosition) return;

        if (transform.position != newPosition)
        {
            transform.position = Vector3.Lerp(transform.position, newPosition, Time.fixedDeltaTime * smoothness);
        }
        if (transform.rotation != newRotation)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.fixedDeltaTime * smoothness);
        }
    }



    public void SendPosAndRotToClients()
    {
        if (transform.position != lastPosition || transform.rotation != lastRotation)
        {
            SyncPositionAndRotation data = new SyncPositionAndRotation(id, transform.position, transform.rotation);
            ServerManager.Instance.SendMessageToAllClients<SyncPositionAndRotation>(Tags.SyncPositionAndRotation, data, SendMode.Reliable);
            lastPosition = transform.position;
            lastRotation = transform.rotation;
        }

    }
}
