using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LevelUp.Mobile.Features.Auth.Services;
using LevelUp.Mobile.Infrastructure.Token;

namespace LevelUp.Mobile.Features.Auth.ViewModels
{

    public partial class LoginViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        private readonly ITokenService _tokenService; 
        public LoginViewModel(AuthService authService, ITokenService tokenService)
        {
            _authService = authService;
            _tokenService = tokenService;
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotLoggedIn))]
        private bool isLoggedIn;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool isBusy;

        [ObservableProperty]
        private string? userName;

        [ObservableProperty]
        private string? userEmail;

        [ObservableProperty]
        private string? profilePhotoUrl;

        // Propiedades calculadas — sin converter en XAML
        public bool IsNotLoggedIn => !IsLoggedIn;
        public bool IsNotBusy => !IsBusy;

        [RelayCommand]
        private async Task LoginWithGoogleAsync()
        {
            IsBusy = true;

            var result = await _authService.LoginWithGoogleAsync();

            if (result.IsSuccess)
            {
                var claims = await _tokenService.GetUserClaimsAsync();

                if(claims.TryGetValue("userName", out var name)) UserName = name;
                if (claims.TryGetValue("email", out var email)) UserEmail = email;
                if (claims.TryGetValue("picture", out var picture)) ProfilePhotoUrl = picture;

                // Si quieres la foto que agregaste en el backend ("picture"):
                //profilePhoto = claims.GetValueOrDefault("picture");
                IsLoggedIn = true;
            }

            IsBusy = false;

        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            await _authService.LogoutAsync();
            IsLoggedIn = false;
            UserName = null;
            UserEmail = null;
            ProfilePhotoUrl = null;
        }
    }
}