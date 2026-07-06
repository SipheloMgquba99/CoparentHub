namespace CoparentHub.Domain.Common
{
    public interface IFieldEncryptor
    {
        string? Encrypt(string? plaintext);
        string? Decrypt(string? ciphertext);
    }
}
