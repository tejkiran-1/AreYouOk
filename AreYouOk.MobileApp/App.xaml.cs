using AreYouOk.MobileApp.Services;

namespace AreYouOk.MobileApp;

public partial class App : Application
{
	private readonly SettingsService _settingsService;

	public App(SettingsService settingsService)
	{
		try
		{
			_settingsService = settingsService;
			InitializeComponent();
			MainPage = new AppShell();

			// Use Shell.Loaded event for safer initialization on physical devices
			if (MainPage is AppShell shell)
			{
				shell.Loaded += OnShellLoaded;
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"=== App Constructor Error ===");
			System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
			System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
			throw;
		}
	}

	private async void OnShellLoaded(object? sender, EventArgs e)
	{
		try
		{
			System.Diagnostics.Debug.WriteLine("=== App Startup (Shell Loaded) ===");
			System.Diagnostics.Debug.WriteLine($"IsLoggedIn: {_settingsService.IsLoggedIn}");
			
			// Small delay for Shell to complete initialization
			await Task.Delay(100);
			
			var shell = MainPage as AppShell;
			if (shell == null) return;

			if (_settingsService.IsLoggedIn)
			{
				System.Diagnostics.Debug.WriteLine("User logged in - showing TabBar and navigating to home");
				shell.ShowTabBar();
				await Shell.Current.GoToAsync("//home");
			}
			else
			{
				System.Diagnostics.Debug.WriteLine("User not logged in - showing login page");
				shell.ShowLoginPage();
				await Shell.Current.GoToAsync("//login");
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Startup error: {ex.Message}");
			// On error, show login
			try
			{
				if (MainPage is AppShell shell)
				{
					shell.ShowLoginPage();
					await Shell.Current.GoToAsync("//login");
				}
			}
			catch { /* Ignore navigation errors */ }
		}
	}
}

