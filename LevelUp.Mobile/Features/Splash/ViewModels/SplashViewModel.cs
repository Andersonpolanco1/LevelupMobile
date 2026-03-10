using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Features.Auth.Services;
using LevelUp.Mobile.Infrastructure.Token;

namespace LevelUp.Mobile.Features.Splash.ViewModels
{
    public partial class SplashViewModel(ISessionService sessionService, ITokenService tokenService, AuthService authService) : BaseViewModel
    {
        public async Task InitializeAsync()
        {
            if (await sessionService.TryRestoreAsync())
            {
                await Shell.Current.GoToAsync("//Home");
                return;
            }

            var refreshToken = await tokenService.GetRefreshTokenAsync();
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var refreshed = await authService.TryRefreshAsync();
                if (refreshed)
                {
                    await Shell.Current.GoToAsync("//Home");
                    return;
                }
            }

            await Shell.Current.GoToAsync("//Login");
        }
    }
}
