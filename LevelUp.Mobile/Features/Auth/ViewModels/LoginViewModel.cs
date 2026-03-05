using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Features.Auth.Services;
using LevelUp.Mobile.Infrastructure.Token;

namespace LevelUp.Mobile.Features.Auth.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly AuthService _authService;
    private readonly ITokenService _tokenService;

    public LoginViewModel(AuthService authService, ITokenService tokenService)
    {
        _authService = authService;
        _tokenService = tokenService;
    }

    // ── Estado de sesión ──────────────────────────────────────────────
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotLoggedIn))]
    private bool _isLoggedIn;

    [ObservableProperty] private string? _userName;
    [ObservableProperty] private string? _userEmail;
    [ObservableProperty] private string? _profilePhotoUrl;

    // Propiedades calculadas — sin necesidad de converters en XAML
    public bool IsNotLoggedIn => !IsLoggedIn;


    // ── Commands ──────────────────────────────────────────────────────
    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginWithGoogleAsync()
    {
        IsBusy = true;

        try
        {
            var result = await _authService.LoginWithGoogleAsync();

            if (!result.IsSuccess) return;

            var claims = await _tokenService.GetUserClaimsAsync();

            if (claims.TryGetValue("userName", out var name)) UserName = name;
            if (claims.TryGetValue("email", out var email)) UserEmail = email;
            if (claims.TryGetValue("picture", out var picture)) ProfilePhotoUrl = picture;

            IsLoggedIn = true;

            // Navegar al área autenticada — el TabBar aparece automáticamente
            await Shell.Current.GoToAsync("//Home");
        }
        finally
        {
            IsBusy = false;
            LoginWithGoogleCommand.NotifyCanExecuteChanged(); // refresca el botón
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _authService.LogoutAsync();

        IsLoggedIn = false;
        UserName = null;
        UserEmail = null;
        ProfilePhotoUrl = null;

        await Shell.Current.GoToAsync("//Login");
    }

    private bool CanLogin() => !IsBusy;

}
