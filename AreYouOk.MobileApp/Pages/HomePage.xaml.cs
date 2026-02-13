using AreYouOk.MobileApp.ViewModels;

namespace AreYouOk.MobileApp.Pages;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;

    public HomePage(HomeViewModel viewModel)
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
