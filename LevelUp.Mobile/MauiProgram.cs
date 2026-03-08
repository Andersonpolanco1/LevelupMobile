using CommunityToolkit.Maui;
using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Features.Auth.Pages;
using LevelUp.Mobile.Features.Auth.Services;
using LevelUp.Mobile.Features.Auth.ViewModels;
using LevelUp.Mobile.Features.Exercises.Pages;
using LevelUp.Mobile.Features.Home.Pages;
using LevelUp.Mobile.Features.Home.ViewModels;
using LevelUp.Mobile.Features.Plans.Pages;
using LevelUp.Mobile.Features.Plans.ViewModels;
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
using Microsoft.Maui.Handlers;
using LevelUp.Mobile.Infrastructure.Repositories;
using LevelUp.Mobile.Infrastructure.Sync;
using LevelUp.Mobile.Infrastructure.LocalDb;

#if ANDROID
using Android.Content.Res;
#endif


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

            builder.Services.AddTransient<PlansViewModel>();
            builder.Services.AddTransient<PlansPage>();


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
            builder.Services.AddTransient<CreatePlanViewModel>();

            // Profile
            builder.Services.AddTransient<ProfilePage>();

            // Base de datos — singleton porque la conexión debe ser compartida
            builder.Services.AddSingleton<LocalDatabase>();


            // Repositorios
            builder.Services.AddSingleton<ExerciseRepository>();
            builder.Services.AddSingleton<WeeklyPlanRepository>();
            builder.Services.AddSingleton<WorkoutRepository>();

            // Sync
            builder.Services.AddSingleton<ISyncQueue, SyncQueue>();
            builder.Services.AddSingleton<ISyncService, SyncService>();

            // Servicios de negocio
            builder.Services.AddSingleton<WeeklyPlanService>();
            builder.Services.AddSingleton<HomeService>();

            // ── Shell y App ───────────────────────────────────────
            builder.Services.AddSingleton<AppShell>();

            // Trigger automático de sync por conectividad
            builder.Services.AddSingleton<ConnectivitySyncTrigger>();

#if DEBUG
            builder.Logging.AddDebug();
#endif


            //elimina linea azul por defecto de android en los inputs
            builder.ConfigureMauiHandlers(handlers =>
            {
#if ANDROID
                EntryHandler.Mapper.AppendToMapping("BrandUnderline", (handler, view) =>
                {
                    var editText = handler.PlatformView;
                    var states = new[]
                    {
            new[] { Android.Resource.Attribute.StateFocused },
            new[] { -Android.Resource.Attribute.StateFocused }
        };
                    var colors = new int[]
                    {
            Android.Graphics.Color.ParseColor("#FF7A00").ToArgb(),
            Android.Graphics.Color.ParseColor("#52525B").ToArgb()
                    };
                    editText.BackgroundTintList = new Android.Content.Res.ColorStateList(states, colors);
                });
                EditorHandler.Mapper.AppendToMapping("BrandUnderline", (handler, view) =>
                {
                    var editText = handler.PlatformView;
                    var states = new[]
                    {
            new[] { Android.Resource.Attribute.StateFocused },
            new[] { -Android.Resource.Attribute.StateFocused }
        };
                    var colors = new int[]
                    {
            Android.Graphics.Color.ParseColor("#FF7A00").ToArgb(),
            Android.Graphics.Color.ParseColor("#52525B").ToArgb()
                    };
                    editText.BackgroundTintList = new Android.Content.Res.ColorStateList(states, colors);
                });
#endif
            });

            try
            {
                var app = builder.Build();
                return app;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DI BUILD FAILED: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"INNER: {ex.InnerException?.Message}");
                System.Diagnostics.Debug.WriteLine($"STACK: {ex.StackTrace}");
                throw;
            }
        }
    }
}