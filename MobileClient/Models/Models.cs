namespace EncryptItVC.MobileClient.Models;

public class Channel
{
    public string Name { get; set; } = "";
    public string Owner { get; set; } = "";
    public bool IsPrivate { get; set; }
    public bool HasPassword { get; set; }
    public int UserCount { get; set; }
    public List<User> Users { get; set; } = new();
}

public class User
{
    public string Username { get; set; } = "";
    public bool IsAdmin { get; set; }
    public bool CanCreateChannels { get; set; }
    public VoiceStatus VoiceStatus { get; set; }
    public string CurrentChannel { get; set; } = "";
}

public enum VoiceStatus
{
    Active,
    Muted,
    Deafened,
    Disconnected
}

public class ChatMessage
{
    public string Username { get; set; } = "";
    public string Message { get; set; } = "";
    public string Channel { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.Now;

    public override string ToString()
    {
        return $"[{Timestamp:HH:mm}] {Username}: {Message}";
    }
}
