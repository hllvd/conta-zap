using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class MessageService(IMessageRepository messageRepository)
{
    public async Task<Message> StoreAsync(
        string userId, string botId, string sender, string content, CancellationToken ct = default)
    {
        var message = new Message
        {
            UserId = userId,
            BotId = botId,
            Sender = sender,
            Content = content
        };
        await messageRepository.AddAsync(message, ct);
        return message;
    }

    public async Task<IEnumerable<MessageDto>> GetByUserAsync(string userId, CancellationToken ct = default)
    {
        var messages = await messageRepository.GetByUserIdAsync(userId, ct);
        return messages.Select(MapToDto);
    }

    public async Task<IEnumerable<MessageDto>> GetByBotAsync(string botId, int limit = 50, CancellationToken ct = default)
    {
        var messages = await messageRepository.GetByBotIdAsync(botId, limit, ct);
        return messages.Select(MapToDto);
    }

    private static MessageDto MapToDto(Message m) =>
        new(m.Id, m.UserId, m.BotId, m.Sender, m.Content, m.Timestamp);
}
