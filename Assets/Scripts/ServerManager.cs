using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;
using UnityEngine.SceneManagement;
using static UnityEngine.GraphicsBuffer;
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
        if(ClientManager.Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        if(Instance != null)
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
        if(server != null && server.ClientManager != null)
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
        if(characterDictionary.TryGetValue(client.ID, out gameObject))
        {
            Destroy(gameObject);
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
        using(Message message = e.GetMessage())
        {
            switch ((Tags)message.Tag)
            {
                case Tags.SpawnRequest:
                    {
                        SpawnData data = message.Deserialize<SpawnData>();
                        GameObject gameObject = GameObject.Instantiate(characterPrefab);
                        gameObject.transform.position = data.position;
                        characterDictionary.Add(client.ID, gameObject);

                        SendMessageToAllClients<SpawnData>(Tags.SpawnResponse, data, SendMode.Reliable, client.ID);

                        foreach(KeyValuePair<ushort, GameObject> chara in characterDictionary)
                        {
                            if(chara.Key != client.ID)
                            {
                                SpawnData dataOthers = new SpawnData(chara.Value.transform.position, chara.Key);
                                SendMessage<SpawnData>(client.ID, Tags.SpawnResponse, dataOthers, SendMode.Reliable);
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
                        if(characterDictionary.TryGetValue(client.ID, out gameObject))
                        {
                            //Add Configurable Joint
                            //ConfigurableJoint joint = gameObject.transform.Find("Root").GetComponent<ConfigurableJoint>();
                            objectToFind = gameObject.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
                            rootRB = objectToFind.GetComponent<Rigidbody>();
                            joint = rootRB.GetComponent<ConfigurableJoint>();
                            joint.targetRotation = data.rotation;
                            //Debug.Log(objectToFind);
                            //Debug.Log(joint);
                            Debug.Log(data.rotation);


                            //gameObject.transform.position = data.position;
                            //gameObject.transform.rotation = data.rotation;
                            //joint.targetRotation = data.rotation;
                            gameObject.transform.GetChild(0).GetChild(0).GetChild(0).position = data.position;
                            SendMessageToAllClients<PositionRotationPayload>(Tags.Position, data, SendMode.Unreliable, client.ID);
                        }
                    }
                    break;

            }
        }
    }

    public void SendMessage<T>(ushort destID, Tags msgTag, T content, SendMode mode) where T : IDarkRiftSerializable
    {
        GameObject gameObject;
        if(characterDictionary.TryGetValue(destID, out gameObject))
        {
            using(Message m = Message.Create((ushort)msgTag, content))
            {
                server.ClientManager[destID].SendMessage(m, mode);
            }
        }
    }

    public void SendMessageToAllClients<T>(Tags msgTag, T content, SendMode mode, ushort exceptionID) where T : IDarkRiftSerializable
    {
        foreach(IClient client in server.ClientManager.GetAllClients())
        {
            if (client.ID == exceptionID) continue;
            using (Message m = Message.Create((ushort)msgTag, content))
            {
                client.SendMessage(m, mode);
            }
        }
    }

    public void SendMessage(ushort destID, Message msg, SendMode mode)
    {
        GameObject gameObject;
        if(characterDictionary.TryGetValue(destID, out gameObject))
        {
            server.ClientManager[destID].SendMessage(msg, mode);
        }
    }
    
}


