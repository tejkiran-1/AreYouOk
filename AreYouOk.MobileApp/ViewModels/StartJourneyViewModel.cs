using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AreYouOk.MobileApp.Services;
using AreYouOk.MobileApp.Models;
using AreYouOk.Shared.DTOs;
using System.Collections.ObjectModel;

namespace AreYouOk.MobileApp.ViewModels;

/// <summary>
/// ViewModel for starting a new journey
/// </summary>
public partial class StartJourneyViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly LocationService _locationService;

    [ObservableProperty]
    private ObservableCollection<FamilyMemberInput> familyMembers = new();

    [ObservableProperty]
    private double checkInIntervalHours = 10;

    [ObservableProperty]
    private string newPhoneNumber = string.Empty;

    [ObservableProperty]
    private string newDisplayName = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public StartJourneyViewModel(ApiClient apiClient, LocationService locationService)
    {
        _apiClient = apiClient;
        _locationService = locationService;
    }

    [RelayCommand]
    private void AddFamilyMember()
    {
        if (string.IsNullOrWhiteSpace(NewPhoneNumber))
        {
            ErrorMessage = "Please enter a phone number";
            return;
        }

        // Check if already added
        if (FamilyMembers.Any(fm => fm.PhoneNumber == NewPhoneNumber.Trim()))
        {
            ErrorMessage = "This phone number has already been added";
            return;
        }

        var member = new FamilyMemberInput
        {
            PhoneNumber = NewPhoneNumber.Trim(),
            DisplayName = string.IsNullOrWhiteSpace(NewDisplayName) ? NewPhoneNumber.Trim() : NewDisplayName.Trim()
        };
        
        FamilyMembers.Add(member);
        OnPropertyChanged(nameof(FamilyMembers));

        NewPhoneNumber = string.Empty;
        NewDisplayName = string.Empty;
        ErrorMessage = string.Empty;
        
        System.Diagnostics.Debug.WriteLine($"Family member added: {member.DisplayName} ({member.PhoneNumber}). Total: {FamilyMembers.Count}");
    }

    [RelayCommand]
    private void RemoveFamilyMember(FamilyMemberInput member)
    {
        FamilyMembers.Remove(member);
    }

    [RelayCommand]
    private async Task StartJourneyAsync()
    {
        try
        {
            ErrorMessage = string.Empty;

            if (FamilyMembers.Count == 0)
            {
                ErrorMessage = "Please add at least one family member";
                return;
            }

            if (CheckInIntervalHours < 0.5 || CheckInIntervalHours > 168)
            {
                ErrorMessage = "Check-in interval must be between 0.5 and 168 hours";
                return;
            }

            IsLoading = true;

            // Get current location
            var location = await _locationService.GetCurrentLocationAsync();

            var startJourneyDto = new StartJourneyDto
            {
                FamilyMemberPhones = FamilyMembers.Select(fm => fm.PhoneNumber).ToList(),
                CheckInIntervalHours = CheckInIntervalHours,
                Latitude = location?.Latitude,
                Longitude = location?.Longitude
            };

            var response = await _apiClient.StartJourneyAsync(startJourneyDto);

            System.Diagnostics.Debug.WriteLine($"StartJourney Response: Success={response?.Success}, Error={response?.ErrorMessage}");

            if (response?.Success == true)
            {
                // Clear family members list
                FamilyMembers.Clear();
                
                // Navigate back to home and then to active journey
                try
                {
                    // Pop back to home first
                    await Shell.Current.Navigation.PopToRootAsync();
                    // Then navigate to active journey
                    await Shell.Current.GoToAsync("activejourney");
                }
                catch
                {
                    // Fallback: just close this page
                    await Shell.Current.Navigation.PopAsync();
                }
            }
            else
            {
                ErrorMessage = response?.ErrorMessage ?? "Failed to start journey. Please try again.";
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
    private async Task CancelAsync()
    {
        try
        {
            if (Shell.Current.Navigation.NavigationStack.Count > 1)
            {
                await Shell.Current.Navigation.PopAsync();
            }
            else
            {
                await Shell.Current.GoToAsync("//home");
            }
        }
        catch
        {
            await Shell.Current.GoToAsync("//home");
        }
    }
}
