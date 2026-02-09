using AreYouOk.MobileApp.Services;
using AreYouOk.MobileApp.Pages;
using AreYouOk.MobileApp.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace AreYouOk.MobileApp;

public partial class App : Application
{
	private readonly SettingsService _settingsService;
	private readonly DatabaseService _databaseService;
	private readonly NavigationService _navigationService;
	private readonly IServiceProvider _serviceProvider;
	private bool _isInitialized = false;

	public App(
		SettingsService settingsService,
		DatabaseService databaseService,
		NavigationService navigationService,
		IServiceProvider serviceProvider)
	{
		try
		{
			Debug.WriteLine("=== App Constructor Started ===");
			
			_settingsService = settingsService;
			_databaseService = databaseService;
			_navigationService = navigationService;
			_serviceProvider = serviceProvider;
			
			InitializeComponent();
			
			// Show loading page immediately
			MainPage = new ContentPage
			{
				Content = new VerticalStackLayout
				{
					Spacing = 20,
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					Children =
					{
						new ActivityIndicator { IsRunning = true, Color = Colors.Blue },
						new Label { Text = "Loading AreYouOk...", HorizontalOptions = LayoutOptions.Center }
					}
				}
			};

			Debug.WriteLine("=== App Constructor Completed ===");
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"=== App Constructor Error ===");
			Debug.WriteLine($"Exception: {ex.Message}");
			Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
			
			// Set a fallback page
			MainPage = new ContentPage
			{
				Content = new Label
				{
					Text = $"App initialization error: {ex.Message}",
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				}
			};
		}
	}

	private async Task InitializeAppAsync()
	{
		if (_isInitialized)
			return;

		try
		{
			Debug.WriteLine("[App] Starting async initialization...");
			
			// Step 1: Initialize database
			await _databaseService.InitializeAsync();
			Debug.WriteLine("[App] Database initialized successfully");
			
			// Step 2: Give UI thread a moment
			await Task.Delay(100);
			
			// Step 3: Set initial page based on login status
			await MainThread.InvokeOnMainThreadAsync(() =>
			{
				SetInitialPage();
			});

			_isInitialized = true;
			Debug.WriteLine("[App] Async initialization completed");
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"[App] Initialization error: {ex.Message}");
			Debug.WriteLine($"[App] Stack trace: {ex.StackTrace}");
			
			// Fallback to login on error
			await MainThread.InvokeOnMainThreadAsync(() =>
			{
				MainPage = new AppShell();
			});
		}
	}

	private void SetInitialPage()
	{
		try
		{
			Debug.WriteLine($"[App] Checking login status...");
			
			bool isLoggedIn = false;
			try
			{
				isLoggedIn = _settingsService.IsLoggedIn;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"[App] Error checking login status: {ex.Message}");
			}
			
			Debug.WriteLine($"[App] IsLoggedIn: {isLoggedIn}");
			
			if (isLoggedIn)
			{
				Debug.WriteLine("[App] User is logged in - showing MainTabbedPage");
				try
				{
					MainPage = _serviceProvider.GetRequiredService<MainTabbedPage>();
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"[App] Error creating MainTabbedPage: {ex.Message}");
					Debug.WriteLine("[App] Falling back to login");
					MainPage = new AppShell();
				}
			}
			else
			{
				Debug.WriteLine("[App] User is NOT logged in - showing AppShell (login page)");
				MainPage = new AppShell();
			}
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"[App] Error setting initial page: {ex.Message}");
			Debug.WriteLine($"[App] Stack trace: {ex.StackTrace}");
			// Fallback to login
			MainPage = new AppShell();
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		Debug.WriteLine("[App] OnStart called");
	}

	protected override void OnSleep()
	{
		base.OnSleep();
		Debug.WriteLine("[App] OnSleep called");
	}

	protected override void OnResume()
	{
		base.OnResume();
		Debug.WriteLine("[App] OnResume called");
		
		// Check if user was logged out while app was asleep
		if (!_settingsService.IsLoggedIn && MainPage is not AppShell)
		{
			Debug.WriteLine("[App] User logged out during sleep - returning to login");
			MainPage = new AppShell();
		}
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = base.CreateWindow(activationState);
		
		// Set window properties
		window.Title = "AreYouOk";
		
		// Handle window lifecycle events
		window.Created += async (s, e) => 
		{
			Debug.WriteLine("[App] Window created");
			// Initialize app after window is created
			await InitializeAppAsync();
		};
		
		window.Activated += (s, e) => Debug.WriteLine("[App] Window activated");
		window.Deactivated += (s, e) => Debug.WriteLine("[App] Window deactivated");
		window.Stopped += (s, e) => Debug.WriteLine("[App] Window stopped");
		window.Resumed += async (s, e) => 
		{
			Debug.WriteLine("[App] Window resumed");
			
			// Reinitialize if needed
			if (!_isInitialized)
			{
				Debug.WriteLine("[App] App not initialized, reinitializing...");
				await InitializeAppAsync();
			}
			else
			{
				// Verify auth token is still valid
				try
				{
					if (_settingsService.IsLoggedIn && _settingsService.IsTokenExpiringSoon())
					{
						Debug.WriteLine("[App] Token expiring soon - consider refreshing");
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"[App] Error checking token status: {ex.Message}");
				}
			}
		};
		window.Destroying += (s, e) => Debug.WriteLine("[App] Window destroying");

		return window;
	}
}
