using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Common;

namespace Kerberos.Service;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Service Server started");

        var listener = new TcpListener(IPAddress.Loopback, 5002);
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
            var parts = msg.Payload.Split('|');

            var ticket = JsonSerializer.Deserialize<Ticket>(parts[0])!;
            var message = parts[1];

            if (ticket.ExpireAt < DateTime.UtcNow)
            {
                Console.WriteLine("Expired ticket");
                return;
            }

            Console.WriteLine($"[{ticket.Username}] says: {message}");
        }
    }
}