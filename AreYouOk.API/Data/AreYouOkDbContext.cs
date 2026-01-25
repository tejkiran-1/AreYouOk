using AreYouOk.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AreYouOk.API.Data;

/// <summary>
/// Database context for the AreYouOk application
/// </summary>
public class AreYouOkDbContext : DbContext
{
    /// <summary>
    /// Constructor with options
    /// </summary>
    /// <param name="options">DbContext options</param>
    public AreYouOkDbContext(DbContextOptions<AreYouOkDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Users table
    /// </summary>
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>
    /// Journeys table
    /// </summary>
    public DbSet<Journey> Journeys { get; set; } = null!;

    /// <summary>
    /// Family members table
    /// </summary>
    public DbSet<FamilyMember> FamilyMembers { get; set; } = null!;

    /// <summary>
    /// Safety status updates table
    /// </summary>
    public DbSet<SafetyStatus> SafetyStatuses { get; set; } = null!;

    /// <summary>
    /// Configure the model relationships and constraints
    /// </summary>
    /// <param name="modelBuilder">Model builder</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.PhoneNumber).IsUnique();
            
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
            
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        });

        // Configure Journey entity
        modelBuilder.Entity<Journey>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.UserId, e.Status });
            
            entity.Property(e => e.StartTime)
                .HasDefaultValueSql("GETUTCDATE()");
            
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Journeys)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure FamilyMember entity
        modelBuilder.Entity<FamilyMember>(entity =>
        {
            entity.HasIndex(e => e.JourneyId);
            entity.HasIndex(e => e.PhoneNumber);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.JourneyId, e.PhoneNumber });
            
            entity.Property(e => e.AddedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Journey)
                .WithMany(j => j.FamilyMembers)
                .HasForeignKey(e => e.JourneyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany(u => u.FamilyMembersForMyJourneys)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Configure SafetyStatus entity
        modelBuilder.Entity<SafetyStatus>(entity =>
        {
            entity.HasIndex(e => e.JourneyId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.JourneyId, e.Timestamp });
            
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Journey)
                .WithMany(j => j.SafetyStatuses)
                .HasForeignKey(e => e.JourneyId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
