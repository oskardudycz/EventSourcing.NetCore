using System.Security.Cryptography;
using Newtonsoft.Json;

namespace CryptoShredding.Serialization.JsonConverters;

public class DecryptionJsonConverter
    : JsonConverter
{
    private readonly ICryptoTransform? _decryptor;
    private readonly FieldEncryptionDecryption _fieldEncryptionDecryption;

    public DecryptionJsonConverter(
        ICryptoTransform? decryptor,
        FieldEncryptionDecryption fieldEncryptionService)
    {
        _decryptor = decryptor;
        _fieldEncryptionDecryption = fieldEncryptionService;
    }

    public override void WriteJson(JsonWriter writer, object? value, Newtonsoft.Json.JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, Newtonsoft.Json.JsonSerializer serializer)
    {
        var value = reader.Value;

        if (value == null)
            return value;

        var result = _fieldEncryptionDecryption.GetDecryptedOrDefault(value, _decryptor, objectType);
        return result;
    }

    public override bool CanConvert(Type objectType)
    {
        return true;
    }

    public override bool CanRead => true;
    public override bool CanWrite => false;
}
