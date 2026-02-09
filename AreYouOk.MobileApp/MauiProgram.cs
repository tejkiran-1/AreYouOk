using AreYouOk.MobileApp.Services;
using AreYouOk.MobileApp.Pages;
using AreYouOk.MobileApp.ViewModels;
using AreYouOk.MobileApp.Data;
using CommunityToolkit.Maui;

namespace AreYouOk.MobileApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Register core services as singletons
		builder.Services.AddSingleton<SettingsService>();
		builder.Services.AddSingleton<LocationService>();
		builder.Services.AddSingleton<DatabaseService>();
		builder.Services.AddSingleton<NavigationService>();
		
		// Configure HttpClient for ApiClient with proper timeout and base address
		builder.Services.AddHttpClient<ApiClient>((serviceProvider, client) =>
		{
			var settingsService = serviceProvider.GetRequiredService<SettingsService>();
			client.BaseAddress = new Uri(settingsService.ApiBaseUrl);
			client.Timeout = TimeSpan.FromSeconds(60);
		});

		// Register ViewModels as transient (new instance each time)
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<RegisterViewModel>();
		builder.Services.AddTransient<HomeViewModel>();
		builder.Services.AddTransient<StartJourneyViewModel>();
		builder.Services.AddTransient<ActiveJourneyViewModel>();
		builder.Services.AddTransient<HistoryViewModel>();
		builder.Services.AddTransient<MonitorViewModel>();
		builder.Services.AddTransient<ProfileViewModel>();

		// Register Pages as transient
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<RegisterPage>();
		builder.Services.AddTransient<HomePage>();
		builder.Services.AddTransient<StartJourneyPage>();
		builder.Services.AddTransient<ActiveJourneyPage>();
		builder.Services.AddTransient<HistoryPage>();
		builder.Services.AddTransient<MonitorPage>();
		builder.Services.AddTransient<ProfilePage>();
		builder.Services.AddTransient<MainTabbedPage>();

		return builder.Build();
	}
}



