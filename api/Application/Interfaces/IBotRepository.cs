using Domain.Entities;

namespace Application.Interfaces;

public interface IBotRepository
{
    Task<Bot?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<Bot?> GetByNumberAsync(string botNumber, CancellationToken ct = default);
    Task<IEnumerable<Bot>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Bot bot, CancellationToken ct = default);
    Task UpdateAsync(Bot bot, CancellationToken ct = default);
}
