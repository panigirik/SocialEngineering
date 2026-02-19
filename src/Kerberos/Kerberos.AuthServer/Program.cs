using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Common;

namespace Kerberos;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Auth Server started");

        var listener = new TcpListener(IPAddress.Loopback, 5000);
        listener.Start();

        while (true)
        {
            var client = await listener.AcceptTcpClientAsync();
            _ = Task.Run(() => Handle(client));
        }

        async Task Handle(TcpClient client)
        {
            using var stream = client.GetStream();
            var buffer = new byte[4096];
            var read = await stream.ReadAsync(buffer);

            var msg = Message.Deserialize(Encoding.UTF8.GetString(buffer, 0, read));
            var parts = msg.Payload.Split(':');

            var username = parts[0];
            var password = parts[1];

            if (!UserStore.Validate(username, password))
            {
                await Send(stream, new Message { Type = "ERR", Payload = "Invalid credentials" });
                return;
            }

            var tgt = new Ticket
            {
                Username = username,
                Target = "TGS",
                SessionKey = CryptoUtils.GenerateKey(),
                ExpireAt = DateTime.UtcNow.AddMinutes(5)
            };

            await Send(stream, new Message { Type = "TGT", Payload = JsonSerializer.Serialize(tgt) });
        }

        async Task Send(NetworkStream s, Message m)
        {
            var data = Encoding.UTF8.GetBytes(m.Serialize());
            await s.WriteAsync(data);
        }
    }
}