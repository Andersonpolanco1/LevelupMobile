using CommunityToolkit.Maui;
using LevelUp.Mobile.Extensions;
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
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Font Awesome 7 Brands-Regular-400.otf", "FABrands");
                    fonts.AddFont("Font Awesome 7 Free-Regular-400.otf", "FARegular");
                    fonts.AddFont("Font Awesome 7 Free-Solid-900.otf", "FASolid");
                });

            builder.AddAppConfiguration();
            builder.Services.AddHttpClients(builder.Configuration);
            builder.Services.AddAppServices();
            builder.Services.AddPagesAndViewModels();
            builder.Services.AddInfrastructure();
            builder.ConfigurePlatformHandlers();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            try
            {
                return builder.Build();
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