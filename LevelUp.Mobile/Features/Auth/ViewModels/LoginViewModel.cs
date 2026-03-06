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
#if DEBUG
            var googleToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6IjI1MDdmNTFhZjJhMTYyNDY3MDc0ODQ2NzRhNDJhZTNjMmI2MjMxOWMiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2FjY291bnRzLmdvb2dsZS5jb20iLCJhenAiOiI0MDc0MDg3MTgxOTIuYXBwcy5nb29nbGV1c2VyY29udGVudC5jb20iLCJhdWQiOiI0MDc0MDg3MTgxOTIuYXBwcy5nb29nbGV1c2VyY29udGVudC5jb20iLCJzdWIiOiIxMTQ3MDIxOTc3NzcyMjE4MDcwNTYiLCJlbWFpbCI6ImFuZGVyc29ucG9sYW5jb2NvbnRyZXJhc0BnbWFpbC5jb20iLCJlbWFpbF92ZXJpZmllZCI6dHJ1ZSwiYXRfaGFzaCI6Ii0zazVQSHFuZExVZ2ZuWFFWN2JERXciLCJpYXQiOjE3NzI4Mjk2NzMsImV4cCI6MTc3MjgzMzI3M30.XhItYIc_BPwtTH8BEAf-Zl2zn9Ie53aJIt2Em9b89QbI59bLV63olOS8Z4NrsZwpLhvCFXQxC1ALiD0hsqEZCzU76W5zFldMknRV-qlYML9ibrGYTln6DI9AcYZ5yCw4hLkh_h-fRuXUjqo3qTpakT7sx2h_NmbNhooRmCXGGHqyCfIA2o1q5UlP957qxPtCgYt59U2TGqW-k4bT_p7WYFC3U1KlP1JnljXQ-PAXuKvmD1ODm-bzvaAMM-21wBROE1C-fWevhZ1i7wMz6JidL0E25Vlnfi9Gf05rmvq0E71HZgxZ89Q2G28TooKQn8WVBm385KDBiniISkoUdv7b4Q";
            var result = await _authService.LoginWithGoogleAsync(googleToken);

            if (!result.IsSuccess) return;

            var claims = await _tokenService.GetUserClaimsAsync();

            if (claims.TryGetValue("userName", out var name)) UserName = name;
            if (claims.TryGetValue("email", out var email)) UserEmail = email;
            if (claims.TryGetValue("picture", out var picture)) ProfilePhotoUrl = picture;

            IsLoggedIn = true;

            // Navegar al área autenticada — el TabBar aparece automáticamente
            await Shell.Current.GoToAsync("//Home");


#else
            var result = await _authService.LoginWithGoogleAsync();

            if (!result.IsSuccess) return;

            var claims = await _tokenService.GetUserClaimsAsync();

            if (claims.TryGetValue("userName", out var name)) UserName = name;
            if (claims.TryGetValue("email", out var email)) UserEmail = email;
            if (claims.TryGetValue("picture", out var picture)) ProfilePhotoUrl = picture;

            IsLoggedIn = true;

            // Navegar al área autenticada — el TabBar aparece automáticamente
            await Shell.Current.GoToAsync("//Home");
#endif

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
