using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Bot> Bots => Set<Bot>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Bot ──
        modelBuilder.Entity<Bot>(b =>
        {
            b.HasKey(e => e.Id);
            b.HasIndex(e => e.BotNumber).IsUnique();
            b.Property(e => e.BotType).HasConversion<string>();
            b.Property(e => e.CreatedAt).HasConversion(
                v => v.ToString("O"), v => DateTimeOffset.Parse(v));
            b.Property(e => e.UpdatedAt).HasConversion(
                v => v.ToString("O"), v => DateTimeOffset.Parse(v));
        });

        // ── User ──
        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(e => e.Id);
            b.HasIndex(e => e.Email).IsUnique();
            b.HasIndex(e => e.WhatsAppNumber).IsUnique();
            b.Property(e => e.Role).HasConversion<string>();
            b.Property(e => e.BotType).HasConversion<string>();
            b.Property(e => e.CreatedAt).HasConversion(
                v => v.ToString("O"), v => DateTimeOffset.Parse(v));
            b.Property(e => e.UpdatedAt).HasConversion(
                v => v.ToString("O"), v => DateTimeOffset.Parse(v));
            b.HasOne(e => e.Bot)
             .WithMany(b => b.Users)
             .HasForeignKey(e => e.BotId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Message ──
        modelBuilder.Entity<Message>(b =>
        {
            b.HasKey(e => e.Id);
            b.HasIndex(e => e.UserId);
            b.HasIndex(e => e.BotId);
            b.HasIndex(e => e.Timestamp);
            b.Property(e => e.Timestamp).HasConversion(
                v => v.ToString("O"), v => DateTimeOffset.Parse(v));
            b.HasOne(e => e.User)
             .WithMany(u => u.Messages)
             .HasForeignKey(e => e.UserId)
             .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(e => e.Bot)
             .WithMany(b => b.Messages)
             .HasForeignKey(e => e.BotId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
