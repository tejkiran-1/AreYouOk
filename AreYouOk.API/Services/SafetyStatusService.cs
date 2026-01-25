using AreYouOk.API.Data;
using AreYouOk.API.Models;
using AreYouOk.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AreYouOk.API.Services;

/// <summary>
/// Service for managing safety status updates
/// </summary>
public class SafetyStatusService
{
    private readonly AreYouOkDbContext _context;
    private readonly NotificationService _notificationService;

    public SafetyStatusService(
        AreYouOkDbContext context,
        NotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Update safety status (user check-in)
    /// </summary>
    public async Task<ApiResponse<SafetyStatusDto>> UpdateSafetyStatusAsync(int userId, UpdateSafetyStatusDto dto)
    {
        // Get active journey
        var journey = await _context.Journeys
            .Include(j => j.User)
            .Include(j => j.FamilyMembers)
            .FirstOrDefaultAsync(j => j.UserId == userId && j.Status == JourneyStatus.Active);

        if (journey == null)
        {
            return ApiResponse<SafetyStatusDto>.ErrorResponse("No active journey found");
        }

        // Create safety status record
        var safetyStatus = new SafetyStatus
        {
            JourneyId = journey.Id,
            IsSafe = dto.IsSafe,
            Timestamp = DateTime.UtcNow,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Message = dto.Message,
            UpdateType = dto.IsSafe ? StatusUpdateType.ManualSafe : StatusUpdateType.ManualAlert
        };

        _context.SafetyStatuses.Add(safetyStatus);

        // Update journey
        journey.LastCheckInTime = DateTime.UtcNow;
        journey.UpdatedAt = DateTime.UtcNow;

        if (dto.Latitude.HasValue && dto.Longitude.HasValue)
        {
            journey.Latitude = dto.Latitude;
            journey.Longitude = dto.Longitude;
            journey.LastLocationUpdate = DateTime.UtcNow;
        }

        // If user is safe, ensure journey status is Active
        if (dto.IsSafe)
        {
            if (journey.Status == JourneyStatus.Alert)
            {
                journey.Status = JourneyStatus.Active;
            }
        }
        else
        {
            // User indicated they're not safe - set alert status
            journey.Status = JourneyStatus.Alert;
        }

        await _context.SaveChangesAsync();

        // Send notifications to family members
        if (dto.IsSafe)
        {
            await _notificationService.NotifySafeCheckInAsync(journey, safetyStatus);
        }
        else
        {
            await _notificationService.NotifyAlertAsync(journey, safetyStatus);
        }

        var response = new SafetyStatusDto
        {
            Id = safetyStatus.Id,
            JourneyId = safetyStatus.JourneyId,
            IsSafe = safetyStatus.IsSafe,
            Timestamp = safetyStatus.Timestamp,
            Latitude = safetyStatus.Latitude,
            Longitude = safetyStatus.Longitude,
            Message = safetyStatus.Message,
            UpdateType = safetyStatus.UpdateType.ToString()
        };

        return ApiResponse<SafetyStatusDto>.SuccessResponse(response);
    }

    /// <summary>
    /// Get safety status history for a journey
    /// </summary>
    public async Task<ApiResponse<List<SafetyStatusDto>>> GetJourneyStatusHistoryAsync(int journeyId, int userId)
    {
        var journey = await _context.Journeys
            .Include(j => j.FamilyMembers)
            .FirstOrDefaultAsync(j => j.Id == journeyId);

        if (journey == null)
        {
            return ApiResponse<List<SafetyStatusDto>>.ErrorResponse("Journey not found");
        }

        // Check if user is owner or family member
        if (journey.UserId != userId && !journey.FamilyMembers.Any(fm => fm.UserId == userId))
        {
            return ApiResponse<List<SafetyStatusDto>>.ErrorResponse("Access denied");
        }

        var statuses = await _context.SafetyStatuses
            .Where(s => s.JourneyId == journeyId)
            .OrderByDescending(s => s.Timestamp)
            .ToListAsync();

        var dtos = statuses.Select(s => new SafetyStatusDto
        {
            Id = s.Id,
            JourneyId = s.JourneyId,
            IsSafe = s.IsSafe,
            Timestamp = s.Timestamp,
            Latitude = s.Latitude,
            Longitude = s.Longitude,
            Message = s.Message,
            UpdateType = s.UpdateType.ToString()
        }).ToList();

        return ApiResponse<List<SafetyStatusDto>>.SuccessResponse(dtos);
    }
}
