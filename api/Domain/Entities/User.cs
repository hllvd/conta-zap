using Domain.Enums;

namespace Domain.Entities;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string WhatsAppNumber { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public BotType BotType { get; set; }
    public string? GoogleSheetId { get; set; }
    public string? BotId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Bot? Bot { get; set; }
    public ICollection<Message> Messages { get; set; } = [];
}
