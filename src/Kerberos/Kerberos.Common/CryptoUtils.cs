using System.Security.Cryptography;
using System.Text;

namespace Common;

public static class CryptoUtils
{
    public static string GenerateKey()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));

    public static string Hash(string value)
    {
        using var sha = SHA256.Create();
        return Convert.ToBase64String(
            sha.ComputeHash(Encoding.UTF8.GetBytes(value))
        );
    }
}