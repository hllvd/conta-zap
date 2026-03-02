using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.BotEngine;
using Infrastructure.Data;
using Infrastructure.Google;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration ──────────────────────────────────────────────────────────
builder.Configuration.AddJsonFile("config.json", optional: false, reloadOnChange: true);
var config = builder.Configuration;

// ── Database ───────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(config["Database:ConnectionString"]
        ?? "Data Source=/data/contazap.db"));

// ── Repositories ────────────────────────────────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBotRepository, BotRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();

// ── Google Sheets ────────────────────────────────────────────────────────────
builder.Services.AddScoped<IGoogleSheetService, GoogleSheetsClient>();

// ── Bot strategies (Strategy Pattern) ───────────────────────────────────────
builder.Services.AddScoped<IBotStrategy, PersonalFinanceBotStrategy>();
builder.Services.AddScoped<IBotStrategy, MeiBotStrategy>();
builder.Services.AddScoped<IBotStrategy, AgroBotStrategy>();
builder.Services.AddScoped<IBotStrategyFactory, BotStrategyFactory>();

// ── Application services ─────────────────────────────────────────────────────
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<BotService>();
builder.Services.AddScoped<MessageService>();

// ── JWT Authentication ────────────────────────────────────────────────────────
var jwtKey = config["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("Jwt:SecretKey not configured.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// ── CORS ─────────────────────────────────────────────────────────────────────
var allowedOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("ContaZapCors", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// ── Middleware ────────────────────────────────────────────────────────────────
app.UseCors("ContaZapCors");
app.UseAuthentication();
app.UseAuthorization();

// ── Seed & Migrate ─────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await SeedSuperAdmin(db, config);
}

// ── Endpoints ─────────────────────────────────────────────────────────────────
app.MapHealthEndpoints();
app.MapAuthEndpoints(config);
app.MapUserEndpoints();
app.MapBotEndpoints();
app.MapWebhookEndpoints(config);
app.MapTestEndpoints();

app.Run();

// ── Seeder ────────────────────────────────────────────────────────────────────
static async Task SeedSuperAdmin(AppDbContext db, IConfiguration config)
{
    var email = config["SuperAdmin:Email"] ?? "superadmin@test.com";
    if (await db.Users.AnyAsync(u => u.Email == email)) return;

    var password = config["SuperAdmin:Password"] ?? "string";
    db.Users.Add(new User
    {
        Name = "Super Admin",
        Email = email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
        WhatsAppNumber = "+5500000000000",
        Role = UserRole.Admin,
        BotType = BotType.PersonalFinance
    });
    await db.SaveChangesAsync();
    Console.WriteLine($"[SEED] Super admin created: {email}");
}
