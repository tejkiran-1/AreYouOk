using AreYouOk.MobileApp.Pages;

namespace AreYouOk.MobileApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes for navigation
        Routing.RegisterRoute("register", typeof(RegisterPage));
        Routing.RegisterRoute("home", typeof(HomePage));
        Routing.RegisterRoute("history", typeof(HistoryPage));
        Routing.RegisterRoute("monitor", typeof(MonitorPage));
        Routing.RegisterRoute("profile", typeof(ProfilePage));
        Routing.RegisterRoute("startjourney", typeof(StartJourneyPage));
        Routing.RegisterRoute("activejourney", typeof(ActiveJourneyPage));
    }
}

