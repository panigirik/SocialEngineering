using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Concurrent;

namespace SecureTcpServer
{
    internal class Program
    {
        private const int Port = 5000;
        private const int MaxConnections = 5;
        private const int MaxConnectionsPerIp = 3;
        private const string SecretKey = "SuperSecretKey123";

        private static int _activeConnections = 0;
        private static readonly ConcurrentDictionary<string, int> _ipConnections = new();
        private static readonly ConcurrentDictionary<string, DateTime> _blockedIps = new();

        static async Task Main()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();

            Console.WriteLine($"Secure TCP Server started on port {Port}");

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleClientAsync(client));
            }
        }

        private static async Task HandleClientAsync(TcpClient client)
        {
            string ip = ((IPEndPoint)client.Client.RemoteEndPoint!).Address.ToString();

            if (IsBlocked(ip))
            {
                client.Close();
                return;
            }

            if (_activeConnections >= MaxConnections)
            {
                BlockIp(ip);
                client.Close();
                return;
            }

            int ipCount = _ipConnections.AddOrUpdate(ip, 1, (_, count) => count + 1);

            if (ipCount > MaxConnectionsPerIp)
            {
                BlockIp(ip);
                client.Close();
                return;
            }

            Interlocked.Increment(ref _activeConnections);

            try
            {
                using NetworkStream stream = client.GetStream();
                stream.ReadTimeout = 5000;

                byte[] buffer = new byte[4096];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead <= 0)
                {
                    return;
                }

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                bool valid = ValidateMessage(message);

                string response = valid ? "VALID\n" : "INVALID\n";
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
            }
            catch
            {
            }
            finally
            {
                _ipConnections.AddOrUpdate(ip, 0, (_, count) => Math.Max(0, count - 1));
                Interlocked.Decrement(ref _activeConnections);
                client.Close();
            }
        }

        private static bool ValidateMessage(string message)
        {
            string[] parts = message.Split('|');

            if (parts.Length != 3)
            {
                return false;
            }

            string payload = parts[0];
            string nonce = parts[1];
            string receivedHmac = parts[2];

            string computedHmac = ComputeHmac($"{payload}|{nonce}");

            if (receivedHmac != computedHmac)
            {
                return false;
            }

            return true;
        }

        private static string ComputeHmac(string data)
        {
            using HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SecretKey));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToHexString(hash);
        }

        private static bool IsBlocked(string ip)
        {
            if (_blockedIps.TryGetValue(ip, out DateTime blockedUntil))
            {
                if (DateTime.UtcNow < blockedUntil)
                {
                    Console.WriteLine($"IP {ip} is blocked.");
                    return true;
                }

                _blockedIps.TryRemove(ip, out _);
            }

            return false;
        }

        private static void BlockIp(string ip)
        {
            _blockedIps[ip] = DateTime.UtcNow.AddSeconds(30);
            Console.WriteLine($"IP {ip} blocked for 30 seconds.");
        }
    }
}
