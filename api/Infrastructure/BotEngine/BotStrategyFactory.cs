using Application.Interfaces;
using Domain.Enums;

namespace Infrastructure.BotEngine;

public class BotStrategyFactory(IEnumerable<IBotStrategy> strategies) : IBotStrategyFactory
{
    public IBotStrategy Resolve(BotType type) =>
        strategies.FirstOrDefault(s => s.SupportedType == type)
        ?? throw new NotSupportedException($"No strategy registered for bot type '{type}'.");
}
