using AreYouOk.Shared.DTOs;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace AreYouOk.MobileApp.Services;

/// <summary>
/// API client for communicating with the backend
/// </summary>
public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly SettingsService _settingsService;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiClient(HttpClient httpClient, SettingsService settingsService)
    {
        _httpClient = httpClient;
        _settingsService = settingsService;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Add auth token to request headers if available
    /// </summary>
    private void AddAuthHeader()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null; // Clear existing
        
        if (!string.IsNullOrEmpty(_settingsService.AuthToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _settingsService.AuthToken);
        }
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/register", dto);
            var content = await response.Content.ReadAsStringAsync();
            
            System.Diagnostics.Debug.WriteLine($"API Response Status: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"API Response Content: {content}");
            
            if (!response.IsSuccessStatusCode)
            {
                return ApiResponse<AuthResponseDto>.ErrorResponse($"API Error ({response.StatusCode}): {content}");
            }
            
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>(_jsonOptions);
            return result ?? ApiResponse<AuthResponseDto>.ErrorResponse("Invalid response from server");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
            return ApiResponse<AuthResponseDto>.ErrorResponse($"Network error: {ex.Message}");
        }
    }

    /// <summary>
    /// Login user
    /// </summary>
    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/login", dto);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>(_jsonOptions);
            return result ?? ApiResponse<AuthResponseDto>.ErrorResponse("Invalid response from server");
        }
        catch (Exception ex)
        {
            return ApiResponse<AuthResponseDto>.ErrorResponse($"Network error: {ex.Message}");
        }
    }

    /// <summary>
    /// Start a new journey
    /// </summary>
    public async Task<ApiResponse<JourneyDto>> StartJourneyAsync(StartJourneyDto dto)
    {
        try
        {
            AddAuthHeader();
            
            var response = await _httpClient.PostAsJsonAsync("api/Journey/start", dto);
            var content = await response.Content.ReadAsStringAsync();
            
            System.Diagnostics.Debug.WriteLine($"StartJourney API Status: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"StartJourney API Content: {content}");
            
            if (!response.IsSuccessStatusCode)
            {
                return ApiResponse<JourneyDto>.ErrorResponse($"API Error ({response.StatusCode}): {content}");
            }
            
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<JourneyDto>>(_jsonOptions);
            return result ?? ApiResponse<JourneyDto>.ErrorResponse("Invalid response from server");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"StartJourney Exception: {ex}");
            return ApiResponse<JourneyDto>.ErrorResponse($"Network error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get active journey
    /// </summary>
    public async Task<ApiResponse<JourneyDto>> GetActiveJourneyAsync()
    {
        try
        {
            AddAuthHeader();
            var response = await _httpClient.GetAsync("api/Journey/active");
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<JourneyDto>>(_jsonOptions);
            return result ?? ApiResponse<JourneyDto>.ErrorResponse("Invalid response from server");
        }
        catch (Exception ex)
        {
            return ApiResponse<JourneyDto>.ErrorResponse($"Network error: {ex.Message}");
        }
    }

    /// <summary>
    /// End active journey
    /// </summary>
    public async Task<ApiResponse<JourneyDto>> EndJourneyAsync(EndJourneyDto dto)
    {
        try
        {
            AddAuthHeader();
            var response = await _httpClient.PostAsJsonAsync("api/Journey/end", dto);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<JourneyDto>>(_jsonOptions);
            return result ?? ApiResponse<JourneyDto>.ErrorResponse("Invalid response from server");
        }
        catch (Exception ex)
        {
            return ApiResponse<JourneyDto>.ErrorResponse($"Network error: {ex.Message}");
        }
    }

    /// <summary>
    /// Update safety status
    /// </summary>
    public async Task<ApiResponse<SafetyStatusDto>> UpdateSafetyStatusAsync(UpdateSafetyStatusDto dto)
    {
        try
        {
            AddAuthHeader();
            var response = await _httpClient.PostAsJsonAsync("api/SafetyStatus", dto);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<SafetyStatusDto>>(_jsonOptions);
            return result ?? ApiResponse<SafetyStatusDto>.ErrorResponse("Invalid response from server");
        }
        catch (Exception ex)
        {
            return ApiResponse<SafetyStatusDto>.ErrorResponse($"Network error: {ex.Message}");
        }
    }

    /// <summary>
    /// Update location
    /// </summary>
    public async Task<ApiResponse<JourneyDto>> UpdateLocationAsync(UpdateLocationDto dto)
    {
        try
        {
            AddAuthHeader();
            var response = await _httpClient.PutAsJsonAsync("api/Journey/location", dto);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<JourneyDto>>(_jsonOptions);
            return result ?? ApiResponse<JourneyDto>.ErrorResponse("Invalid response from server");
        }
        catch (Exception ex)
        {
            return ApiResponse<JourneyDto>.ErrorResponse($"Network error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get journey history
    /// </summary>
    public async Task<ApiResponse<List<JourneyDto>>> GetJourneyHistoryAsync(int page = 1, int pageSize = 20)
    {
        try
        {
            AddAuthHeader();
            var response = await _httpClient.GetAsync($"api/Journey/history?page={page}&pageSize={pageSize}");
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<JourneyDto>>>(_jsonOptions);
            return result ?? ApiResponse<List<JourneyDto>>.ErrorResponse("Invalid response from server");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<JourneyDto>>.ErrorResponse($"Network error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get monitored journeys (where user is a family member)
    /// </summary>
    public async Task<ApiResponse<List<JourneyDto>>> GetMonitoredJourneysAsync()
    {
        try
        {
            AddAuthHeader();
            var response = await _httpClient.GetAsync("api/Journey/monitoring");
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<JourneyDto>>>(_jsonOptions);
            return result ?? ApiResponse<List<JourneyDto>>.ErrorResponse("Invalid response from server");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<JourneyDto>>.ErrorResponse($"Network error: {ex.Message}");
        }
    }
}
