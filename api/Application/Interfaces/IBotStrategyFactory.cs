using Domain.Enums;

namespace Application.Interfaces;

public interface IBotStrategyFactory
{
    IBotStrategy Resolve(BotType type);
}
