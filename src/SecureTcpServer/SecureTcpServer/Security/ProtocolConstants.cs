namespace SecureTcpServer.Security;

public static class ProtocolConstants
{
    public const int MaxConnections = 50;
    public const int AuthTimeoutSeconds = 10;
    public const int MaxMessageSize = 4096;
    public const string SharedSecret = "VeryStrongSharedSecretKey";
}