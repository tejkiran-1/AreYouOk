using AreYouOk.API.Data;
using AreYouOk.API.Models;
using AreYouOk.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AreYouOk.API.Services;

/// <summary>
/// Service for managing user journeys
/// </summary>
public class JourneyService
{
    private readonly AreYouOkDbContext _context;
    private readonly UserService _userService;
    private readonly NotificationService _notificationService;

    public JourneyService(
        AreYouOkDbContext context,
        UserService userService,
        NotificationService notificationService)
    {
        _context = context;
        _userService = userService;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Start a new journey
    /// </summary>
    public async Task<ApiResponse<JourneyDto>> StartJourneyAsync(int userId, StartJourneyDto dto)
    {
        // Check if user has an active journey
        var activeJourney = await _context.Journeys
            .FirstOrDefaultAsync(j => j.UserId == userId && j.Status == JourneyStatus.Active);

        if (activeJourney != null)
        {
            return ApiResponse<JourneyDto>.ErrorResponse("You already have an active journey. Please end it before starting a new one.");
        }

        // Create new journey
        var journey = new Journey
        {
            UserId = userId,
            StartTime = DateTime.UtcNow,
            CheckInIntervalHours = dto.CheckInIntervalHours,
            LastCheckInTime = DateTime.UtcNow, // First check-in is at start
            Status = JourneyStatus.Active,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            LastLocationUpdate = dto.Latitude.HasValue && dto.Longitude.HasValue ? DateTime.UtcNow : null,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Journeys.Add(journey);
        await _context.SaveChangesAsync();

        // Add family members
        var familyMembers = new List<FamilyMember>();
        foreach (var phone in dto.FamilyMemberPhones.Distinct())
        {
            var familyUser = await _userService.GetUserByPhoneAsync(phone);
            
            var familyMember = new FamilyMember
            {
                JourneyId = journey.Id,
                PhoneNumber = phone,
                UserId = familyUser?.Id,
                DisplayName = familyUser != null ? $"{familyUser.FirstName} {familyUser.LastName}" : null,
                ReceiveNotifications = true,
                AddedAt = DateTime.UtcNow
            };

            familyMembers.Add(familyMember);
        }

        _context.FamilyMembers.AddRange(familyMembers);
        await _context.SaveChangesAsync();

        // Load user and family members for response
        await _context.Entry(journey).Reference(j => j.User).LoadAsync();
        journey.FamilyMembers = familyMembers;

        // Notify family members
        await _notificationService.NotifyJourneyStartedAsync(journey);

        return ApiResponse<JourneyDto>.SuccessResponse(MapToDto(journey));
    }

    /// <summary>
    /// Get active journey for a user
    /// </summary>
    public async Task<ApiResponse<JourneyDto>> GetActiveJourneyAsync(int userId)
    {
        var journey = await _context.Journeys
            .Include(j => j.User)
            .Include(j => j.FamilyMembers)
                .ThenInclude(fm => fm.User)
            .FirstOrDefaultAsync(j => j.UserId == userId && j.Status == JourneyStatus.Active);

        if (journey == null)
        {
            return ApiResponse<JourneyDto>.ErrorResponse("No active journey found");
        }

        return ApiResponse<JourneyDto>.SuccessResponse(MapToDto(journey));
    }

    /// <summary>
    /// Get journey by ID
    /// </summary>
    public async Task<ApiResponse<JourneyDto>> GetJourneyByIdAsync(int journeyId, int userId)
    {
        var journey = await _context.Journeys
            .Include(j => j.User)
            .Include(j => j.FamilyMembers)
                .ThenInclude(fm => fm.User)
            .FirstOrDefaultAsync(j => j.Id == journeyId);

        if (journey == null)
        {
            return ApiResponse<JourneyDto>.ErrorResponse("Journey not found");
        }

        // Check if user is owner or family member
        if (journey.UserId != userId && !journey.FamilyMembers.Any(fm => fm.UserId == userId))
        {
            return ApiResponse<JourneyDto>.ErrorResponse("Access denied");
        }

        return ApiResponse<JourneyDto>.SuccessResponse(MapToDto(journey));
    }

    /// <summary>
    /// End a journey
    /// </summary>
    public async Task<ApiResponse<JourneyDto>> EndJourneyAsync(int userId, EndJourneyDto dto)
    {
        var journey = await _context.Journeys
            .Include(j => j.User)
            .Include(j => j.FamilyMembers)
            .FirstOrDefaultAsync(j => j.UserId == userId && j.Status == JourneyStatus.Active);

        if (journey == null)
        {
            return ApiResponse<JourneyDto>.ErrorResponse("No active journey found");
        }

        journey.EndTime = DateTime.UtcNow;
        journey.Status = JourneyStatus.Completed;
        journey.UpdatedAt = DateTime.UtcNow;

        if (dto.Latitude.HasValue && dto.Longitude.HasValue)
        {
            journey.Latitude = dto.Latitude;
            journey.Longitude = dto.Longitude;
            journey.LastLocationUpdate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Notify family members
        await _notificationService.NotifyJourneyEndedAsync(journey);

        return ApiResponse<JourneyDto>.SuccessResponse(MapToDto(journey));
    }

    /// <summary>
    /// Update location for active journey
    /// </summary>
    public async Task<ApiResponse<JourneyDto>> UpdateLocationAsync(int userId, UpdateLocationDto dto)
    {
        var journey = await _context.Journeys
            .Include(j => j.User)
            .Include(j => j.FamilyMembers)
            .FirstOrDefaultAsync(j => j.UserId == userId && j.Status == JourneyStatus.Active);

        if (journey == null)
        {
            return ApiResponse<JourneyDto>.ErrorResponse("No active journey found");
        }

        journey.Latitude = dto.Latitude;
        journey.Longitude = dto.Longitude;
        journey.LastLocationUpdate = DateTime.UtcNow;
        journey.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse<JourneyDto>.SuccessResponse(MapToDto(journey));
    }

    /// <summary>
    /// Get all journeys for a user (history)
    /// </summary>
    public async Task<ApiResponse<List<JourneyDto>>> GetUserJourneysAsync(int userId, int page = 1, int pageSize = 20)
    {
        var journeys = await _context.Journeys
            .Include(j => j.User)
            .Include(j => j.FamilyMembers)
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => j.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = journeys.Select(MapToDto).ToList();
        return ApiResponse<List<JourneyDto>>.SuccessResponse(dtos);
    }

    /// <summary>
    /// Get journeys where user is a family member
    /// </summary>
    public async Task<ApiResponse<List<JourneyDto>>> GetMonitoredJourneysAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return ApiResponse<List<JourneyDto>>.ErrorResponse("User not found");
        }

        var journeys = await _context.Journeys
            .Include(j => j.User)
            .Include(j => j.FamilyMembers)
            .Where(j => j.FamilyMembers.Any(fm => fm.PhoneNumber == user.PhoneNumber) && j.Status == JourneyStatus.Active)
            .OrderByDescending(j => j.StartTime)
            .ToListAsync();

        var dtos = journeys.Select(MapToDto).ToList();
        return ApiResponse<List<JourneyDto>>.SuccessResponse(dtos);
    }

    /// <summary>
    /// Map Journey entity to DTO
    /// </summary>
    private JourneyDto MapToDto(Journey journey)
    {
        return new JourneyDto
        {
            Id = journey.Id,
            UserId = journey.UserId,
            UserName = $"{journey.User.FirstName} {journey.User.LastName}",
            StartTime = journey.StartTime,
            EndTime = journey.EndTime,
            CheckInIntervalHours = journey.CheckInIntervalHours,
            LastCheckInTime = journey.LastCheckInTime,
            Status = journey.Status.ToString(),
            Latitude = journey.Latitude,
            Longitude = journey.Longitude,
            LastLocationUpdate = journey.LastLocationUpdate,
            FamilyMembers = journey.FamilyMembers.Select(fm => new FamilyMemberDto
            {
                Id = fm.Id,
                PhoneNumber = fm.PhoneNumber,
                UserId = fm.UserId,
                DisplayName = fm.DisplayName,
                ReceiveNotifications = fm.ReceiveNotifications,
                AddedAt = fm.AddedAt
            }).ToList()
        };
    }
}
