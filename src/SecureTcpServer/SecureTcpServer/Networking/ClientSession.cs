using System.Net.Sockets;
using System.Text;
using SecureTcpServer.Protocol;
using SecureTcpServer.Security;

namespace SecureTcpServer.Networking;

public class ClientSession : IDisposable
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;

    public ClientSession(TcpClient client)
    {
        _client = client;
        _stream = client.GetStream();
        _stream.ReadTimeout = ProtocolConstants.AuthTimeoutSeconds * 1000;
    }

    public async Task ProcessAsync()
    {
        var buffer = new byte[ProtocolConstants.MaxMessageSize];

        int bytesRead = await _stream.ReadAsync(buffer);

        if (bytesRead <= 0)
        {
            return;
        }

        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        if (!ProtocolHandler.ValidateMessage(message))
        {
            return;
        }

        string response = "OK";
        var signed = ProtocolHandler.CreateSignedMessage(response);

        var data = Encoding.UTF8.GetBytes(signed);
        await _stream.WriteAsync(data);
    }

    public void Dispose()
    {
        _stream.Dispose();
        _client.Dispose();
    }
}