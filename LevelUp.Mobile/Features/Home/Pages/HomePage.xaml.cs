using LevelUp.Mobile.Features.Home.ViewModels;

namespace LevelUp.Mobile.Features.Home.Pages;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;

    public HomePage(HomeViewModel viewModel)
    {
        BindingContext = viewModel;
        _viewModel = viewModel;
        InitializeComponent();
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateTogglePosition(animated: false);
        _viewModel.InitializeCommand.Execute(null);
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(HomeViewModel.IsDarkTheme)) return;

        // Ejecutar en main thread con delay para que el re-render del tema termine primero
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Task.Delay(50); // espera que MAUI aplique el cambio de tema
            UpdateTogglePosition(animated: true);
        });
    }

    private void UpdateTogglePosition(bool animated)
    {
        double targetX = _viewModel.IsDarkTheme ? 28 : 0;
        ThumbIcon.Text = _viewModel.IsDarkTheme ? "🌙" : "☀️";

        if (animated)
            ThumbIndicator.TranslateTo(targetX, 0, 220, Easing.CubicInOut);
        else
            ThumbIndicator.TranslationX = targetX;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }
}