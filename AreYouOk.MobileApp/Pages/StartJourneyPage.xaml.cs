using AreYouOk.MobileApp.ViewModels;

namespace AreYouOk.MobileApp.Pages;

public partial class StartJourneyPage : ContentPage
{
    public StartJourneyPage(StartJourneyViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
