using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AreYouOk.MobileApp.Services;
using AreYouOk.Shared.DTOs;
using System.Collections.ObjectModel;

namespace AreYouOk.MobileApp.ViewModels;

/// <summary>
/// ViewModel for the home/dashboard page
/// </summary>
public class HomeViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly SettingsService _settingsService;

    private string userName = string.Empty;
    public string UserName
    {
        get => userName;
        set => SetProperty(ref userName, value);
    }

    private bool hasActiveJourney;
    public bool HasActiveJourney
    {
        get => hasActiveJourney;
        set => SetProperty(ref hasActiveJourney, value);
    }

    private JourneyDto? activeJourney;
    public JourneyDto? ActiveJourney
    {
        get => activeJourney;
        set => SetProperty(ref activeJourney, value);
    }

    private bool isLoading;
    public bool IsLoading
    {
        get => isLoading;
        set => SetProperty(ref isLoading, value);
    }

    private string statusMessage = "You don't have an active journey";
    public string StatusMessage
    {
        get => statusMessage;
        set => SetProperty(ref statusMessage, value);
    }

    public HomeViewModel(ApiClient apiClient, SettingsService settingsService)
    {
        _apiClient = apiClient;
        _settingsService = settingsService;
        
        // Initialize user name safely
        UserName = _settingsService.UserFirstName ?? "User";
        
        // Initialize commands explicitly
        LoadActiveJourneyCommand = new AsyncRelayCommand(LoadActiveJourneyAsync);
        StartJourneyCommand = new AsyncRelayCommand(StartJourneyAsync);
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        
        // Initialize ViewActiveJourneyCommand with inline lambda to avoid any naming conflicts
        ViewActiveJourneyCommand = new AsyncRelayCommand(async () =>
        {
            if (ActiveJourney != null)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"Navigating to activejourney. Journey ID: {ActiveJourney.Id}");
                    await Shell.Current.GoToAsync("activejourney");
                    System.Diagnostics.Debug.WriteLine("Navigation successful");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", $"Unable to view journey: {ex.Message}", "OK");
                    }
                }
            }
        });
    }

    public IAsyncRelayCommand LoadActiveJourneyCommand { get; }
    public IAsyncRelayCommand StartJourneyCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand ViewActiveJourneyCommand { get; }

    private async Task LoadActiveJourneyAsync()
    {
        try
        {
            IsLoading = true;
            var response = await _apiClient.GetActiveJourneyAsync();

            if (response?.Success == true && response.Data != null)
            {
                ActiveJourney = response.Data;
                HasActiveJourney = true;
                StatusMessage = $"Journey started at {ActiveJourney.StartTime:hh:mm tt}";
            }
            else
            {
                HasActiveJourney = false;
                ActiveJourney = null;
                StatusMessage = "You don't have an active journey";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading journey status: {ex.Message}");
            StatusMessage = "Error loading journey status";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task StartJourneyAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Navigating to startjourney");
            await Shell.Current.GoToAsync("startjourney");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Unable to start journey. Please try again.", "OK");
            }
        }
    }

    private async Task RefreshAsync()
    {
        await LoadActiveJourneyAsync();
    }

    public async Task InitializeAsync()
    {
        await LoadActiveJourneyAsync();
    }
}
