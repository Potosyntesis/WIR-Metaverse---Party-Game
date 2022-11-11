using DarkRift;
using DarkriftSerializationExtensions;
using UnityEngine;

public enum Tags
{
    SpawnRequest,
    SpawnResponse,
    Position
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
public struct PositionRotationPayload : IDarkRiftSerializable
{
    public Vector3 position;
    public Quaternion rotation;

    public PositionRotationPayload(Vector3 _position, Quaternion _rotation)
    {
        position = _position;
        rotation = _rotation;
    }

    public void Deserialize(DeserializeEvent e)
    {
        position = e.Reader.ReadVector3();
        rotation = e.Reader.ReadQuaternionCompressed();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.WriteVector3(position);
        e.Writer.WriteQuaternionCompressed(rotation);
    }
}


