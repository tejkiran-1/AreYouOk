using SQLite;

namespace AreYouOk.MobileApp.Data.Entities;

/// <summary>
/// Local database entity for safety status check-ins
/// </summary>
[Table("SafetyStatuses")]
public class SafetyStatusEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int? ServerId { get; set; }
    public int JourneyId { get; set; }
    public int UserId { get; set; }
    public bool IsSafe { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Notes { get; set; }
    public DateTime CheckInTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsSynced { get; set; }
}
