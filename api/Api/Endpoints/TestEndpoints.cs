using Application.DTOs;
using Application.Interfaces;
using Application.Services;

namespace Api.Endpoints;

/// <summary>
/// Local test endpoint — simulates a WhatsApp interaction without requiring a real phone.
/// Admin-only. Uses the same pipeline as the webhook.
/// </summary>
public static class TestEndpoints
{
    public static void MapTestEndpoints(this WebApplication app)
    {
        // POST /api/test/chat
        app.MapPost("/api/test/chat", async (
            ChatRequestDto dto,
            BotService botService,
            UserService userService,
            MessageService messageService,
            IUserRepository userRepository,
            IBotStrategyFactory strategyFactory) =>
        {
            // Resolve bot
            var bot = await botService.GetByNumberAsync(dto.BotNumber);
            if (bot is null || !bot.IsActive)
                return Results.NotFound(new { error = "Bot not found or inactive." });

            // Get user by WhatsApp number
            var user = await userRepository.GetByWhatsAppNumberAsync(dto.UserWhatsApp);
            if (user is null)
                return Results.NotFound(new { error = $"User with WhatsApp '{dto.UserWhatsApp}' not found." });

            // Store test message
            await messageService.StoreAsync(user.Id, bot.Id, "user", dto.Message);

            // Process via strategy
            string reply;
            try
            {
                var strategy = strategyFactory.Resolve(user.BotType);
                reply = await strategy.ProcessMessageAsync(user, dto.Message);
            }
            catch (Exception ex)
            {
                reply = $"[Test Error] {ex.Message}";
            }

            // Store reply
            await messageService.StoreAsync(user.Id, bot.Id, "bot", reply);

            return Results.Ok(new ChatResponseDto(reply, bot.BotName));
        }).RequireAuthorization("AdminOnly");

        // GET /api/test/history/{userWhatsApp}
        app.MapGet("/api/test/history/{userWhatsApp}", async (
            string userWhatsApp,
            IUserRepository userRepository,
            MessageService messageService) =>
        {
            var user = await userRepository.GetByWhatsAppNumberAsync(userWhatsApp);
            if (user is null) return Results.NotFound();
            var history = await messageService.GetByUserAsync(user.Id);
            return Results.Ok(history);
        }).RequireAuthorization("AdminOnly");
    }
}
