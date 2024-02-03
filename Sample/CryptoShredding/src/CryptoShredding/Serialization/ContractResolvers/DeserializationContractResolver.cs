using System.Security.Cryptography;
using CryptoShredding.Serialization.JsonConverters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CryptoShredding.Serialization.ContractResolvers;

public class DeserializationContractResolver
    : DefaultContractResolver
{
    private readonly ICryptoTransform? _decryptor;
    private readonly FieldEncryptionDecryption _fieldEncryptionDecryption;

    public DeserializationContractResolver(
        ICryptoTransform? decryptor,
        FieldEncryptionDecryption fieldEncryptionDecryption)
    {
        _decryptor = decryptor;
        _fieldEncryptionDecryption = fieldEncryptionDecryption;
        NamingStrategy = new CamelCaseNamingStrategy();
    }

    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var properties = base.CreateProperties(type, memberSerialization);
        foreach (var jsonProperty in properties)
        {
            var isSimpleValue = IsSimpleValue(type, jsonProperty);
            if (!isSimpleValue) continue;

            var jsonConverter = new DecryptionJsonConverter(_decryptor, _fieldEncryptionDecryption);
            jsonProperty.Converter = jsonConverter;
        }
        return properties;
    }

    private bool IsSimpleValue(Type type, JsonProperty jsonProperty)
    {
        var propertyInfo = type.GetProperty(jsonProperty.UnderlyingName!);
        if (propertyInfo is null)
            return false;

        var propertyType = propertyInfo.PropertyType;
        var isSimpleValue = propertyType.IsValueType || propertyType == typeof(string);
        return isSimpleValue;
    }
}
