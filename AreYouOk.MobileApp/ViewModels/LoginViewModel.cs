using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AreYouOk.MobileApp.Services;
using AreYouOk.Shared.DTOs;

namespace AreYouOk.MobileApp.ViewModels;

/// <summary>
/// ViewModel for the Login page
/// Example implementation showing MVVM pattern
/// </summary>
public partial class LoginViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public LoginViewModel(ApiClient apiClient, SettingsService settingsService)
    {
        _apiClient = apiClient;
        _settingsService = settingsService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter email and password";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var loginDto = new LoginDto
            {
                Email = Email,
                Password = Password
            };

            var result = await _apiClient.LoginAsync(loginDto);

            if (result.Success && result.Data != null)
            {
                // Save user data
                _settingsService.AuthToken = result.Data.Token;
                _settingsService.UserId = result.Data.UserId;
                _settingsService.UserEmail = result.Data.Email;
                _settingsService.UserFirstName = result.Data.FirstName;
                _settingsService.UserLastName = result.Data.LastName;

                // Show TabBar and hide login routes
                if (Application.Current?.MainPage is AppShell shell)
                {
                    shell.ShowTabBar();
                }

                // Navigate to home page
                await Shell.Current.GoToAsync("//home");
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Login failed";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToRegisterAsync()
    {
        await Shell.Current.GoToAsync("register");
    }
}
