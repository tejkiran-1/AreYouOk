using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AreYouOk.MobileApp.Services;
using AreYouOk.Shared.DTOs;
using AreYouOk.Shared.Enums;
using System.Collections.ObjectModel;

namespace AreYouOk.MobileApp.ViewModels;

/// <summary>
/// ViewModel for monitoring family members' journeys
/// </summary>
public partial class MonitorViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private bool _isInitialized = false;

    [ObservableProperty]
    private ObservableCollection<JourneyDto> monitoredJourneys = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isEmpty;

    [ObservableProperty]
    private string emptyMessage = "You're not monitoring any journeys";

    public MonitorViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [RelayCommand]
    private async Task LoadMonitoredJourneysAsync()
    {
        try
        {
            IsLoading = true;
            var response = await _apiClient.GetMonitoredJourneysAsync();

            if (response?.Success == true && response.Data != null)
            {
                MonitoredJourneys = new ObservableCollection<JourneyDto>(response.Data);
                IsEmpty = MonitoredJourneys.Count == 0;
            }
            else
            {
                MonitoredJourneys.Clear();
                IsEmpty = true;
            }
        }
        catch (Exception ex)
        {
            EmptyMessage = $"Error loading monitored journeys: {ex.Message}";
            IsEmpty = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadMonitoredJourneysAsync();
    }

    [RelayCommand]
    private async Task ViewJourneyDetailsAsync(JourneyDto journey)
    {
        var status = journey.Status == "Alert" ? "⚠ ALERT - Check on them!" : "✓ Safe";
        var timeInfo = journey.TimeUntilNextCheckIn.HasValue && journey.TimeUntilNextCheckIn.Value.TotalHours > 0
            ? $"Next check-in: {journey.TimeUntilNextCheckIn.Value.Hours}h {journey.TimeUntilNextCheckIn.Value.Minutes}m"
            : "⚠ Check-in overdue!";

        await Application.Current!.MainPage!.DisplayAlert(
            $"Journey: {journey.UserName}",
            $"Status: {status}\n" +
            $"Started: {journey.StartTime:g}\n" +
            $"{timeInfo}\n" +
            $"Check-in Interval: {journey.CheckInIntervalHours} hours\n" +
            $"Last Update: {journey.LastCheckInTime:g}",
            "OK");
    }

    public async Task InitializeAsync()
    {
        // Only load on first appearance to avoid blocking UI on tab switches
        if (!_isInitialized)
        {
            _isInitialized = true;
            await LoadMonitoredJourneysAsync();
        }
    }
}
