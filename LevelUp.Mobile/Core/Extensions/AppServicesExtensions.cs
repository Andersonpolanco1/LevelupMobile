using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Features.Auth.Services;
using LevelUp.Mobile.Infrastructure.Session;
using LevelUp.Mobile.Infrastructure.Sync;
using LevelUp.Mobile.Infrastructure.Token;
using LevelUp.Mobile.Services;

namespace LevelUp.Mobile.Extensions
{
    public static class AppServicesExtensions
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            services.AddSingleton<ITokenService, TokenService>();
            services.AddSingleton<ISessionService, SessionService>();

            services.AddSingleton<WeeklyPlanService>();
            services.AddSingleton<HomeService>();

            services.AddSingleton<AppShell>();
            services.AddSingleton<ConnectivitySyncTrigger>();

            return services;
        }
    }
}