using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Features.Auth.Services;
using LevelUp.Mobile.Infrastructure.Api;
using LevelUp.Mobile.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace LevelUp.Mobile.Extensions
{
    public static class HttpExtensions
    {
        public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration _)
        {
            services.AddTransient<AuthHeaderHandler>();
            services.AddTransient<RefreshHandler>();

            services.AddHttpClient<IApiClient, ApiClient>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
                client.BaseAddress = new Uri(settings.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
            })
            .AddHttpMessageHandler<AuthHeaderHandler>()
            .AddHttpMessageHandler<RefreshHandler>();

            // Cliente sin handlers para evitar loop infinito en el refresh
            services.AddHttpClient<AuthService>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
                client.BaseAddress = new Uri(settings.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
            });

            return services;
        }
    }
}