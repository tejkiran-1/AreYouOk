using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AreYouOk.MobileApp.Services;
using AreYouOk.Shared.DTOs;

namespace AreYouOk.MobileApp.ViewModels;

/// <summary>
/// ViewModel for user registration
/// </summary>
public partial class RegisterViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    private string firstName = string.Empty;

    [ObservableProperty]
    private string lastName = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string phoneNumber = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string confirmPassword = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public RegisterViewModel(ApiClient apiClient, SettingsService settingsService)
    {
        _apiClient = apiClient;
        _settingsService = settingsService;
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        try
        {
            ErrorMessage = string.Empty;

            // Validation
            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
            {
                ErrorMessage = "Please enter your full name";
                return;
            }

            if (string.IsNullOrWhiteSpace(Email) || !Email.Contains("@"))
            {
                ErrorMessage = "Please enter a valid email address";
                return;
            }

            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                ErrorMessage = "Please enter your phone number";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password) || Password.Length < 6)
            {
                ErrorMessage = "Password must be at least 6 characters";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match";
                return;
            }

            IsLoading = true;

            var registerDto = new RegisterDto
            {
                FirstName = FirstName.Trim(),
                LastName = LastName.Trim(),
                Email = Email.Trim(),
                PhoneNumber = PhoneNumber.Trim(),
                Password = Password,
                ConfirmPassword = ConfirmPassword
            };

            var response = await _apiClient.RegisterAsync(registerDto);

            if (response?.Success == true && response.Data != null)
            {
                // Save user data
                _settingsService.AuthToken = response.Data.Token;
                _settingsService.UserId = response.Data.UserId;
                _settingsService.UserEmail = response.Data.Email;
                _settingsService.UserFirstName = response.Data.FirstName;
                _settingsService.UserLastName = response.Data.LastName;

                // Show TabBar and hide login routes
                if (Application.Current?.MainPage is AppShell shell)
                {
                    shell.ShowTabBar();
                }

                // Navigate to home
                await Shell.Current.GoToAsync("//home");
            }
            else
            {
                ErrorMessage = response?.ErrorMessage ?? "Registration failed. Please try again.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToLoginAsync()
    {
        await Shell.Current.GoToAsync("//login");
    }
}
