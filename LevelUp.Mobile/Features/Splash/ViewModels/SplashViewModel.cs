using LevelUp.Mobile.Core.Abstractions;

namespace LevelUp.Mobile.Features.Splash.ViewModels
{
    public partial class SplashViewModel(ISessionService sessionService) : BaseViewModel
    {
        public async Task InitializeAsync()
        {
            var hasSession = await sessionService.HasValidSessionAsync();

            if (hasSession)
                await Shell.Current.GoToAsync("//Home");
            else
                await Shell.Current.GoToAsync("//Login");
        }
    }
}
