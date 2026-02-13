using AreYouOk.MobileApp.ViewModels;

namespace AreYouOk.MobileApp.Pages;

public partial class HistoryPage : ContentPage
{
    private readonly HistoryViewModel _viewModel;

    public HistoryPage(HistoryViewModel viewModel)
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
