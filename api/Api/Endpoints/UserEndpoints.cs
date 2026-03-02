using Application.DTOs;
using Application.Services;

namespace Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/users").RequireAuthorization();

        // GET /api/users  (Admin only)
        group.MapGet("/", async (UserService userService) =>
        {
            var users = await userService.GetAllAsync();
            return Results.Ok(users);
        }).RequireAuthorization("AdminOnly");

        // GET /api/users/{id}
        group.MapGet("/{id}", async (string id, UserService userService) =>
        {
            var user = await userService.GetByIdAsync(id);
            return user is null ? Results.NotFound() : Results.Ok(user);
        }).RequireAuthorization("AdminOnly");

        // POST /api/users  (Admin only)
        group.MapPost("/", async (CreateUserDto dto, UserService userService) =>
        {
            try
            {
                var user = await userService.CreateAsync(dto);
                return Results.Created($"/api/users/{user.Id}", user);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).RequireAuthorization("AdminOnly");

        // PUT /api/users/{id}
        group.MapPut("/{id}", async (string id, UpdateUserDto dto, UserService userService) =>
        {
            var user = await userService.UpdateAsync(id, dto);
            return user is null ? Results.NotFound() : Results.Ok(user);
        }).RequireAuthorization("AdminOnly");

        // DELETE /api/users/{id}
        group.MapDelete("/{id}", async (string id, UserService userService) =>
        {
            var deleted = await userService.DeleteAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        }).RequireAuthorization("AdminOnly");
    }
}
