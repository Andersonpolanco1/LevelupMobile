using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Features.Auth.Pages;
using LevelUp.Mobile.Features.Auth.Services;
using LevelUp.Mobile.Features.Auth.ViewModels;
using LevelUp.Mobile.Features.Home;
using LevelUp.Mobile.Features.Home.Pages;
using LevelUp.Mobile.Features.Splash.Pages;
using LevelUp.Mobile.Features.Splash.ViewModels;
using LevelUp.Mobile.Infrastructure.Api;
using LevelUp.Mobile.Infrastructure.Session;
using LevelUp.Mobile.Infrastructure.Token;
using Microsoft.Extensions.Logging;
namespace LevelUp.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // ── HTTP ──────────────────────────────────────────────
            builder.Services.AddTransient<AuthHeaderHandler>();
            builder.Services.AddTransient<RefreshHandler>();
            builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
            {
                client.BaseAddress = new Uri("localhost:7145/api");
            })
            .AddHttpMessageHandler<AuthHeaderHandler>()
            .AddHttpMessageHandler<RefreshHandler>();

            // ── Servicios ─────────────────────────────────────────
            builder.Services.AddSingleton<ITokenService, TokenService>();
            builder.Services.AddSingleton<ISessionService, SessionService>();
            builder.Services.AddTransient<AuthService>();

            // ── Páginas y ViewModels ──────────────────────────────
            builder.Services.AddTransient<SplashPage>();
            builder.Services.AddTransient<SplashViewModel>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<HomeViewModel>();

            // ── Shell y App ───────────────────────────────────────
            builder.Services.AddSingleton<AppShell>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}