using LotoMln.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace LotoMln.DataAccess.DBContext;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionSlot> QuestionSlots => Set<QuestionSlot>();
    public DbSet<GameStateSnapshot> GameStates => Set<GameStateSnapshot>();
    public DbSet<CalledNumber> CalledNumbers => Set<CalledNumber>();
    public DbSet<StealAttempt> StealAttempts => Set<StealAttempt>();
    public DbSet<KinhClaim> KinhClaims => Set<KinhClaim>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // === Room ===
        mb.Entity<Room>(e =>
        {
            e.HasKey(r => r.Code);
            e.Property(r => r.Code).HasMaxLength(6);
            e.Property(r => r.State).HasConversion<string>().HasMaxLength(20);
        });

        // === Player ===
        mb.Entity<Player>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.RoomCode).HasMaxLength(6);
            e.Property(p => p.Name).HasMaxLength(15);
            e.Property(p => p.MarkedNumbers).HasColumnType("jsonb");

            e.HasOne(p => p.Room)
             .WithMany(r => r.Players)
             .HasForeignKey(p => p.RoomCode)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(p => p.Card)
             .WithMany()
             .HasForeignKey(p => p.CardId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(p => p.RoomCode);
        });

        // === Card ===
        mb.Entity<Card>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.RoomCode).HasMaxLength(6);
            e.Property(c => c.Grid).HasColumnType("jsonb");

            e.HasOne(c => c.Room)
             .WithMany(r => r.Cards)
             .HasForeignKey(c => c.RoomCode)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(c => c.RoomCode);
        });

        // === Question ===
        mb.Entity<Question>(e =>
        {
            e.HasKey(q => q.Id);
            e.Property(q => q.Text).HasMaxLength(500);
            e.Property(q => q.Source).HasMaxLength(100);
            e.Property(q => q.Options).HasColumnType("jsonb");
            e.Property(q => q.Type).HasConversion<string>().HasMaxLength(20);
        });

        // === QuestionSlot ===
        mb.Entity<QuestionSlot>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.RoomCode).HasMaxLength(6);
            e.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);

            e.HasOne(s => s.Room)
             .WithMany(r => r.QuestionPool)
             .HasForeignKey(s => s.RoomCode)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(s => s.Question)
             .WithMany()
             .HasForeignKey(s => s.QuestionId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(s => new { s.RoomCode, s.Position }).IsUnique();
            e.HasIndex(s => new { s.RoomCode, s.AssignedNumber }).IsUnique();
        });

        // === GameStateSnapshot (1-1 với Room) ===
        mb.Entity<GameStateSnapshot>(e =>
        {
            e.HasKey(g => g.RoomCode);
            e.Property(g => g.RoomCode).HasMaxLength(6);
            e.Property(g => g.Phase).HasConversion<string>().HasMaxLength(20);
            e.Property(g => g.PlayerQueue).HasColumnType("jsonb");

            e.HasOne(g => g.Room)
             .WithOne(r => r.GameState)
             .HasForeignKey<GameStateSnapshot>(g => g.RoomCode)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // === CalledNumber (composite PK) ===
        mb.Entity<CalledNumber>(e =>
        {
            e.HasKey(c => new { c.RoomCode, c.Number });
            e.Property(c => c.RoomCode).HasMaxLength(6);

            e.HasOne(c => c.Room)
             .WithMany(r => r.CalledNumbers)
             .HasForeignKey(c => c.RoomCode)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // === StealAttempt ===
        mb.Entity<StealAttempt>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.RoomCode).HasMaxLength(6);
            e.HasIndex(s => new { s.RoomCode, s.SlotId });
            e.HasIndex(s => s.Timestamp);
        });

        // === KinhClaim ===
        mb.Entity<KinhClaim>(e =>
        {
            e.HasKey(k => k.Id);
            e.Property(k => k.RoomCode).HasMaxLength(6);
            e.Property(k => k.WinType).HasConversion<string>().HasMaxLength(10);
            e.HasIndex(k => new { k.RoomCode, k.ClaimedAt });
        });
    }
}