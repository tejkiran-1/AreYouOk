using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AreYouOk.MobileApp.Services;
using AreYouOk.MobileApp.Data;
using AreYouOk.MobileApp.Data.Entities;
using AreYouOk.Shared.DTOs;
using System.Diagnostics;

namespace AreYouOk.MobileApp.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly SettingsService _settingsService;
    private readonly DatabaseService _databaseService;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool rememberMe = true;

    public LoginViewModel(
        ApiClient apiClient,
        SettingsService settingsService,
        DatabaseService databaseService,
        NavigationService navigationService)
    {
        _apiClient = apiClient;
        _settingsService = settingsService;
        _databaseService = databaseService;
        _navigationService = navigationService;
        
        Debug.WriteLine("[LoginViewModel] Initialized");
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
            Debug.WriteLine($"[LoginViewModel] Attempting login for {Email}");
            
            var loginDto = new LoginDto
            {
                Email = Email,
                Password = Password
            };

            var result = await _apiClient.LoginAsync(loginDto);

            if (result.Success && result.Data != null)
            {
                Debug.WriteLine("[LoginViewModel] Login successful");
                
                // Save user session to preferences
                _settingsService.SaveUserSession(
                    result.Data.UserId,
                    result.Data.Email,
                    result.Data.FirstName,
                    result.Data.LastName,
                    result.Data.Token,
                    DateTime.UtcNow.AddDays(30) // Token expires in 30 days
                );

                // Save user to local database
                if (RememberMe)
                {
                    var userEntity = new UserEntity
                    {
                        ServerId = result.Data.UserId,
                        Email = result.Data.Email,
                        FirstName = result.Data.FirstName,
                        LastName = result.Data.LastName,
                        AuthToken = result.Data.Token,
                        TokenExpiresAt = DateTime.UtcNow.AddDays(30),
                        IsActive = true
                    };
                    
                    await _databaseService.SaveUserAsync(userEntity);
                    Debug.WriteLine("[LoginViewModel] User saved to local database");
                }

                // Navigate to main page
                await _navigationService.NavigateToMainAsync();
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Login failed. Please check your credentials.";
                Debug.WriteLine($"[LoginViewModel] Login failed: {ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Connection error. Please check your internet and try again.";
            Debug.WriteLine($"[LoginViewModel] Login error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToRegisterAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("register");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LoginViewModel] Navigation error: {ex.Message}");
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
}
