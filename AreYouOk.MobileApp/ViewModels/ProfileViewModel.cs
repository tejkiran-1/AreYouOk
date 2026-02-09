using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AreYouOk.MobileApp.Services;
using AreYouOk.MobileApp.Pages;

namespace AreYouOk.MobileApp.ViewModels;

/// <summary>
/// ViewModel for user profile page
/// </summary>
public partial class ProfileViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;
    private readonly ApiClient _apiClient;

    [ObservableProperty]
    private string firstName = string.Empty;

    [ObservableProperty]
    private string lastName = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string phoneNumber = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    public ProfileViewModel(SettingsService settingsService, ApiClient apiClient)
    {
        _settingsService = settingsService;
        _apiClient = apiClient;
        LoadUserData();
    }

    private void LoadUserData()
    {
        FirstName = _settingsService.UserFirstName;
        LastName = _settingsService.UserLastName;
        Email = _settingsService.UserEmail;
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
if (Application.Current?.MainPage == null)
                return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
            "Logout",
            "Are you sure you want to logout?",
            "Yes",
            "No");

        if (confirm)
        {
            _settingsService.ClearUserData();
            
            // Navigate back to AppShell (login page) after logout
            Application.Current.MainPage = new AppShell();
        }
    }

    [RelayCommand]
    private async Task ChangeApiUrlAsync()
    {
        if (Application.Current?.MainPage == null)
            return;

        string? newUrl = await Application.Current.MainPage.DisplayPromptAsync(
            "API Base URL",
            "Enter the API base URL:",
            initialValue: _settingsService.ApiBaseUrl,
            placeholder: "https://localhost:7001");

        if (!string.IsNullOrWhiteSpace(newUrl))
        {
            _settingsService.ApiBaseUrl = newUrl.TrimEnd('/');
            
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert("Success", "API URL updated. Please restart the app.", "OK");
            }
        }
    }
}
