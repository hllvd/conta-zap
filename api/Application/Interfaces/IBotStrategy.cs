using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces;

public interface IBotStrategy
{
    BotType SupportedType { get; }
    Task<string> ProcessMessageAsync(User user, string message, CancellationToken ct = default);
}
