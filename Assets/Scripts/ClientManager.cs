using System;
using System.Collections.Generic;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using UnityEngine;

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
        myChar.transform.position = spawnPosition.position;
        myChar.GetComponentInChildren<PlayerRagdollInputManager>().isOwner = true;
        CameraControl.Instance.player = myChar.transform.GetChild(0).GetChild(0).GetChild(0);

        SpawnData spawnData = new SpawnData(myChar.transform.position, 0);
        Debug.Log("sending spawndata");
        ClientManager.Instance.SendMessage<SpawnData>(Tags.SpawnForAll, spawnData, SendMode.Reliable);
        ClientManager.Instance.SendMessage<SpawnData>(Tags.SpawnRequestForSelf, spawnData, SendMode.Reliable);
    }

    private void OnConnectedToServer(Exception e)
    {
        Debug.Log("On Connect Complete");
        SpawnCharacter();
        ObjectSyncManager.Instance.Init();
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
                        gameObject.transform.GetChild(0).GetChild(0).GetChild(0).position = data.position;
                        characterDictionary.Add(data.id, gameObject);
                        Debug.Log("data.id = " + data.id);
                        gameObject.GetComponentInChildren<PlayerRagdollInputManager>().isOwner = false;
                    }
                    break;
                case Tags.Position:
                    {
                        PositionRotationPayload data = message.Deserialize<PositionRotationPayload>();
                        GameObject gameObject, objectToFind;
                        ConfigurableJoint joint;
                        Rigidbody rootRB;
                        Animator ani, aniLeft, aniRight;
                        if (characterDictionary.TryGetValue(data.id, out gameObject))
                        {
                            objectToFind = gameObject.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
                            rootRB = objectToFind.GetComponent<Rigidbody>();
                            joint = rootRB.GetComponent<ConfigurableJoint>();
                            joint.targetRotation = data.rotation;

                            gameObject.transform.GetChild(0).GetChild(0).GetChild(0).position = data.position;

                            ani = gameObject.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Animator>();
                            aniRight = gameObject.transform.GetChild(1).GetChild(1).gameObject.GetComponent<Animator>();
                            aniLeft = gameObject.transform.GetChild(1).GetChild(2).gameObject.GetComponent<Animator>();

                            ani.SetBool("IsWalk", data.isWalk);
                            aniRight.SetBool("IsWalk", data.isWalk);
                            aniLeft.SetBool("IsWalk", data.isWalk);

                            //ani = gameObject.transform.GetChild(1).GetComponents<Animator>();
                            //ani[0].SetBool("IsWalk", data.isWalk);
                            //ani[1].SetBool("IsWalk", data.isWalk);
                            //ani[2].SetBool("IsWalk", data.isWalk);
                        }
                    }
                    break;
                case Tags.PlayerDisconnect:
                    {
                        PlayerDisconnect data = message.Deserialize<PlayerDisconnect>();
                        GameObject gameObject;
                        if (characterDictionary.TryGetValue(client.ID, out gameObject))
                        {
                            Debug.Log("Player Disconnected");
                            Destroy(gameObject);
                            characterDictionary.Remove(client.ID);
                        }
                    }
                    break;
                case Tags.GrabAnimation:
                    {
                        GrabAnimation data = message.Deserialize<GrabAnimation>();
                        GameObject gameObject;
                        Animator aniLeft, aniRight;
                        if (characterDictionary.TryGetValue(data.id, out gameObject))
                        {
                            aniRight = gameObject.transform.GetChild(1).GetChild(1).gameObject.GetComponent<Animator>();
                            aniLeft = gameObject.transform.GetChild(1).GetChild(2).gameObject.GetComponent<Animator>();

                            aniRight.SetBool("IsRightUp", data.isGrab);
                            aniLeft.SetBool("IsLeftUp", data.isGrab);
                        }
                    }
                    break;
                case Tags.SyncPositionAndRotation:
                    {
                        SyncPositionAndRotation data = message.Deserialize<SyncPositionAndRotation>();

                        ObjectSynchronizer obj = GetObjectSync(data.id);
                        if (obj != null)
                        {
                            obj.newPosition = data.position;
                            obj.newRotation = data.rotation;
                        }
                    }
                    break;
                case Tags.SpawnObjects:
                    {
                        SpawnObjects data = message.Deserialize<SpawnObjects>();
                        Debug.Log("A");
                        int i = 0;
                        int size = data.prefabIds.Length;
                        for(;i<size;i++)
                        {
                            Debug.Log("B");

                            GameObject prefab = PrefabMapper.Instance.GetPrefabClientWithId(data.prefabIds[i]);
                            GameObject instantiantedObject = Instantiate(prefab);
                            Debug.Log(data.position[i]);
                             
                            instantiantedObject.transform.position = data.position[i];
                            instantiantedObject.transform.rotation = data.rotation[i];

                            Rigidbody rb = instantiantedObject.GetComponent<Rigidbody>();
                            rb.isKinematic = true;
                            
                            ObjectSynchronizer obj = instantiantedObject.GetComponent<ObjectSynchronizer>();
                            obj.id = data.ids[i];
                            obj.prefabId = data.prefabIds[i];
                            ObjectSyncManager.Instance.list.Add(obj);
                        }


                    }
                    break;
            }
        }
    }

    public ObjectSynchronizer GetObjectSync(ushort searchId)
    {
        int i = 0;
        int size = ObjectSyncManager.Instance.list.Count;
        for (; i < size; i++)
        {
            if (ObjectSyncManager.Instance.list[i].id == searchId)
            {
                return ObjectSyncManager.Instance.list[i];
            }
        }
        return null;
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
