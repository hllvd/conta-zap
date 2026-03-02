using Application.DTOs;
using Application.Interfaces;
using Application.Services;

namespace Api.Endpoints;

public static class WebhookEndpoints
{
    public static void MapWebhookEndpoints(this WebApplication app, IConfiguration config)
    {
        app.MapPost("/api/webhook/{botNumber}", async (
            string botNumber,
            WebhookRequestDto dto,
            HttpContext httpContext,
            BotService botService,
            UserService userService,
            MessageService messageService,
            IUserRepository userRepository,
            IBotStrategyFactory strategyFactory) =>
        {
            // 1. Validate webhook secret
            var expectedSecret = config["Webhook:Secret"];
            if (!string.IsNullOrEmpty(expectedSecret))
            {
                var providedSecret = httpContext.Request.Headers["X-Webhook-Secret"].FirstOrDefault();
                if (providedSecret != expectedSecret)
                    return Results.Unauthorized();
            }

            // 2. Identify bot
            var bot = await botService.GetByNumberAsync(botNumber);
            if (bot is null || !bot.IsActive)
                return Results.NotFound(new { error = "Bot not found or inactive." });

            // 3. Identify or create user (first contact scenario)
            var user = await userRepository.GetByWhatsAppNumberAsync(dto.From);
            if (user is null)
            {
                // Auto-create user on first contact with this bot number
                try
                {
                    await userService.CreateAsync(new CreateUserDto(
                        Name: dto.From,
                        Email: $"{dto.From.TrimStart('+')}@whatsapp.local",
                        Password: Guid.NewGuid().ToString(),
                        WhatsAppNumber: dto.From,
                        BotId: bot.Id));
                    user = await userRepository.GetByWhatsAppNumberAsync(dto.From);
                }
                catch
                {
                    return Results.Problem("Could not register user.");
                }
            }

            if (user is null)
                return Results.Problem("User lookup failed.");

            // 4. Store incoming message
            await messageService.StoreAsync(user.Id, bot.Id, "user", dto.Message);

            // 5. Resolve strategy and process
            string reply;
            try
            {
                var strategy = strategyFactory.Resolve(user.BotType);
                reply = await strategy.ProcessMessageAsync(user, dto.Message);
            }
            catch (NotSupportedException)
            {
                reply = "Bot type not supported yet.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Bot processing failed: {ex.Message}");
                reply = "Desculpe, ocorreu um erro. Tente novamente. 🙏";
            }

            // 6. Store bot reply
            await messageService.StoreAsync(user.Id, bot.Id, "bot", reply);

            // 7. Return response (WhatsApp provider will deliver it)
            return Results.Ok(new { reply });
        });
    }
}
