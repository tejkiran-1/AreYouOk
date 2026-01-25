using AreYouOk.API.Data;
using AreYouOk.API.Models;
using AreYouOk.API.Services;
using Microsoft.EntityFrameworkCore;

namespace AreYouOk.API.BackgroundServices;

/// <summary>
/// Background service that monitors active journeys and sends alerts
/// when users miss their check-in deadlines
/// </summary>
public class JourneyMonitoringService : BackgroundService
{
    private readonly ILogger<JourneyMonitoringService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // Check every minute

    public JourneyMonitoringService(
        ILogger<JourneyMonitoringService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Journey Monitoring Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckOverdueJourneysAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking overdue journeys");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Journey Monitoring Service stopped");
    }

    /// <summary>
    /// Check for journeys that have missed their check-in deadline
    /// </summary>
    private async Task CheckOverdueJourneysAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AreYouOkDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();

        // Get all active journeys
        var activeJourneys = await context.Journeys
            .Include(j => j.User)
            .Include(j => j.FamilyMembers)
            .Where(j => j.Status == JourneyStatus.Active)
            .ToListAsync();

        foreach (var journey in activeJourneys)
        {
            // Skip if no check-in has been made yet
            if (journey.LastCheckInTime == null)
                continue;

            // Calculate deadline
            var deadline = journey.LastCheckInTime.Value.AddHours(journey.CheckInIntervalHours);
            var now = DateTime.UtcNow;

            // Check if deadline has passed
            if (now > deadline)
            {
                _logger.LogWarning($"Journey {journey.Id} missed deadline. User: {journey.User.FirstName} {journey.User.LastName}");

                // Update journey status to Alert
                journey.Status = JourneyStatus.Alert;
                journey.UpdatedAt = DateTime.UtcNow;

                // Create automatic alert safety status
                var alertStatus = new SafetyStatus
                {
                    JourneyId = journey.Id,
                    IsSafe = false,
                    Timestamp = DateTime.UtcNow,
                    Latitude = journey.Latitude,
                    Longitude = journey.Longitude,
                    Message = $"Automatic alert: User missed check-in deadline at {deadline:yyyy-MM-dd HH:mm:ss} UTC",
                    UpdateType = StatusUpdateType.AutomaticAlert
                };

                context.SafetyStatuses.Add(alertStatus);
                await context.SaveChangesAsync();

                // Send alert notifications to family members
                await notificationService.NotifyAlertAsync(journey, alertStatus);
                await notificationService.SendAlertWithSoundAsync(journey, alertStatus);

                _logger.LogInformation($"Alert sent for journey {journey.Id}");
            }
        }
    }
}
