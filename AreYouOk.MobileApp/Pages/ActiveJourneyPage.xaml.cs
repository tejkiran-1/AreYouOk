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

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
