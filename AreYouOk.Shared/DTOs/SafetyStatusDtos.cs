using System.ComponentModel.DataAnnotations;

namespace AreYouOk.Shared.DTOs;

/// <summary>
/// DTO for updating safety status (check-in)
/// </summary>
public class UpdateSafetyStatusDto
{
    [Required(ErrorMessage = "Safety status is required")]
    public bool IsSafe { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    [StringLength(500, ErrorMessage = "Message cannot exceed 500 characters")]
    public string? Message { get; set; }
}

/// <summary>
/// DTO for safety status response
/// </summary>
public class SafetyStatusDto
{
    public int Id { get; set; }
    public int JourneyId { get; set; }
    public bool IsSafe { get; set; }
    public DateTime Timestamp { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Message { get; set; }
    public string UpdateType { get; set; } = string.Empty;
}

/// <summary>
/// DTO for alert notification
/// </summary>
public class AlertNotificationDto
{
    public int JourneyId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserPhone { get; set; } = string.Empty;
    public string AlertType { get; set; } = string.Empty;
    public DateTime AlertTime { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Message { get; set; }
    
    /// <summary>
    /// Google Maps URL for the location
    /// </summary>
    public string? LocationUrl
    {
        get
        {
            if (Latitude.HasValue && Longitude.HasValue)
            {
                return $"https://www.google.com/maps?q={Latitude.Value},{Longitude.Value}";
            }
            return null;
        }
    }
}
