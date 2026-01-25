using AreYouOk.API.Services;
using AreYouOk.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AreYouOk.API.Controllers;

/// <summary>
/// Journey management controller
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class JourneyController : ControllerBase
{
    private readonly JourneyService _journeyService;
    private readonly ILogger<JourneyController> _logger;

    public JourneyController(JourneyService journeyService, ILogger<JourneyController> logger)
    {
        _journeyService = journeyService;
        _logger = logger;
    }

    /// <summary>
    /// Start a new journey
    /// </summary>
    /// <param name="dto">Journey details</param>
    /// <returns>Created journey</returns>
    [HttpPost("start")]
    [ProducesResponseType(typeof(ApiResponse<JourneyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<JourneyDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<JourneyDto>>> StartJourney([FromBody] StartJourneyDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
            
            return BadRequest(ApiResponse<JourneyDto>.ValidationErrorResponse(errors));
        }

        var userId = GetCurrentUserId();
        var result = await _journeyService.StartJourneyAsync(userId, dto);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        _logger.LogInformation($"Journey started by user {userId}");
        return Ok(result);
    }

    /// <summary>
    /// Get active journey for current user
    /// </summary>
    /// <returns>Active journey or error</returns>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<JourneyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<JourneyDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<JourneyDto>>> GetActiveJourney()
    {
        var userId = GetCurrentUserId();
        var result = await _journeyService.GetActiveJourneyAsync(userId);
        
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get journey by ID
    /// </summary>
    /// <param name="id">Journey ID</param>
    /// <returns>Journey details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<JourneyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<JourneyDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<JourneyDto>>> GetJourney(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _journeyService.GetJourneyByIdAsync(id, userId);
        
        if (!result.Success)
        {
            if (result.ErrorMessage == "Access denied")
            {
                return StatusCode(StatusCodes.Status403Forbidden, result);
            }
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// End active journey
    /// </summary>
    /// <param name="dto">Journey end details</param>
    /// <returns>Ended journey</returns>
    [HttpPost("end")]
    [ProducesResponseType(typeof(ApiResponse<JourneyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<JourneyDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<JourneyDto>>> EndJourney([FromBody] EndJourneyDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _journeyService.EndJourneyAsync(userId, dto);
        
        if (!result.Success)
        {
            return NotFound(result);
        }

        _logger.LogInformation($"Journey ended by user {userId}");
        return Ok(result);
    }

    /// <summary>
    /// Update location for active journey
    /// </summary>
    /// <param name="dto">Location details</param>
    /// <returns>Updated journey</returns>
    [HttpPut("location")]
    [ProducesResponseType(typeof(ApiResponse<JourneyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<JourneyDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<JourneyDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<JourneyDto>>> UpdateLocation([FromBody] UpdateLocationDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
            
            return BadRequest(ApiResponse<JourneyDto>.ValidationErrorResponse(errors));
        }

        var userId = GetCurrentUserId();
        var result = await _journeyService.UpdateLocationAsync(userId, dto);
        
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get journey history for current user
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of journeys</returns>
    [HttpGet("history")]
    [ProducesResponseType(typeof(ApiResponse<List<JourneyDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<List<JourneyDto>>>> GetHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        var result = await _journeyService.GetUserJourneysAsync(userId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get journeys where current user is a family member
    /// </summary>
    /// <returns>List of journeys being monitored</returns>
    [HttpGet("monitoring")]
    [ProducesResponseType(typeof(ApiResponse<List<JourneyDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<List<JourneyDto>>>> GetMonitoredJourneys()
    {
        var userId = GetCurrentUserId();
        var result = await _journeyService.GetMonitoredJourneysAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Helper method to get current user ID from JWT claims
    /// </summary>
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            throw new UnauthorizedAccessException("Invalid user token");
        }
        return userId;
    }
}
