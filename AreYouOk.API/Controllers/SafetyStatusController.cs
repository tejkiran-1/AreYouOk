using AreYouOk.API.Services;
using AreYouOk.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AreYouOk.API.Controllers;

/// <summary>
/// Safety status management controller
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SafetyStatusController : ControllerBase
{
    private readonly SafetyStatusService _safetyStatusService;
    private readonly ILogger<SafetyStatusController> _logger;

    public SafetyStatusController(
        SafetyStatusService safetyStatusService,
        ILogger<SafetyStatusController> logger)
    {
        _safetyStatusService = safetyStatusService;
        _logger = logger;
    }

    /// <summary>
    /// Update safety status (check-in)
    /// </summary>
    /// <param name="dto">Safety status details</param>
    /// <returns>Created safety status</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SafetyStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SafetyStatusDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<SafetyStatusDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<SafetyStatusDto>>> UpdateSafetyStatus([FromBody] UpdateSafetyStatusDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
            
            return BadRequest(ApiResponse<SafetyStatusDto>.ValidationErrorResponse(errors));
        }

        var userId = GetCurrentUserId();
        var result = await _safetyStatusService.UpdateSafetyStatusAsync(userId, dto);
        
        if (!result.Success)
        {
            return NotFound(result);
        }

        _logger.LogInformation($"Safety status updated by user {userId}: {(dto.IsSafe ? "Safe" : "Alert")}");
        return Ok(result);
    }

    /// <summary>
    /// Get safety status history for a journey
    /// </summary>
    /// <param name="journeyId">Journey ID</param>
    /// <returns>List of safety status updates</returns>
    [HttpGet("journey/{journeyId}")]
    [ProducesResponseType(typeof(ApiResponse<List<SafetyStatusDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<SafetyStatusDto>>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<SafetyStatusDto>>>> GetJourneyStatusHistory(int journeyId)
    {
        var userId = GetCurrentUserId();
        var result = await _safetyStatusService.GetJourneyStatusHistoryAsync(journeyId, userId);
        
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
