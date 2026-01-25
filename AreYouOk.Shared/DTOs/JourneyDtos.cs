using System.ComponentModel.DataAnnotations;

namespace AreYouOk.Shared.DTOs;

/// <summary>
/// DTO for starting a new journey
/// </summary>
public class StartJourneyDto
{
    [Required(ErrorMessage = "Check-in interval is required")]
    [Range(0.5, 168, ErrorMessage = "Check-in interval must be between 0.5 and 168 hours")]
    public double CheckInIntervalHours { get; set; }

    [Required(ErrorMessage = "At least one family member is required")]
    [MinLength(1, ErrorMessage = "At least one family member is required")]
    public List<string> FamilyMemberPhones { get; set; } = new();

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

/// <summary>
/// DTO for journey response
/// </summary>
public class JourneyDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public double CheckInIntervalHours { get; set; }
    public DateTime? LastCheckInTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime? LastLocationUpdate { get; set; }
    public List<FamilyMemberDto> FamilyMembers { get; set; } = new();
    
    /// <summary>
    /// Time remaining until next required check-in
    /// </summary>
    public TimeSpan? TimeUntilNextCheckIn
    {
        get
        {
            if (LastCheckInTime == null || Status != "Active")
                return null;

            var nextCheckInTime = LastCheckInTime.Value.AddHours(CheckInIntervalHours);
            var timeRemaining = nextCheckInTime - DateTime.UtcNow;
            
            return timeRemaining.TotalSeconds > 0 ? timeRemaining : TimeSpan.Zero;
        }
    }

    /// <summary>
    /// Whether the check-in deadline has been missed
    /// </summary>
    public bool IsOverdue => TimeUntilNextCheckIn != null && TimeUntilNextCheckIn.Value.TotalSeconds <= 0;
}

/// <summary>
/// DTO for ending a journey
/// </summary>
public class EndJourneyDto
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

/// <summary>
/// DTO for updating location
/// </summary>
public class UpdateLocationDto
{
    [Required]
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
    public double Latitude { get; set; }

    [Required]
    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
    public double Longitude { get; set; }
}
