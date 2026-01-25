using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AreYouOk.API.Models;

/// <summary>
/// Represents a safety status update (check-in) during a journey
/// </summary>
public class SafetyStatus
{
    /// <summary>
    /// Unique identifier for the safety status update
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Foreign key: Journey this status update belongs to
    /// </summary>
    [Required]
    public int JourneyId { get; set; }

    /// <summary>
    /// Navigation property: Journey this status update belongs to
    /// </summary>
    [ForeignKey(nameof(JourneyId))]
    public virtual Journey Journey { get; set; } = null!;

    /// <summary>
    /// Whether the user indicated they are safe
    /// True = "I'm safe", False = "I'm not safe/in danger"
    /// </summary>
    [Required]
    public bool IsSafe { get; set; }

    /// <summary>
    /// Date and time of this status update
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Latitude where the status was updated
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Longitude where the status was updated
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Optional message from the user
    /// </summary>
    [MaxLength(500)]
    public string? Message { get; set; }

    /// <summary>
    /// Type of status update
    /// </summary>
    [Required]
    public StatusUpdateType UpdateType { get; set; }
}

/// <summary>
/// Type of safety status update
/// </summary>
public enum StatusUpdateType
{
    /// <summary>
    /// User manually clicked "I'm safe"
    /// </summary>
    ManualSafe = 1,

    /// <summary>
    /// User manually clicked "I'm not safe"
    /// </summary>
    ManualAlert = 2,

    /// <summary>
    /// System generated alert due to missed check-in deadline
    /// </summary>
    AutomaticAlert = 3
}
