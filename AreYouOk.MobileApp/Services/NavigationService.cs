using System.Diagnostics;

namespace AreYouOk.MobileApp.Services;

/// <summary>
/// Centralized navigation service for managing app navigation flow
/// </summary>
public class NavigationService
{
    private readonly IServiceProvider _serviceProvider;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task NavigateToMainAsync()
    {
        try
        {
            Debug.WriteLine("[NavigationService] Navigating to MainTabbedPage");
            var mainPage = _serviceProvider.GetRequiredService<Pages.MainTabbedPage>();
            
            if (Application.Current != null)
            {
                Application.Current.MainPage = mainPage;
                Debug.WriteLine("[NavigationService] Successfully navigated to MainTabbedPage");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NavigationService] Error navigating to Main: {ex.Message}");
            throw;
        }
    }

    public async Task NavigateToLoginAsync()
    {
        try
        {
            Debug.WriteLine("[NavigationService] Navigating to Login (AppShell)");
            
            if (Application.Current != null)
            {
                Application.Current.MainPage = new AppShell();
                Debug.WriteLine("[NavigationService] Successfully navigated to Login");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NavigationService] Error navigating to Login: {ex.Message}");
            throw;
        }
    }

    public async Task PushAsync(Page page)
    {
        try
        {
            if (Application.Current?.MainPage?.Navigation != null)
            {
                await Application.Current.MainPage.Navigation.PushAsync(page);
                Debug.WriteLine($"[NavigationService] Pushed page: {page.GetType().Name}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NavigationService] Error pushing page: {ex.Message}");
            throw;
        }
    }

    public async Task PopAsync()
    {
        try
        {
            if (Application.Current?.MainPage?.Navigation != null)
            {
                await Application.Current.MainPage.Navigation.PopAsync();
                Debug.WriteLine("[NavigationService] Popped page");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NavigationService] Error popping page: {ex.Message}");
        }
    }

    public async Task PopToRootAsync()
    {
        try
        {
            if (Application.Current?.MainPage?.Navigation != null)
            {
                await Application.Current.MainPage.Navigation.PopToRootAsync();
                Debug.WriteLine("[NavigationService] Popped to root");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NavigationService] Error popping to root: {ex.Message}");
        }
    }

    public async Task DisplayAlertAsync(string title, string message, string cancel = "OK")
    {
        try
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(title, message, cancel);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NavigationService] Error displaying alert: {ex.Message}");
        }
    }

    public async Task<bool> DisplayConfirmAsync(string title, string message, string accept = "Yes", string cancel = "No")
    {
        try
        {
            if (Application.Current?.MainPage != null)
            {
                return await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NavigationService] Error displaying confirm: {ex.Message}");
            return false;
        }
    }

    public void GoToTab(int tabIndex)
    {
        try
        {
            if (Application.Current?.MainPage is TabbedPage tabbedPage && tabIndex < tabbedPage.Children.Count)
            {
                tabbedPage.CurrentPage = tabbedPage.Children[tabIndex];
                Debug.WriteLine($"[NavigationService] Switched to tab: {tabIndex}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NavigationService] Error switching tab: {ex.Message}");
        }
    }
}
