namespace AreYouOk.MobileApp.Pages;

public partial class MainTabbedPage : TabbedPage
{
	public MainTabbedPage(
		HomePage homePage,
		HistoryPage historyPage,
		MonitorPage monitorPage,
		ProfilePage profilePage)
	{
		InitializeComponent();

		// Add pages programmatically to support dependency injection
		homePage.Title = "Home";
		homePage.IconImageSource = "🏠";
		Children.Add(homePage);

		historyPage.Title = "History";
		historyPage.IconImageSource = "📜";
		Children.Add(historyPage);

		monitorPage.Title = "Monitor";
		monitorPage.IconImageSource = "👥";
		Children.Add(monitorPage);

		profilePage.Title = "Profile";
		profilePage.IconImageSource = "👤";
		Children.Add(profilePage);
	}
}
