using LevelUp.Mobile.Features.Auth.ViewModels;

namespace LevelUp.Mobile.Features.Auth.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }


}