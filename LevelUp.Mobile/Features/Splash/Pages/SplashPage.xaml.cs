
using LevelUp.Mobile.Features.Splash.ViewModels;

namespace LevelUp.Mobile.Features.Splash.Pages;

public partial class SplashPage : ContentPage
{
    private readonly SplashViewModel _vm;

    public SplashPage(SplashViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    // OnAppearing es el lugar correcto para disparar la lógica de inicio.
    // Se ejecuta cada vez que la página se vuelve visible.
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await Task.WhenAll(
            MainLogo.FadeToAsync(1, 800),
            MainLogo.ScaleToAsync(1.0, 1000, Easing.CubicOut)
        );

        await _vm.InitializeAsync();
    }
}
