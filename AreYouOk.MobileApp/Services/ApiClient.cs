using AreYouOk.Shared.DTOs;
using Polly;
using Polly.Extensions.Http;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace AreYouOk.MobileApp.Services;

/// <summary>
/// Enhanced API client with retry logic, timeout handling, and offline support
/// </summary>
public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly SettingsService _settingsService;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly SemaphoreSlim _authLock = new(1, 1);

    public ApiClient(HttpClient httpClient, SettingsService settingsService)
    {
        _httpClient = httpClient;
        _settingsService = settingsService;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        // Configure longer timeout for slow networks
        _httpClient.Timeout = TimeSpan.FromSeconds(60);
    }

    private void AddAuthHeader()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
        
        if (!string.IsNullOrEmpty(_settingsService.AuthToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _settingsService.AuthToken);
            Debug.WriteLine("[ApiClient] Auth header added");
        }
    }

    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> action, int maxRetries = 3) where T : class
    {
        var retryPolicy = Policy
            .Handle<HttpRequestException>(ex => 
            {
                // Don't retry 404 errors - they're expected when no journey exists
                if (ex.Message.Contains("404") || ex.Message.Contains("Not Found"))
                {
                    Debug.WriteLine($"[ApiClient] 404 Not Found - skipping retries (expected for missing resources)");
                    return false;
                }
                return true;
            })
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                maxRetries,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    Debug.WriteLine($"[ApiClient] Retry {retryCount}/{maxRetries} after {timeSpan.TotalSeconds}s. Error: {exception.Message}");
                });

        try
        {
            return await retryPolicy.ExecuteAsync(action);
        }
        catch (Exception ex)
        {
            // Don't spam logs for expected 404s
            if (ex.Message.Contains("404") || ex.Message.Contains("Not Found"))
            {
                Debug.WriteLine($"[ApiClient] Resource not found (404) - this is expected");
            }
            else
            {
                Debug.WriteLine($"[ApiClient] All retries exhausted. Error: {ex.Message}");
            }
            throw;
        }
    }

    private async Task<ApiResponse<TResult>> SendRequestAsync<TResult>(
        Func<Task<HttpResponseMessage>> requestFunc,
        string operationName)
    {
        try
        {
            Debug.WriteLine($"[ApiClient] Starting {operationName}");
            
            var response = await ExecuteWithRetryAsync(async () =>
            {
                var result = await requestFunc();
                result.EnsureSuccessStatusCode();
                return result;
            });

            var content = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"[ApiClient] {operationName} - Status: {response.StatusCode}");
            
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<TResult>>(_jsonOptions);
            
            if (result == null)
            {
                Debug.WriteLine($"[ApiClient] {operationName} - Null response");
                return ApiResponse<TResult>.ErrorResponse("Invalid response from server");
            }

            Debug.WriteLine($"[ApiClient] {operationName} - Success: {result.Success}");
            return result;
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine($"[ApiClient] {operationName} - HTTP Error: {ex.Message}");
            return ApiResponse<TResult>.ErrorResponse($"Connection error: {ex.Message}. Please check your internet connection.");
        }
        catch (TaskCanceledException ex)
        {
            Debug.WriteLine($"[ApiClient] {operationName} - Timeout: {ex.Message}");
            return ApiResponse<TResult>.ErrorResponse("Request timeout. Please check your network connection and try again.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ApiClient] {operationName} - Unexpected Error: {ex.Message}");
            return ApiResponse<TResult>.ErrorResponse($"Unexpected error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        return await SendRequestAsync<AuthResponseDto>(
            () => _httpClient.PostAsJsonAsync("api/Auth/register", dto),
            "Register");
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        return await SendRequestAsync<AuthResponseDto>(
            () => _httpClient.PostAsJsonAsync("api/Auth/login", dto),
            "Login");
    }

    public async Task<ApiResponse<JourneyDto>> StartJourneyAsync(StartJourneyDto dto)
    {
        AddAuthHeader();
        return await SendRequestAsync<JourneyDto>(
            () => _httpClient.PostAsJsonAsync("api/Journey/start", dto),
            "StartJourney");
    }

    public async Task<ApiResponse<JourneyDto>> GetActiveJourneyAsync()
    {
        AddAuthHeader();
        return await SendRequestAsync<JourneyDto>(
            () => _httpClient.GetAsync("api/Journey/active"),
            "GetActiveJourney");
    }

    public async Task<ApiResponse<JourneyDto>> EndJourneyAsync(EndJourneyDto dto)
    {
        AddAuthHeader();
        return await SendRequestAsync<JourneyDto>(
            () => _httpClient.PostAsJsonAsync("api/Journey/end", dto),
            "EndJourney");
    }

    public async Task<ApiResponse<SafetyStatusDto>> UpdateSafetyStatusAsync(UpdateSafetyStatusDto dto)
    {
        AddAuthHeader();
        return await SendRequestAsync<SafetyStatusDto>(
            () => _httpClient.PostAsJsonAsync("api/SafetyStatus", dto),
            "UpdateSafetyStatus");
    }

    public async Task<ApiResponse<JourneyDto>> UpdateLocationAsync(UpdateLocationDto dto)
    {
        AddAuthHeader();
        return await SendRequestAsync<JourneyDto>(
            () => _httpClient.PutAsJsonAsync("api/Journey/location", dto),
            "UpdateLocation");
    }

    public async Task<ApiResponse<List<JourneyDto>>> GetJourneyHistoryAsync(int page = 1, int pageSize = 20)
    {
        AddAuthHeader();
        return await SendRequestAsync<List<JourneyDto>>(
            () => _httpClient.GetAsync($"api/Journey/history?page={page}&pageSize={pageSize}"),
            "GetJourneyHistory");
    }

    public async Task<ApiResponse<List<JourneyDto>>> GetMonitoredJourneysAsync()
    {
        AddAuthHeader();
        return await SendRequestAsync<List<JourneyDto>>(
            () => _httpClient.GetAsync("api/Journey/monitoring"),
            "GetMonitoredJourneys");
    }

    public async Task<bool> IsApiAvailableAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var response = await _httpClient.GetAsync("api/health", cts.Token);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
