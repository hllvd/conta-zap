using Application.DTOs;
using Application.Services;
using Domain.Enums;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app, IConfiguration config)
    {
        app.MapPost("/api/auth/login", async (LoginDto dto, UserService userService) =>
        {
            var user = await userService.ValidateCredentialsAsync(dto.Email, dto.Password);
            if (user is null)
                return Results.Unauthorized();

            var token = GenerateJwt(user.Id, user.Email, user.Role.ToString(), config);
            var refreshToken = Guid.NewGuid().ToString("N");
            // TODO: persist refresh token to DB for production use
            return Results.Ok(new TokenResponseDto(token, refreshToken, 3600));
        });

        app.MapGet("/api/auth/me", (ClaimsPrincipal principal) =>
        {
            var id = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            var role = principal.FindFirst(ClaimTypes.Role)?.Value;
            return Results.Ok(new { id, email, role });
        }).RequireAuthorization();
    }

    public static string GenerateJwt(string userId, string email, string role, IConfiguration config)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["Jwt:SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = int.Parse(config["Jwt:ExpiryMinutes"] ?? "60");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiry),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
