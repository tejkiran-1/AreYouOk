using AreYouOk.MobileApp.ViewModels;

namespace AreYouOk.MobileApp.Pages;

public partial class ProfilePage : ContentPage
{
    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
