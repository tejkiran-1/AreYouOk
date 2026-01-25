using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AreYouOk.API.Models;

/// <summary>
/// Represents a family member who can monitor a user's journey
/// Family members are identified by their phone number and linked to a registered user
/// </summary>
public class FamilyMember
{
    /// <summary>
    /// Unique identifier for the family member relationship
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Foreign key: Journey being monitored
    /// </summary>
    [Required]
    public int JourneyId { get; set; }

    /// <summary>
    /// Navigation property: Journey being monitored
    /// </summary>
    [ForeignKey(nameof(JourneyId))]
    public virtual Journey Journey { get; set; } = null!;

    /// <summary>
    /// Phone number of the family member
    /// </summary>
    [Required]
    [MaxLength(20)]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key: User record if this phone number belongs to a registered user (nullable)
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Navigation property: User record if this family member is a registered user
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }

    /// <summary>
    /// Display name for this family member (fetched from User if registered)
    /// </summary>
    [MaxLength(200)]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Device token for push notifications (if family member is a registered user)
    /// </summary>
    [MaxLength(500)]
    public string? DeviceToken { get; set; }

    /// <summary>
    /// Whether this family member should receive notifications
    /// </summary>
    public bool ReceiveNotifications { get; set; } = true;

    /// <summary>
    /// Date and time when this family member was added
    /// </summary>
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
