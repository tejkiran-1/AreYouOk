using AreYouOk.MobileApp.ViewModels;

namespace AreYouOk.MobileApp.Pages;

public partial class MonitorPage : ContentPage
{
    private readonly MonitorViewModel _viewModel;

    public MonitorPage(MonitorViewModel viewModel)
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
