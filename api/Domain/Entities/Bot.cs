using Domain.Enums;

namespace Domain.Entities;

public class Bot
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string BotNumber { get; set; } = string.Empty;
    public string BotName { get; set; } = string.Empty;
    public BotType BotType { get; set; }
    public string PersonalityPrompt { get; set; } = string.Empty;
    public string SetupMessage { get; set; } = string.Empty;
    public string SheetTemplateId { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public ICollection<User> Users { get; set; } = [];
    public ICollection<Message> Messages { get; set; } = [];
}
