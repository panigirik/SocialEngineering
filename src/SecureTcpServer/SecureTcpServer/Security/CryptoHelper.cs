using System.Security.Cryptography;
using System.Text;

namespace SecureTcpServer.Security;


public static class CryptoHelper
{
    public static string ComputeHmac(string message)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(ProtocolConstants.SharedSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        return Convert.ToBase64String(hash);
    }

    public static bool VerifyHmac(string message, string receivedHmac)
    {
        var computed = ComputeHmac(message);
        return computed == receivedHmac;
    }
}