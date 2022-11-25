using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;
using Unity.VisualScripting;

public class ServerManager : MonoBehaviour
{
    public static ServerManager Instance;
    private XmlUnityServer xmlServer;
    private DarkRiftServer server;

    public GameObject characterPrefab;
    private Dictionary<ushort, GameObject> characterDictionary;

    private void Awake()
    {
        if (ClientManager.Instance != null)
        {
 
            Destroy(gameObject);
            return;
        }

        if (Instance != null)
        {

            Destroy(gameObject);
            return;
        }
 
        Instance = this;
        DontDestroyOnLoad(this);
        Application.runInBackground = true;
    }

    protected virtual void Start()
    {
        characterDictionary = new Dictionary<ushort, GameObject>();
        xmlServer = GetComponent<XmlUnityServer>();
        server = xmlServer.Server;
        server.ClientManager.ClientConnected += OnClientConnected;
        server.ClientManager.ClientDisconnected += OnClientDisconnected;
    }

    private void OnDestroy()
    {
        if (server != null && server.ClientManager != null)
        {
            server.ClientManager.ClientConnected -= OnClientConnected;
            server.ClientManager.ClientDisconnected -= OnClientDisconnected;
        }
    }

    private void OnClientDisconnected(object sender, ClientDisconnectedEventArgs e)
    {
        IClient client = e.Client;
        e.Client.MessageReceived -= OnReceiveMessage;
        Debug.Log("On Client Disconnected");
        GameObject gameObject;
        if (characterDictionary.TryGetValue(client.ID, out gameObject))
        {
            Destroy(gameObject);
            PlayerDisconnect data = new PlayerDisconnect(client.ID);
            SendMessageToAllClients<PlayerDisconnect>(Tags.PlayerDisconnect, data, SendMode.Reliable, client.ID);
            characterDictionary.Remove(client.ID);
        }
    }

    private void OnClientConnected(object sender, ClientConnectedEventArgs e)
    {
        Debug.Log("On Client Connected");

        e.Client.MessageReceived += OnReceiveMessage;
    }

    protected virtual void OnReceiveMessage(object sender, MessageReceivedEventArgs e)
    {
        IClient client = (IClient)sender;
        using (Message message = e.GetMessage())
        {
            switch ((Tags)message.Tag)
            {
                case Tags.SpawnRequestForSelf:
                    {
                        SpawnData data = message.Deserialize<SpawnData>();
                        data.id = client.ID;
                        GameObject gameObject = GameObject.Instantiate(characterPrefab);
                        gameObject.transform.GetChild(0).GetChild(0).GetChild(0).position = data.position;
                        characterDictionary.Add(client.ID, gameObject);
                        foreach (KeyValuePair<ushort, GameObject> chara in characterDictionary)
                        {
                            if (chara.Key != client.ID)
                            {
                                GameObject go = chara.Value;
                                SpawnData otherData = new SpawnData(go.transform.GetChild(0).GetChild(0).GetChild(0).position, chara.Key);
                                SendMessage<SpawnData>(client.ID, Tags.SpawnResponse, otherData, SendMode.Reliable);
                            }
                        }
                    }
                    break;
                case Tags.Position:
                    {
                        PositionRotationPayload data = message.Deserialize<PositionRotationPayload>();
                        GameObject gameObject, objectToFind;
                        ConfigurableJoint joint;
                        Rigidbody rootRB;
                        Animator ani, aniLeft, aniRight;
                        if (characterDictionary.TryGetValue(client.ID, out gameObject))
                        {
                            data.id = client.ID;

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

                            SendMessageToAllClients<PositionRotationPayload>(Tags.Position, data, SendMode.Reliable, client.ID);
                        }
                    }
                    break;
                case Tags.SpawnForAll:
                    {
                        Debug.Log("SpawnForAll Called");
                        Debug.Log(characterDictionary);
                        SpawnData data = message.Deserialize<SpawnData>();
                        data.id = client.ID;
                        Debug.Log("Player Connected, Player ID: " + client.ID);
                        SendMessageToAllClients<SpawnData>(Tags.SpawnResponse, data, SendMode.Reliable, client.ID);
                    }
                    break;
                case Tags.GrabAnimation:
                    {
                        GrabAnimation data = message.Deserialize<GrabAnimation>();
                        GameObject gameObject, grabObject, leftHand, rightHand;
                        Rigidbody leftRB, rightRB, attachedObject;
                        Animator aniLeft, aniRight;
                        if(characterDictionary.TryGetValue(client.ID, out gameObject))
                        {
                            data.id = client.ID;

                            aniRight = gameObject.transform.GetChild(1).GetChild(1).gameObject.GetComponent<Animator>();
                            aniLeft = gameObject.transform.GetChild(1).GetChild(2).gameObject.GetComponent<Animator>();

                            aniRight.SetBool("IsRightUp", data.isGrab);
                            aniLeft.SetBool("IsLeftUp", data.isGrab);

                            rightHand = gameObject.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetChild(0).gameObject;
                            rightRB = rightHand.GetComponent<Rigidbody>();
                            leftHand = gameObject.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).gameObject;
                            leftRB = leftHand.GetComponent<Rigidbody>();


                            grabObject = ObjectGrabbing.Instance.GetObject();
                            attachedObject = grabObject.GetComponent<Rigidbody>();

                            if(attachedObject != null && data.isGrab)
                            {
                                FixedJoint fj = attachedObject.AddComponent<FixedJoint>();
                                fj.connectedBody = leftRB;
                                fj.breakForce = 10000;
                            }
                            else
                            {
                                FixedJoint fj = attachedObject.GetComponent<FixedJoint>();
                                fj.connectedBody = null;
                                fj.breakForce = 0;
                                Destroy(fj);
                            }
                            SendMessageToAllClients<GrabAnimation>(Tags.GrabAnimation, data, SendMode.Reliable, client.ID);
                        }
                    }
                    break;
                case Tags.ObjectResponse:
                    {
                        Debug.Log("Recieved");
                        ObjectResponse data = message.Deserialize<ObjectResponse>();
                        List<ObjectSynchronizer> list = ObjectSyncManager.Instance.list;

                        Vector3[] objectPos = new Vector3[list.Count];
                        Quaternion[] objectRot = new Quaternion[list.Count];
                        ushort[] objectIds = new ushort[list.Count];
                        string[] objectPrefabIds = new string[list.Count];

                        for(int i = 0; i < list.Count; i++)
                        {
                            objectIds[i] = list[i].id;
                            objectPrefabIds[i] = list[i].prefabId;
                            objectPos[i] = list[i].transform.position;
                            objectRot[i] = list[i].transform.rotation;
                            Debug.Log(objectPrefabIds[i]);
                        }
                        Debug.Log(objectPrefabIds.Length);

                        SpawnObjects spawnObjects = new SpawnObjects(objectIds, objectPrefabIds, objectPos, objectRot);
                        SendMessage<SpawnObjects>(client.ID, Tags.SpawnObjects, spawnObjects, DarkRift.SendMode.Reliable);
                        Debug.Log("Spawn Object Data Sent");
                    }
                    break;
                    //it will receive the tag, and send the the list of objectsync to the client that asked it, give also the position and rotation
            }
        }
    }

    public void SendMessage<T>(ushort destID, Tags msgTag, T content, SendMode mode) where T : IDarkRiftSerializable
    {
        GameObject gameObject;
        if (characterDictionary.TryGetValue(destID, out gameObject))
        {
            using (Message m = Message.Create((ushort)msgTag, content))
            {
                server.ClientManager[destID].SendMessage(m, mode);
            }
        }
    }

    public void SendMessageToAllClients<T>(Tags msgTag, T content, SendMode mode, ushort exceptionID) where T : IDarkRiftSerializable
    {
        foreach (IClient client in server.ClientManager.GetAllClients())
        {
            if (client.ID == exceptionID) continue;
            using (Message m = Message.Create((ushort)msgTag, content))
            {
                client.SendMessage(m, mode);
            }
        }
    }

    public void SendMessageToAllClients<T>(Tags msgTag, T content, SendMode mode) where T : IDarkRiftSerializable
    {
        foreach (IClient client in server.ClientManager.GetAllClients())
        {
          
            using (Message m = Message.Create((ushort)msgTag, content))
            {
                client.SendMessage(m, mode);
            }
        }
    }

    public void SendMessage(ushort destID, Message msg, SendMode mode)
    {
        GameObject gameObject;
        if (characterDictionary.TryGetValue(destID, out gameObject))
        {
            server.ClientManager[destID].SendMessage(msg, mode);
        }
    }

    public void ObjectTrigger(Collider collider, bool isAttached)
    {
        if(isAttached && collider.gameObject != null)
        {
            
        }
    }

}


