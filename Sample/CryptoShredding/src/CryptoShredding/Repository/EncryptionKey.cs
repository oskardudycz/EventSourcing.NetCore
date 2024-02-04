namespace CryptoShredding.Repository;

public record EncryptionKey(
    byte[] Key,
    byte[] Nonce
);
