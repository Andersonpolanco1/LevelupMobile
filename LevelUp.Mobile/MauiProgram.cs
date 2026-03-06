using CommunityToolkit.Maui;
using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Features.Auth.Pages;
using LevelUp.Mobile.Features.Auth.Services;
using LevelUp.Mobile.Features.Auth.ViewModels;
using LevelUp.Mobile.Features.Exercises.Pages;
using LevelUp.Mobile.Features.Home.Pages;
using LevelUp.Mobile.Features.Home.ViewModels;
using LevelUp.Mobile.Features.Plans.Pages;
using LevelUp.Mobile.Features.Profile.Pages;
using LevelUp.Mobile.Features.Splash.Pages;
using LevelUp.Mobile.Features.Splash.ViewModels;
using LevelUp.Mobile.Features.Workouts.Pages;
using LevelUp.Mobile.Infrastructure.Api;
using LevelUp.Mobile.Infrastructure.Session;
using LevelUp.Mobile.Infrastructure.Token;
using LevelUp.Mobile.Services;
using LevelUp.Mobile.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace LevelUp.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Font Awesome 7 Brands-Regular-400.otf", "FABrands");
                    fonts.AddFont("Font Awesome 7 Free-Regular-400.otf", "FARegular");
                    fonts.AddFont("Font Awesome 7 Free-Solid-900.otf", "FASolid");
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

            // Exercises
            builder.Services.AddTransient<ExercisesPage>();
            builder.Services.AddTransient<ExerciseDetailPage>();

            // Workouts
            builder.Services.AddTransient<WorkoutPage>();
            builder.Services.AddTransient<ActiveWorkoutPage>();
            builder.Services.AddTransient<WorkoutSummaryPage>();

            // Plans
            builder.Services.AddTransient<PlansPage>();
            builder.Services.AddTransient<PlanDetailPage>();
            builder.Services.AddTransient<CreatePlanPage>();

            // Profile
            builder.Services.AddTransient<ProfilePage>();

            //services
            builder.Services.AddTransient<HomeService>();

            // ── Shell y App ───────────────────────────────────────
            builder.Services.AddSingleton<AppShell>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}