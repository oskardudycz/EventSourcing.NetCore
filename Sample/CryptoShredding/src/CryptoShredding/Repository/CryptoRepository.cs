using System;
using System.Collections.Generic;

namespace CryptoShredding.Repository;

public class CryptoRepository
{
    private readonly IDictionary<string, EncryptionKey> _cryptoStore;

    public CryptoRepository()
    {
        _cryptoStore = new Dictionary<string, EncryptionKey>();
    }

    public EncryptionKey GetExistingOrNew(string id, Func<EncryptionKey> keyGenerator)
    {
        if (_cryptoStore.TryGetValue(id, out var keyStored))
            return keyStored;

        var newEncryptionKey = keyGenerator.Invoke();
        _cryptoStore.Add(id, newEncryptionKey);
        return newEncryptionKey;
    }

    public EncryptionKey? GetExistingOrDefault(string id)
    {
        return _cryptoStore.TryGetValue(id, out var keyStored) ? keyStored : default;
    }

    public void DeleteEncryptionKey(string id)
    {
        _cryptoStore.Remove(id);
    }
}
