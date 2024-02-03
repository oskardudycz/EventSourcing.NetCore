namespace CryptoShredding.Serialization;

public class SerializedEvent
{
    public byte[] Data { get; }
    public byte[] MetaData { get; }
    public bool IsJson { get; }
        
    public SerializedEvent(byte[] data, byte[] metaData, bool isJson)
    {
        Data = data;
        MetaData = metaData;
        IsJson = isJson;
    }
}