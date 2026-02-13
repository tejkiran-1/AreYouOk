using AreYouOk.MobileApp.ViewModels;

namespace AreYouOk.MobileApp.Pages;

public partial class ActiveJourneyPage : ContentPage
{
    private readonly ActiveJourneyViewModel _viewModel;

    public ActiveJourneyPage(ActiveJourneyViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Fire-and-forget to avoid blocking UI thread
        _ = _viewModel.InitializeAsync();
    }
}
