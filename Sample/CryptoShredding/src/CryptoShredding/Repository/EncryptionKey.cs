namespace CryptoShredding.Repository;

public class EncryptionKey(
    byte[] key,
    byte[] nonce)
{
    public byte[] Key { get; } = key;
    public byte[] Nonce { get; } = nonce;
}
