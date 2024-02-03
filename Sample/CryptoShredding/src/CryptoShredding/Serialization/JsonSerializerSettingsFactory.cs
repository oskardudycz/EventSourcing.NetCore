using CryptoShredding.Serialization.ContractResolvers;
using CryptoShredding.Serialization.JsonConverters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CryptoShredding.Serialization;

public class JsonSerializerSettingsFactory
{
    private readonly EncryptorDecryptor _encryptorDecryptor;

    public JsonSerializerSettingsFactory(EncryptorDecryptor encryptorDecryptor)
    {
        _encryptorDecryptor = encryptorDecryptor;
    }
        
    public JsonSerializerSettings CreateDefault()
    {
        var defaultContractResolver = new CamelCasePropertyNamesContractResolver();
        var defaultSettings = GetSettings(defaultContractResolver);
        return defaultSettings;
    }
        
    public JsonSerializerSettings CreateForEncryption(string dataSubjectId)
    {
        var encryptor = _encryptorDecryptor.GetEncryptor(dataSubjectId);
        var fieldEncryptionDecryption = new FieldEncryptionDecryption();
        var serializationContractResolver = 
            new SerializationContractResolver(encryptor, fieldEncryptionDecryption);
        var jsonSerializerSettings = GetSettings(serializationContractResolver);
        return jsonSerializerSettings;
    }
        
    public JsonSerializerSettings CreateForDecryption(string dataSubjectId)
    {
        var decryptor = _encryptorDecryptor.GetDecryptor(dataSubjectId);
        var fieldEncryptionDecryption = new FieldEncryptionDecryption();
        var deserializationContractResolver = 
            new DeserializationContractResolver(decryptor, fieldEncryptionDecryption);
        var jsonDeserializerSettings = GetSettings(deserializationContractResolver);
        return jsonDeserializerSettings;
    }
        
    private JsonSerializerSettings GetSettings(IContractResolver contractResolver)
    {
        var settings =
            new JsonSerializerSettings
            {
                ContractResolver = contractResolver
            };
        return settings;
    }
}