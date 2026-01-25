using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AreYouOk.API.Models;

/// <summary>
/// Represents a journey where a user tracks their safety status
/// </summary>
public class Journey
{
    /// <summary>
    /// Unique identifier for the journey
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Foreign key: User who started this journey
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// Navigation property: User who started this journey
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Date and time when the journey started
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the journey ended (null if ongoing)
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Minimum time interval (in hours) for safety check-ins
    /// Example: 10 means user must check in every 10 hours
    /// </summary>
    [Required]
    [Range(0.5, 168)] // From 30 minutes to 1 week
    public double CheckInIntervalHours { get; set; }

    /// <summary>
    /// Last time the user checked in (clicked "I'm safe")
    /// </summary>
    public DateTime? LastCheckInTime { get; set; }

    /// <summary>
    /// Current status of the journey
    /// </summary>
    [Required]
    public JourneyStatus Status { get; set; } = JourneyStatus.Active;

    /// <summary>
    /// Last known latitude of the user
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Last known longitude of the user
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Last time location was updated
    /// </summary>
    public DateTime? LastLocationUpdate { get; set; }

    /// <summary>
    /// Date and time of last update
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property: Family members monitoring this journey
    /// </summary>
    public virtual ICollection<FamilyMember> FamilyMembers { get; set; } = new List<FamilyMember>();

    /// <summary>
    /// Navigation property: Safety status updates during this journey
    /// </summary>
    public virtual ICollection<SafetyStatus> SafetyStatuses { get; set; } = new List<SafetyStatus>();
}

/// <summary>
/// Enum representing the status of a journey
/// </summary>
public enum JourneyStatus
{
    /// <summary>
    /// Journey is currently active and ongoing
    /// </summary>
    Active = 1,

    /// <summary>
    /// Journey completed successfully
    /// </summary>
    Completed = 2,

    /// <summary>
    /// User missed check-in deadline - alert sent
    /// </summary>
    Alert = 3,

    /// <summary>
    /// Journey was cancelled by user
    /// </summary>
    Cancelled = 4
}
