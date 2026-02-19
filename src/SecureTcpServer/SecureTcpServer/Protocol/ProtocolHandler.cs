using SecureTcpServer.Security;

namespace SecureTcpServer.Protocol;

public static class ProtocolHandler
{
    public static bool ValidateMessage(string message)
    {
        var parts = message.Split('|');

        if (parts.Length != 3)
        {
            return false;
        }

        var payload = parts[0];
        var nonce = parts[1];
        var hmac = parts[2];

        var combined = $"{payload}|{nonce}";

        return CryptoHelper.VerifyHmac(combined, hmac);
    }

    public static string CreateSignedMessage(string payload)
    {
        var nonce = Guid.NewGuid().ToString();
        var combined = $"{payload}|{nonce}";
        var hmac = CryptoHelper.ComputeHmac(combined);

        return $"{payload}|{nonce}|{hmac}";
    }
}