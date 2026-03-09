using LevelUp.Mobile.Settings;
using Microsoft.Extensions.Configuration;

namespace LevelUp.Mobile.Extensions
{
    public static class ConfigurationExtensions
    {
        public static MauiAppBuilder AddAppConfiguration(this MauiAppBuilder builder)
        {
            var assembly = typeof(MauiProgram).Assembly;
            using var stream = assembly.GetManifestResourceStream("LevelUp.Mobile.appsettings.json")
                ?? throw new FileNotFoundException("No se pudo encontrar el archivo appsettings.json como EmbeddedResource");

            var config = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();

            builder.Configuration.AddConfiguration(config);

            builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection(ApiSettings.SettingPath));
            builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection(AuthSettings.SettingPath));

            return builder;
        }
    }
}