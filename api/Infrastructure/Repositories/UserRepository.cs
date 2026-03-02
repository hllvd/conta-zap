using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(string id, CancellationToken ct = default) =>
        db.Users.Include(u => u.Bot).FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<User?> GetByWhatsAppNumberAsync(string number, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.WhatsAppNumber == number, ct);

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default) =>
        await db.Users.Include(u => u.Bot).ToListAsync(ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        db.Users.Update(user);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var user = await db.Users.FindAsync([id], ct);
        if (user is not null)
        {
            db.Users.Remove(user);
            await db.SaveChangesAsync(ct);
        }
    }
}
