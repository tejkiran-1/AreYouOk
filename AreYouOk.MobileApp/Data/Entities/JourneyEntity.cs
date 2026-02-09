using SQLite;

namespace AreYouOk.MobileApp.Data.Entities;

/// <summary>
/// Local database entity for journey information
/// </summary>
[Table("Journeys")]
public class JourneyEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Unique]
    public int ServerId { get; set; }

    public int UserId { get; set; }
    public string Destination { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Status { get; set; } = string.Empty; // Active, Completed, Cancelled
    public double? StartLatitude { get; set; }
    public double? StartLongitude { get; set; }
    public double? CurrentLatitude { get; set; }
    public double? CurrentLongitude { get; set; }
    public int CheckInIntervalMinutes { get; set; }
    public DateTime? NextCheckInTime { get; set; }
    public string FamilyMembersJson { get; set; } = string.Empty; // JSON serialized list
    public DateTime LastSyncedAt { get; set; }
    public bool IsSynced { get; set; }
}
