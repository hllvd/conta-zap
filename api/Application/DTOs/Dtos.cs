using Domain.Enums;

namespace Application.DTOs;

public record LoginDto(string Email, string Password);

public record TokenResponseDto(string AccessToken, string RefreshToken, int ExpiresIn);

public record CreateUserDto(
    string Name,
    string Email,
    string Password,
    string WhatsAppNumber,
    string BotId);

public record UpdateUserDto(
    string? Name,
    string? Email,
    string? WhatsAppNumber);

public record UserDto(
    string Id,
    string Name,
    string Email,
    string WhatsAppNumber,
    UserRole Role,
    BotType BotType,
    string? GoogleSheetId,
    string BotId,
    DateTimeOffset CreatedAt);

public record BotDto(
    string Id,
    string BotNumber,
    string BotName,
    BotType BotType,
    string PersonalityPrompt,
    string SetupMessage,
    string SheetTemplateId,
    bool IsActive,
    DateTimeOffset CreatedAt);

public record CreateBotDto(
    string BotNumber,
    string BotName,
    BotType BotType,
    string PersonalityPrompt,
    string SetupMessage,
    string SheetTemplateId);

public record MessageDto(
    string Id,
    string UserId,
    string BotId,
    string Sender,
    string Content,
    DateTimeOffset Timestamp);

public record WebhookRequestDto(string From, string Message);

public record ChatRequestDto(
    string UserWhatsApp,
    string BotNumber,
    string Message);

public record ChatResponseDto(string Reply, string BotName);
