using Application.Services;
using Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Integration;

/// <summary>
/// Shared test factory that replaces SQLite with InMemory EF Core
/// so tests remain isolated and fast.
/// </summary>
public class ContaZapTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real AppDbContext registration
            var descriptor = services
                .SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // Replace with in-memory DB
            services.AddDbContext<AppDbContext>(opt =>
                opt.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));
        });

        builder.UseEnvironment("Test");
    }
}

// ── Tests ─────────────────────────────────────────────────────────

public class HealthEndpointTests(ContaZapTestFactory factory)
    : IClassFixture<ContaZapTestFactory>
{
    [Fact]
    public async Task Health_ReturnsOk()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

public class AuthEndpointTests(ContaZapTestFactory factory)
    : IClassFixture<ContaZapTestFactory>
{
    [Fact]
    public async Task Login_WithSuperAdmin_ReturnsToken()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { Email = "superadmin@test.com", Password = "string" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(body?.AccessToken);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { Email = "superadmin@test.com", Password = "wrong" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UsersEndpoint_WithoutAuth_Returns401()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private record TokenResponse(string AccessToken, string RefreshToken, int ExpiresIn);
}

public class BotEndpointTests(ContaZapTestFactory factory)
    : IClassFixture<ContaZapTestFactory>
{
    private async Task<string> GetAdminTokenAsync()
    {
        var client = factory.CreateClient();
        var res = await client.PostAsJsonAsync("/api/auth/login",
            new { Email = "superadmin@test.com", Password = "string" });
        var body = await res.Content.ReadFromJsonAsync<TokenResponse>()!;
        return body!.AccessToken;
    }

    [Fact]
    public async Task GetBots_AsAdmin_ReturnsEmptyList()
    {
        var client = factory.CreateClient();
        var token = await GetAdminTokenAsync();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/bots");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private record TokenResponse(string AccessToken, string RefreshToken, int ExpiresIn);
}
