using LevelUp.Mobile.Features.Auth.ViewModels;

namespace LevelUp.Mobile.Features.Auth.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }

    // En LoginPage.xaml.cs
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is LoginViewModel vm && vm.IsNotLoggedIn)
        {
            var stack = this.FindByName<VerticalStackLayout>("LoginControlsStack"); 
            stack.Opacity = 0;
            stack.TranslationY = 30;

            await Task.Delay(200); 
            await Task.WhenAll(
                stack.FadeToAsync(1, 600, Easing.CubicOut),
                stack.TranslateToAsync(0, 0, 600, Easing.CubicOut)
            );
        }
    }


}