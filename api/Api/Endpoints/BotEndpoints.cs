using Application.DTOs;
using Application.Services;

namespace Api.Endpoints;

public static class BotEndpoints
{
    public static void MapBotEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/bots").RequireAuthorization("AdminOnly");

        // GET /api/bots
        group.MapGet("/", async (BotService botService) =>
        {
            var bots = await botService.GetAllAsync();
            return Results.Ok(bots);
        });

        // GET /api/bots/{id}
        group.MapGet("/{id}", async (string id, BotService botService) =>
        {
            var bot = await botService.GetByIdAsync(id);
            return bot is null ? Results.NotFound() : Results.Ok(bot);
        });

        // POST /api/bots
        group.MapPost("/", async (CreateBotDto dto, BotService botService) =>
        {
            var bot = await botService.CreateAsync(dto);
            return Results.Created($"/api/bots/{bot.Id}", bot);
        });

        // PATCH /api/bots/{id}/toggle
        group.MapPatch("/{id}/toggle", async (string id, BotService botService) =>
        {
            var result = await botService.ToggleActiveAsync(id);
            return result ? Results.Ok(new { message = "Bot status toggled." }) : Results.NotFound();
        });
    }
}
