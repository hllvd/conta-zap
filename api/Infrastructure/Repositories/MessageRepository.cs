using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class MessageRepository(AppDbContext db) : IMessageRepository
{
    public async Task<IEnumerable<Message>> GetByUserIdAsync(string userId, CancellationToken ct = default) =>
        await db.Messages
            .Where(m => m.UserId == userId)
            .OrderBy(m => m.Timestamp)
            .ToListAsync(ct);

    public async Task<IEnumerable<Message>> GetByBotIdAsync(string botId, int limit = 50, CancellationToken ct = default) =>
        await db.Messages
            .Where(m => m.BotId == botId)
            .OrderByDescending(m => m.Timestamp)
            .Take(limit)
            .OrderBy(m => m.Timestamp)
            .ToListAsync(ct);

    public async Task AddAsync(Message message, CancellationToken ct = default)
    {
        db.Messages.Add(message);
        await db.SaveChangesAsync(ct);
    }
}
