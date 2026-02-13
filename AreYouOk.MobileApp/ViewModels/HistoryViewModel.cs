using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AreYouOk.MobileApp.Services;
using AreYouOk.Shared.DTOs;
using System.Collections.ObjectModel;

namespace AreYouOk.MobileApp.ViewModels;

/// <summary>
/// ViewModel for journey history page
/// </summary>
public partial class HistoryViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private bool _isInitialized = false;

    [ObservableProperty]
    private ObservableCollection<JourneyDto> journeys = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isEmpty;

    [ObservableProperty]
    private string emptyMessage = "No journey history yet";

    public HistoryViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [RelayCommand]
    private async Task LoadHistoryAsync()
    {
        try
        {
            IsLoading = true;
            var response = await _apiClient.GetJourneyHistoryAsync();

            if (response?.Success == true && response.Data != null)
            {
                Journeys = new ObservableCollection<JourneyDto>(response.Data);
                IsEmpty = Journeys.Count == 0;
            }
            else
            {
                Journeys.Clear();
                IsEmpty = true;
            }
        }
        catch (Exception ex)
        {
            EmptyMessage = $"Error loading history: {ex.Message}";
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
        await LoadHistoryAsync();
    }

    [RelayCommand]
    private async Task ViewJourneyDetailsAsync(JourneyDto journey)
    {
        if (Application.Current?.MainPage == null)
            return;

        await Application.Current.MainPage.DisplayAlert(
            "Journey Details",
            $"Started: {journey.StartTime:g}\n" +
            $"Ended: {journey.EndTime:g}\n" +
            $"Status: {journey.Status}\n" +
            $"Family Members: {journey.FamilyMembers.Count}\n" +
            $"Check-in Interval: {journey.CheckInIntervalHours} hours",
            "OK");
    }

    public async Task InitializeAsync()
    {
        // Only load on first appearance to avoid blocking UI on tab switches
        if (!_isInitialized)
        {
            _isInitialized = true;
            await LoadHistoryAsync();
        }
    }
}
