using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AreYouOk.MobileApp.Services;
using AreYouOk.MobileApp.Data;
using AreYouOk.MobileApp.Data.Entities;
using AreYouOk.Shared.DTOs;
using System.Diagnostics;

namespace AreYouOk.MobileApp.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly SettingsService _settingsService;
    private readonly DatabaseService _databaseService;
    private readonly NavigationService _navigationService;

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

    public RegisterViewModel(
        ApiClient apiClient,
        SettingsService settingsService,
        DatabaseService databaseService,
        NavigationService navigationService)
    {
        _apiClient = apiClient;
        _settingsService = settingsService;
        _databaseService = databaseService;
        _navigationService = navigationService;
        
        Debug.WriteLine("[RegisterViewModel] Initialized");
    }

    [RelayCommand]
    private async Task RegisterAsync()
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

        try
        {
            Debug.WriteLine($"[RegisterViewModel] Attempting registration for {Email}");
            
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
                Debug.WriteLine("[RegisterViewModel] Registration successful");
                
                // Save user session
                _settingsService.SaveUserSession(
                    response.Data.UserId,
                    response.Data.Email,
                    response.Data.FirstName,
                    response.Data.LastName,
                    response.Data.Token,
                    DateTime.UtcNow.AddDays(30)
                );
                
                _settingsService.UserPhoneNumber = PhoneNumber.Trim();

                // Save to database
                var userEntity = new UserEntity
                {
                    ServerId = response.Data.UserId,
                    Email = response.Data.Email,
                    FirstName = response.Data.FirstName,
                    LastName = response.Data.LastName,
                    PhoneNumber = PhoneNumber.Trim(),
                    AuthToken = response.Data.Token,
                    TokenExpiresAt = DateTime.UtcNow.AddDays(30),
                    IsActive = true
                };
                
                await _databaseService.SaveUserAsync(userEntity);
                Debug.WriteLine("[RegisterViewModel] User saved to local database");

                // Navigate to main page
                await _navigationService.NavigateToMainAsync();
            }
            else
            {
                ErrorMessage = response?.ErrorMessage ?? "Registration failed. Please try again.";
                Debug.WriteLine($"[RegisterViewModel] Registration failed: {ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Connection error. Please check your internet and try again.";
            Debug.WriteLine($"[RegisterViewModel] Registration error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToLoginAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("//login");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[RegisterViewModel] Navigation error: {ex.Message}");
        }
    }

    partial void OnEmailChanged(string value)
    {
        ErrorMessage = string.Empty;
    }

    partial void OnPasswordChanged(string value)
    {
        ErrorMessage = string.Empty;
    }

    partial void OnConfirmPasswordChanged(string value)
    {
        ErrorMessage = string.Empty;
    }
}
