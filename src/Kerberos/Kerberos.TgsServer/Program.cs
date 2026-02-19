using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Common;

namespace Kerberos.TgsServer;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("TGS Server started");

        var listener = new TcpListener(IPAddress.Loopback, 5001);
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
            var ticket = JsonSerializer.Deserialize<Ticket>(msg.Payload)!;

            if (ticket.ExpireAt < DateTime.UtcNow)
            {
                await Send(stream, new Message { Type = "ERR", Payload = "TGT expired" });
                return;
            }

            var serviceTicket = new Ticket
            {
                Username = ticket.Username,
                Target = "SERVICE",
                SessionKey = CryptoUtils.GenerateKey(),
                ExpireAt = DateTime.UtcNow.AddMinutes(5)
            };

            await Send(stream, new Message { Type = "SERVICE_TICKET", Payload = JsonSerializer.Serialize(serviceTicket) });
        }

        async Task Send(NetworkStream s, Message m)
        {
            var data = Encoding.UTF8.GetBytes(m.Serialize());
            await s.WriteAsync(data);
        }
    }
}