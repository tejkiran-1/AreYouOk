using AreYouOk.API.Models;
using AreYouOk.Shared.DTOs;

namespace AreYouOk.API.Services;

/// <summary>
/// Service for sending notifications to family members
/// This is a placeholder implementation. In production, integrate with:
/// - Firebase Cloud Messaging (FCM) for push notifications
/// - SMS gateway (Twilio, etc.) for SMS alerts
/// - Email service for email alerts
/// </summary>
public class NotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Notify family members that a journey has started
    /// </summary>
    public async Task NotifyJourneyStartedAsync(Journey journey)
    {
        _logger.LogInformation($"Journey {journey.Id} started by {journey.User.FirstName} {journey.User.LastName}");
        
        foreach (var familyMember in journey.FamilyMembers.Where(fm => fm.ReceiveNotifications))
        {
            var message = $"{journey.User.FirstName} {journey.User.LastName} has started a journey and added you as a safety contact. You will be notified if they don't check in within {journey.CheckInIntervalHours} hours.";
            
            await SendNotificationAsync(familyMember, "Journey Started", message, journey);
        }
    }

    /// <summary>
    /// Notify family members that a journey has ended
    /// </summary>
    public async Task NotifyJourneyEndedAsync(Journey journey)
    {
        _logger.LogInformation($"Journey {journey.Id} ended by {journey.User.FirstName} {journey.User.LastName}");
        
        foreach (var familyMember in journey.FamilyMembers.Where(fm => fm.ReceiveNotifications))
        {
            var message = $"{journey.User.FirstName} {journey.User.LastName} has safely completed their journey.";
            
            await SendNotificationAsync(familyMember, "Journey Completed", message, journey);
        }
    }

    /// <summary>
    /// Notify family members of a safe check-in
    /// </summary>
    public async Task NotifySafeCheckInAsync(Journey journey, SafetyStatus status)
    {
        _logger.LogInformation($"Safe check-in for journey {journey.Id}");
        
        foreach (var familyMember in journey.FamilyMembers.Where(fm => fm.ReceiveNotifications))
        {
            var message = $"{journey.User.FirstName} {journey.User.LastName} checked in and confirmed they are safe.";
            
            if (!string.IsNullOrWhiteSpace(status.Message))
            {
                message += $" Message: {status.Message}";
            }
            
            await SendNotificationAsync(familyMember, "Safe Check-in", message, journey);
        }
    }

    /// <summary>
    /// Notify family members of an alert (user indicated not safe or missed deadline)
    /// </summary>
    public async Task NotifyAlertAsync(Journey journey, SafetyStatus status)
    {
        _logger.LogWarning($"ALERT for journey {journey.Id}: User indicated not safe or missed deadline");
        
        foreach (var familyMember in journey.FamilyMembers.Where(fm => fm.ReceiveNotifications))
        {
            var alertType = status.UpdateType == StatusUpdateType.ManualAlert 
                ? "indicated they are NOT SAFE" 
                : "MISSED their check-in deadline";
            
            var message = $"⚠️ ALERT: {journey.User.FirstName} {journey.User.LastName} {alertType}. ";
            
            if (journey.Latitude.HasValue && journey.Longitude.HasValue)
            {
                var locationUrl = $"https://www.google.com/maps?q={journey.Latitude.Value},{journey.Longitude.Value}";
                message += $"Last known location: {locationUrl}";
            }
            
            if (!string.IsNullOrWhiteSpace(status.Message))
            {
                message += $" Message: {status.Message}";
            }
            
            await SendNotificationAsync(familyMember, "⚠️ SAFETY ALERT", message, journey, isUrgent: true);
        }
    }

    /// <summary>
    /// Send notification to a family member
    /// </summary>
    private async Task SendNotificationAsync(
        FamilyMember familyMember, 
        string title, 
        string message, 
        Journey journey, 
        bool isUrgent = false)
    {
        // Log notification (in production, replace with actual notification sending)
        _logger.LogInformation($"Notification to {familyMember.PhoneNumber}: {title} - {message}");
        
        // TODO: Implement actual notification sending
        // 1. If familyMember has DeviceToken, send push notification via FCM
        // 2. Send SMS via Twilio or similar service
        // 3. If urgent, also play alarm sound via push notification
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Send alert with sound notification
    /// </summary>
    public async Task SendAlertWithSoundAsync(Journey journey, SafetyStatus status)
    {
        _logger.LogWarning($"Sending alert with sound for journey {journey.Id}");
        
        // TODO: Implement push notification with custom sound/alarm
        // This should trigger a loud alarm on family members' devices
        
        await Task.CompletedTask;
    }
}
