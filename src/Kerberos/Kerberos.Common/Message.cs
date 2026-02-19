using System.Text.Json;

namespace Common;

public class Message
{
    public string Type { get; set; } = "";
    public string Payload { get; set; } = "";

    public string Serialize() => JsonSerializer.Serialize(this);

    public static Message Deserialize(string json) => JsonSerializer.Deserialize<Message>(json)!;
}