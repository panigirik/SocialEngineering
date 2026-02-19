using System.Collections.Concurrent;

namespace SecureTcpServer.Utils;

public class RateLimiter
{
    private readonly ConcurrentDictionary<string, int> _connections = new();

    public bool TryAdd(string ip)
    {
        int count = _connections.AddOrUpdate(ip, 1, (_, old) => old + 1);

        if (count > 5)
        {
            return false;
        }

        return true;
    }

    public void Remove(string ip)
    {
        _connections.AddOrUpdate(ip, 0, (_, old) => old > 0 ? old - 1 : 0);
    }
}