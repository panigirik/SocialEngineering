using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Common;

namespace Kerberos.Client2;

class Program
{
    static void Main()
    {
        Console.Write("Login: ");
        var user = Console.ReadLine()!;
        Console.Write("Password: ");
        var pass = Console.ReadLine()!;

        var tgt = RequestTicket(5000, $"{user}:{pass}", "TGT");

        var serviceTicket = RequestTicket(5001, JsonSerializer.Serialize(tgt), "SERVICE_TICKET");

        Console.Write("Message: ");
        var msg = Console.ReadLine()!;
        
        SendMessage(5002, $"{JsonSerializer.Serialize(serviceTicket)}|{msg}");

        Ticket RequestTicket(int port, string payload, string expect)
        {
            using var client = new TcpClient("127.0.0.1", port);
            using var stream = client.GetStream();

            var data = Encoding.UTF8.GetBytes(new Message { Type = "REQ", Payload = payload }.Serialize());
            stream.Write(data);

            var buffer = new byte[4096];
            var read = stream.Read(buffer);
            var resp = Message.Deserialize(Encoding.UTF8.GetString(buffer, 0, read));

            return JsonSerializer.Deserialize<Ticket>(resp.Payload)!;
        }

        void SendMessage(int port, string payload)
        {
            using var client = new TcpClient("127.0.0.1", port);
            using var stream = client.GetStream();

            var data = Encoding.UTF8.GetBytes(new Message { Type = "MSG", Payload = payload }.Serialize());
            stream.Write(data);
        }
    }
}