using LevelUp.Mobile.Core.Abstractions;

namespace LevelUp.Mobile.Features.Splash.ViewModels
{
    public partial class SplashViewModel(ISessionService sessionService) : BaseViewModel
    {
        public async Task InitializeAsync()
        {
            await Task.Delay(1200);

            var hasSession = await sessionService.HasValidSessionAsync();

            if (hasSession)
                await Shell.Current.GoToAsync("//Home");
            else
                await Shell.Current.GoToAsync("//Login");
        }
    }
}
