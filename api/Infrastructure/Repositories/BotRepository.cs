using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class BotRepository(AppDbContext db) : IBotRepository
{
    public Task<Bot?> GetByIdAsync(string id, CancellationToken ct = default) =>
        db.Bots.FirstOrDefaultAsync(b => b.Id == id, ct);

    public Task<Bot?> GetByNumberAsync(string botNumber, CancellationToken ct = default) =>
        db.Bots.FirstOrDefaultAsync(b => b.BotNumber == botNumber, ct);

    public async Task<IEnumerable<Bot>> GetAllAsync(CancellationToken ct = default) =>
        await db.Bots.ToListAsync(ct);

    public async Task AddAsync(Bot bot, CancellationToken ct = default)
    {
        db.Bots.Add(bot);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Bot bot, CancellationToken ct = default)
    {
        db.Bots.Update(bot);
        await db.SaveChangesAsync(ct);
    }
}
