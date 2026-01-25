using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AreYouOk.MobileApp.Services;
using AreYouOk.Shared.DTOs;
using System.Collections.ObjectModel;

namespace AreYouOk.MobileApp.ViewModels;

/// <summary>
/// ViewModel for the active journey page with safety buttons
/// </summary>
public partial class ActiveJourneyViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly LocationService _locationService;
    private System.Threading.Timer? _locationUpdateTimer;

    [ObservableProperty]
    private JourneyDto? journey;

    [ObservableProperty]
    private ObservableCollection<FamilyMemberDto> familyMembers = new();

    [ObservableProperty]
    private string timeUntilNextCheckIn = string.Empty;

    [ObservableProperty]
    private double progressValue;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public IAsyncRelayCommand<bool> UpdateSafetyStatusCommand { get; }

    public ActiveJourneyViewModel(ApiClient apiClient, LocationService locationService)
    {
        _apiClient = apiClient;
        _locationService = locationService;
        
        // Initialize command explicitly with proper bool type parameter
        UpdateSafetyStatusCommand = new AsyncRelayCommand<bool>(UpdateSafetyStatusAsync);
    }

    [RelayCommand]
    private async Task LoadJourneyAsync()
    {
        try
        {
            IsLoading = true;
            var response = await _apiClient.GetActiveJourneyAsync();

            if (response?.Success == true && response.Data != null)
            {
                Journey = response.Data;
                FamilyMembers = new ObservableCollection<FamilyMemberDto>(Journey.FamilyMembers);
                UpdateTimeDisplay();
                StartLocationUpdates();
            }
            else
            {
                StatusMessage = "No active journey found";
                try
                {
                    await Shell.Current.Navigation.PopAsync();
                }
                catch
                {
                    // Ignore navigation errors
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"LoadJourney Error: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task UpdateSafetyStatusAsync(bool isSafe)
    {
        try
        {
            IsLoading = true;

            var location = await _locationService.GetCurrentLocationAsync();

            var updateDto = new UpdateSafetyStatusDto
            {
                IsSafe = isSafe,
                Latitude = location?.Latitude,
                Longitude = location?.Longitude,
                Message = isSafe ? "I'm safe and doing well" : "Need help - not safe!"
            };

            var response = await _apiClient.UpdateSafetyStatusAsync(updateDto);

            if (response?.Success == true)
            {
                StatusMessage = isSafe ? "✓ Family members notified you're safe" : "⚠ Alert sent to family members!";
                await LoadJourneyAsync();
            }
            else
            {
                StatusMessage = "Failed to update status";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task EndJourneyAsync()
    {
        try
        {
            if (Application.Current?.MainPage == null)
                return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "End Journey",
                "Are you sure you want to end this journey?",
                "Yes",
                "No");

            if (!confirm) return;

            IsLoading = true;

            // Get current location
            var location = await _locationService.GetCurrentLocationAsync();
            
            var endJourneyDto = new EndJourneyDto
            {
                Latitude = location?.Latitude,
                Longitude = location?.Longitude
            };
            
            var response = await _apiClient.EndJourneyAsync(endJourneyDto);

            if (response?.Success == true)
            {
                StopLocationUpdates();
                // Navigate back to home page
                try
                {
                    // Pop back to previous page (HomePage)
                    await Shell.Current.Navigation.PopAsync();
                }
                catch
                {
                    // If PopAsync fails, try going to home tab
                    await Shell.Current.GoToAsync("//home");
                }
            }
            else
            {
                StatusMessage = "Failed to end journey";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void UpdateTimeDisplay()
    {
        if (Journey == null) return;

        var timeRemaining = Journey.TimeUntilNextCheckIn;
        if (timeRemaining.HasValue && timeRemaining.Value.TotalHours > 0)
        {
            TimeUntilNextCheckIn = $"{timeRemaining.Value.Hours}h {timeRemaining.Value.Minutes}m remaining";
            ProgressValue = 1 - (timeRemaining.Value.TotalHours / Journey.CheckInIntervalHours);
        }
        else
        {
            TimeUntilNextCheckIn = "⚠ Check-in overdue!";
            ProgressValue = 1;
        }

        // Schedule next update
        Device.StartTimer(TimeSpan.FromMinutes(1), () =>
        {
            UpdateTimeDisplay();
            return true;
        });
    }

    private void StartLocationUpdates()
    {
        // Update location every 5 minutes
        _locationUpdateTimer = new System.Threading.Timer(async _ =>
        {
            try
            {
                var location = await _locationService.GetCurrentLocationAsync();
                if (location != null && Journey != null)
                {
                    var updateLocationDto = new UpdateLocationDto
                    {
                        Latitude = location.Latitude,
                        Longitude = location.Longitude
                    };
                    await _apiClient.UpdateLocationAsync(updateLocationDto);
                }
            }
            catch { }
        }, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
    }

    private void StopLocationUpdates()
    {
        _locationUpdateTimer?.Dispose();
        _locationUpdateTimer = null;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await LoadJourneyAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Initialize Error: {ex}");
            StatusMessage = "Error loading journey";
        }
    }
}
