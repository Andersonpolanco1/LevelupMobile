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
using LevelUp.Mobile.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;


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

            var assembly = typeof(MauiProgram).Assembly;
            using var stream = assembly.GetManifestResourceStream("LevelUp.Mobile.appsettings.json");

            if (stream != null)
            {
                var config = new ConfigurationBuilder()
                    .AddJsonStream(stream)
                    .Build();

                builder.Configuration.AddConfiguration(config);
            }
            else
            {
                throw new FileNotFoundException("No se pudo encontrar el archivo appsettings.json como EmbeddedResource");
            }

            builder.Services.Configure<ApiSettings>(
                builder.Configuration.GetSection(ApiSettings.SettingPath));
            builder.Services.Configure<AuthSettings>(
                builder.Configuration.GetSection(AuthSettings.SettingPath));

            // ── HTTP ──────────────────────────────────────────────
            builder.Services.AddTransient<AuthHeaderHandler>();
            builder.Services.AddTransient<RefreshHandler>();

            builder.Services.AddHttpClient<IApiClient, ApiClient>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
                client.BaseAddress = new Uri(settings.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds); 
            })
            .AddHttpMessageHandler<AuthHeaderHandler>()
            .AddHttpMessageHandler<RefreshHandler>();

            // para evitar loop infinito en el refresh
            builder.Services.AddHttpClient<AuthService>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
                client.BaseAddress = new Uri(settings.BaseUrl); 
                client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
            });

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