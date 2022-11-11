using System;
using System.Collections.Generic;
using System.Net;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;

public class ClientManager : MonoBehaviour
{
    public static ClientManager Instance;

    [SerializeField] private string ipAddress;
    [SerializeField] private int port;

    public UnityClient client;
    public GameObject characterPrefab;
    public Transform spawnPosition;

    private Dictionary<ushort, GameObject> characterDictionary;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);

        Application.runInBackground = true;
    }

    private void Start()
    {
        characterDictionary = new Dictionary<ushort, GameObject>();
        client = GetComponent<UnityClient>();
        client.MessageReceived += OnReceiveMessage;
        client.Disconnected += OnDisconnected;

        client.ConnectInBackground(ipAddress, port, true, OnConnectedToServer);
    }

    public void SpawnCharacter()
    {
        GameObject myChar = GameObject.Instantiate(characterPrefab);
        //To Change
        myChar.transform.position = spawnPosition.position;
        myChar.GetComponentInChildren<PlayerRagdollInputManager>().isOwner = true;

        SpawnData spawnData = new SpawnData(myChar.transform.position, 0);
        Debug.Log("sending spawndata");
        ClientManager.Instance.SendMessage<SpawnData>(Tags.SpawnRequest, spawnData, SendMode.Reliable);

        CameraControl.Instance.player = myChar.transform.GetChild(0).GetChild(0).GetChild(0);
    }

    private void OnConnectedToServer(Exception e)
    {
        Debug.Log("On Connect Complete");

        SpawnCharacter();
    }

    private void OnDisconnected(object sender, DisconnectedEventArgs e)
    {
        client.Disconnected -= OnDisconnected;
        client.MessageReceived -= OnReceiveMessage;
        client = null;
    }

    private void OnReceiveMessage(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage())
        {
            switch ((Tags)message.Tag)
            {
                case Tags.SpawnResponse:
                    {
                        SpawnData data = message.Deserialize<SpawnData>();
                        GameObject gameObject = GameObject.Instantiate(characterPrefab);
                        gameObject.transform.position = data.position;
                        characterDictionary.Add(data.id, gameObject);
                    }
                    break;
                case Tags.Position:
                    {
                        PositionRotationPayload data = message.Deserialize<PositionRotationPayload>();
                        GameObject gameObject;
                        if (characterDictionary.TryGetValue(client.ID, out gameObject))
                        {
                            gameObject.transform.position = data.position;
                            gameObject.transform.rotation = data.rotation;
                        }
                    }
                    break;
            }
        }
    }

    public void DisconnectFromServer()
    {
        client.MessageReceived -= OnReceiveMessage;
        client.Disconnect();
    }

    public void SendMessage<T>(Tags msgTag, T content, SendMode mode) where T : IDarkRiftSerializable
    {
        if (client.ConnectionState != ConnectionState.Connected) return;
        using (Message m = Message.Create((ushort)msgTag, content))
        {
            client.SendMessage(m, mode);
        }
    }

    private void OnDestroy()
    {
        Debug.Log("destroy client manager");

        if (client != null)
        {
            switch (client.ConnectionState)
            {
                case ConnectionState.Connected:
                    {
                        Debug.Log("destroy while connected");
                        DisconnectFromServer();
                    }
                    break;
                case ConnectionState.Disconnecting:
                    {
                        Debug.Log("Something Went wrong???");
                    }
                    break;
            }
        }
    }
}
