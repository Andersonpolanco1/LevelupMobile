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
            var googleToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6IjI1MDdmNTFhZjJhMTYyNDY3MDc0ODQ2NzRhNDJhZTNjMmI2MjMxOWMiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2FjY291bnRzLmdvb2dsZS5jb20iLCJhenAiOiI0MDc0MDg3MTgxOTIuYXBwcy5nb29nbGV1c2VyY29udGVudC5jb20iLCJhdWQiOiI0MDc0MDg3MTgxOTIuYXBwcy5nb29nbGV1c2VyY29udGVudC5jb20iLCJzdWIiOiIxMTQ3MDIxOTc3NzcyMjE4MDcwNTYiLCJlbWFpbCI6ImFuZGVyc29ucG9sYW5jb2NvbnRyZXJhc0BnbWFpbC5jb20iLCJlbWFpbF92ZXJpZmllZCI6dHJ1ZSwiYXRfaGFzaCI6IjlHb2Z1M2lXWGNJdy1lb05YMkJuakEiLCJuYW1lIjoiQW5kZXJzb24gUG9sYW5jbyIsInBpY3R1cmUiOiJodHRwczovL2xoMy5nb29nbGV1c2VyY29udGVudC5jb20vYS9BQ2c4b2NLY0tsaHoxZUZybkV2VUFQWlFUeDlWaUdRTFlhTENnRlJyUzc2MHlld04yUi1XaE1LbT1zOTYtYyIsImdpdmVuX25hbWUiOiJBbmRlcnNvbiIsImZhbWlseV9uYW1lIjoiUG9sYW5jbyIsImlhdCI6MTc3Mjg1MjMyMSwiZXhwIjoxNzcyODU1OTIxfQ.emwqMqLa53wXLtJaWKPIIauAk1Oln5oM2GHCWvie5-9SX2qf5lTqE9np6sV_aGik1e8WHywZyKOTlMKVc608a0-MdR37UAp5Rv2Ri1MMHWwCz5e69af1_IreI6vOZemncbWEbDujYTT2PIiJ_8R0xTVFX1Bo1JekjAplLkhlS4Z9smYyDlwoYDnZJ-82zS3r3kX-MhXXJVMP114ZOONCyrUTAXSTLBYWMX-_9AKB4qddPkNoAKmsKLSKJTrzW3DWSC9zcYh-8-DrGVY2-iZZezsspFXxgPkt9pRFUgo0EM2Lm0BAgPrlF7sPVQXyF9wzZXESjroY1zd1ecHSz4kj0A";
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
