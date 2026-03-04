using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LevelUp.Mobile.Features.Auth.Services;

namespace LevelUp.Mobile.Features.Auth.ViewModels
{

    public partial class LoginViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
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
                UserName = await SecureStorage.Default.GetAsync("user_name");
                UserEmail = await SecureStorage.Default.GetAsync("user_email");
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
        }
    }
}