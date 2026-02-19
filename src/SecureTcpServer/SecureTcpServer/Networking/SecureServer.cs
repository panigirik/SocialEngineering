using System.Net;
using System.Net.Sockets;
using SecureTcpServer.Security;
using SecureTcpServer.Utils;

namespace SecureTcpServer.Networking;

public class SecureServer
{
    private readonly TcpListener _listener;
    private readonly RateLimiter _rateLimiter = new();
    private int _activeConnections;

    public SecureServer(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
    }

    public async Task StartAsync()
    {
        _listener.Start();
        Console.WriteLine("Secure TCP Server started.");

        while (true)
        {
            var client = await _listener.AcceptTcpClientAsync();

            if (_activeConnections >= ProtocolConstants.MaxConnections)
            {
                client.Close();
                continue;
            }

            var ip = ((IPEndPoint)client.Client.RemoteEndPoint!).Address.ToString();

            if (!_rateLimiter.TryAdd(ip))
            {
                client.Close();
                continue;
            }

            _activeConnections++;

            _ = HandleClientAsync(client, ip);
        }
    }

    private async Task HandleClientAsync(TcpClient client, string ip)
    {
        try
        {
            using var session = new ClientSession(client);
            await session.ProcessAsync();
        }
        finally
        {
            _activeConnections--;
            _rateLimiter.Remove(ip);
        }
    }
}