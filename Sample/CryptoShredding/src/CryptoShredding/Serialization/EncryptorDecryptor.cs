using System.Security.Cryptography;
using CryptoShredding.Repository;

namespace CryptoShredding.Serialization;

public class EncryptorDecryptor
{
    private readonly CryptoRepository _cryptoRepository;

    public EncryptorDecryptor(CryptoRepository cryptoRepository)
    {
        _cryptoRepository = cryptoRepository;
    }

    public ICryptoTransform GetEncryptor(string dataSubjectId)
    {
        var encryptionKey = _cryptoRepository.GetExistingOrNew(dataSubjectId, CreateNewEncryptionKey);
        var aes = GetAes(encryptionKey);
        var encryptor = aes.CreateEncryptor();
        return encryptor;
    }

    public ICryptoTransform? GetDecryptor(string dataSubjectId)
    {
        var encryptionKey = _cryptoRepository.GetExistingOrDefault(dataSubjectId);
        if (encryptionKey is null)
        {
            // encryption key was deleted
            return default;
        }

        var aes = GetAes(encryptionKey);
        var decryptor = aes.CreateDecryptor();
        return decryptor;
    }

    private EncryptionKey CreateNewEncryptionKey()
    {
        var aes = Aes.Create();

        aes.Padding = PaddingMode.PKCS7;

        var key = aes.Key;
        var nonce = aes.IV;

        var encryptionKey = new EncryptionKey(key, nonce);
        return encryptionKey;
    }

    private Aes GetAes(EncryptionKey encryptionKey)
    {
        var aes = Aes.Create();

        aes.Padding = PaddingMode.PKCS7;
        aes.Key = encryptionKey.Key;
        aes.IV = encryptionKey.Nonce;

        return aes;
    }
}
