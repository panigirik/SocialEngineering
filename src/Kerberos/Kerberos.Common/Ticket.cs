namespace Common;

public class Ticket
{
    public string Username { get; set; } = "";
    public string Target { get; set; } = "";
    public string SessionKey { get; set; } = "";
    public DateTime ExpireAt { get; set; }

    public string Serialize()
    {
        return System.Text.Json.JsonSerializer.Serialize(this);
    }

    public static Ticket Deserialize(string json)
    {
        return System.Text.Json.JsonSerializer.Deserialize<Ticket>(json)!;
    }
}