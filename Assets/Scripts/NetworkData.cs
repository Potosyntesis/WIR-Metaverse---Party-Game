using DarkRift;
using DarkriftSerializationExtensions;
using UnityEngine;

public enum Tags
{
    SpawnRequestForSelf,
    SpawnResponse,
    Position,
    PlayerDisconnect,
    SpawnForAll,
    GrabAnimation,
    SyncPositionAndRotation,
    SpawnObjects,
    ObjectResponse

}



public struct GrabAnimation : IDarkRiftSerializable
{
    public bool isGrab;
    public bool isPush;
    public ushort id;

    public GrabAnimation(bool _isGrab, bool _isPush, ushort _id)
    {
        isGrab = _isGrab;
        isPush = _isPush;
        id = _id;
    }

    public void Deserialize(DeserializeEvent e)
    {
        isGrab = e.Reader.ReadBoolean();
        isPush = e.Reader.ReadBoolean();
        id = e.Reader.ReadUInt16();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(isGrab);
        e.Writer.Write(isPush);
        e.Writer.Write(id);
    }
}

public struct SpawnData : IDarkRiftSerializable
{
    public Vector3 position;
    public ushort id;
    public SpawnData(Vector3 _position, ushort _id)
    {
        position = _position;
        id = _id;
    }

    public void Deserialize(DeserializeEvent e)
    {
        position = e.Reader.ReadVector3();
        id = e.Reader.ReadUInt16();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.WriteVector3(position);
        e.Writer.Write(id);
    }
}
public struct PlayerDisconnect : IDarkRiftSerializable
{
    public ushort id;
    public PlayerDisconnect(ushort _id)
    {
        id = _id;
    }

    public void Deserialize(DeserializeEvent e)
    {
        id = e.Reader.ReadUInt16();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(id);
    }
}

public struct PositionRotationPayload : IDarkRiftSerializable
{
    public Vector3 position;
    public Quaternion rotation;
    public ushort id;
    public bool isWalk;

    public PositionRotationPayload(Vector3 _position, Quaternion _rotation, ushort _id, bool _isWalk)
    {
        position = _position;
        rotation = _rotation;
        id = _id;
        isWalk = _isWalk;
    }

    public void Deserialize(DeserializeEvent e)
    {
        position = e.Reader.ReadVector3();
        rotation = e.Reader.ReadQuaternionCompressed();
        id = e.Reader.ReadUInt16();
        isWalk = e.Reader.ReadBoolean();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.WriteVector3(position);
        e.Writer.WriteQuaternionCompressed(rotation);
        e.Writer.Write(id);
        e.Writer.Write(isWalk);
    }
}

public struct SyncPositionAndRotation : IDarkRiftSerializable
{
    public ushort id;
    public Vector3 position;
    public Quaternion rotation;

    public SyncPositionAndRotation(ushort _id, Vector3 _position, Quaternion _rotation)
    {
        id = _id;
        position = _position;
        rotation = _rotation;
    }

    public void Deserialize(DeserializeEvent e)
    {
        id = e.Reader.ReadUInt16();
        position = e.Reader.ReadVector3();
        rotation = e.Reader.ReadQuaternionCompressed();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(id);
        e.Writer.WriteVector3(position);
        e.Writer.WriteQuaternionCompressed(rotation);
    }
}

public struct SpawnObjects : IDarkRiftSerializable
{
    public ushort[] ids;
    public string[] prefabIds;
    public Vector3[] position;
    public Quaternion[] rotation;

    public SpawnObjects(ushort[] _ids, string[] _prefabIds, Vector3[] _position, Quaternion[] _rotation)
    {
        ids = _ids;
        prefabIds = _prefabIds;
        position = _position;
        rotation = _rotation;
    }

    public void Deserialize(DeserializeEvent e)
    {
        ids = e.Reader.ReadUInt16s();
        prefabIds = e.Reader.ReadStrings();
        position = e.Reader.ReadVector3s(ids.Length);
        rotation = e.Reader.ReadQuaternionCompresseds(ids.Length);
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(ids);
        e.Writer.Write(prefabIds);
        e.Writer.WriteVector3s(position);
        e.Writer.WriteQuaternionCompresseds(rotation);
    }
}

public struct ObjectResponse : IDarkRiftSerializable
{
    ushort id;

    public ObjectResponse(ushort _id)
    {
        id = _id;
    }

    public void Deserialize(DeserializeEvent e)
    {
        id = e.Reader.ReadUInt16();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(id);
    }
}