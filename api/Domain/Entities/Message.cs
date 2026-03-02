namespace Domain.Entities;

public class Message
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string BotId { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty; // "user" | "bot"
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public User? User { get; set; }
    public Bot? Bot { get; set; }
}
