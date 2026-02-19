using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;

namespace SecureTcpClient
{
    internal class Program
    {
        private const string Host = "127.0.0.1";
        private const int Port = 5000;
        private const string SecretKey = "SuperSecretKey123";

        static async Task Main()
        {
            Console.WriteLine("Starting attack simulation...\n");

            List<Task> tasks = new List<Task>();

            for (int i = 0; i < 20; i++)
            {
                tasks.Add(Task.Run(() => ConnectAndSendAsync(i)));
            }

            await Task.WhenAll(tasks);

            Console.WriteLine("\nSimulation finished.");
        }

        private static async Task ConnectAndSendAsync(int id)
        {
            try
            {
                using TcpClient client = new TcpClient();
                await client.ConnectAsync(Host, Port);

                using NetworkStream stream = client.GetStream();

                string payload = $"Message_{id}";
                string nonce = Guid.NewGuid().ToString();
                string data = $"{payload}|{nonce}";
                string hmac = ComputeHmac(data);

                string fullMessage = $"{payload}|{nonce}|{hmac}";
                byte[] bytes = Encoding.UTF8.GetBytes(fullMessage);

                await stream.WriteAsync(bytes, 0, bytes.Length);

                byte[] buffer = new byte[1024];
                int read = await stream.ReadAsync(buffer, 0, buffer.Length);

                if (read > 0)
                {
                    string response = Encoding.UTF8.GetString(buffer, 0, read);
                    Console.WriteLine($"Client {id}: {response.Trim()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client {id}: BLOCKED or FAILED ({ex.Message})");
            }
        }

        private static string ComputeHmac(string data)
        {
            using HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SecretKey));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToHexString(hash);
        }
    }
}
