using Domain.Entities;

namespace Application.Interfaces;

public interface IMessageRepository
{
    Task<IEnumerable<Message>> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<IEnumerable<Message>> GetByBotIdAsync(string botId, int limit = 50, CancellationToken ct = default);
    Task AddAsync(Message message, CancellationToken ct = default);
}
