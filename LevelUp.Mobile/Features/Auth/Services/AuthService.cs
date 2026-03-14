using LevelUp.Mobile.Features.Auth.Models;
using LevelUp.Mobile.Infrastructure.Token;
using LevelUp.Mobile.Settings;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace LevelUp.Mobile.Features.Auth.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenService _tokenService;
        private readonly ApiSettings _apiSettings;
        private readonly AuthSettings _authSettings;

        public AuthService(
            HttpClient httpClient,
            ITokenService tokenService,
            IOptions<ApiSettings> apiOptions,
            IOptions<AuthSettings> authOptions)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
            _apiSettings = apiOptions.Value;
            _authSettings = authOptions.Value;
        }

        public async Task<AuthResult> LoginWithGoogleAsync()
        {
            try
            {
                var authUrl = new Uri($"{_apiSettings.BaseUrl}{_authSettings.GoogleAuthPath}");
                var callbackUrl = _authSettings.CallbackUri;

                var result = await WebAuthenticator.Default.AuthenticateAsync(
                    new WebAuthenticatorOptions
                    {
                        Url = authUrl,
                        CallbackUrl = callbackUrl,
                        PrefersEphemeralWebBrowserSession = false
                    });

                // Verificar error primero
                if (result.Properties.TryGetValue("error", out string? error)
                    && !string.IsNullOrEmpty(error))
                    return AuthResult.Failure(error);

                result.Properties.TryGetValue("token", out string? accessToken);
                result.Properties.TryGetValue("refresh", out string? refreshToken);
                result.Properties.TryGetValue("expires", out string? expiresAt);

                if (string.IsNullOrEmpty(accessToken))
                    return AuthResult.Failure("No se recibió token");

                var expiration = DateTime.TryParse(expiresAt, null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsedExpiry)
                    ? parsedExpiry
                    : DateTime.UtcNow.AddHours(1);

                await _tokenService.SetTokensAsync(accessToken, refreshToken ?? "", expiration);

                return AuthResult.Success(accessToken);
            }
            catch (TaskCanceledException)
            {
                return AuthResult.Failure("El usuario canceló");
            }
            catch (Exception ex)
            {
                return AuthResult.Failure(ex.Message);
            }
        }

        /// <summary>
        /// PAra pruebas
        /// </summary>
        /// <returns></returns>
        public async Task<AuthResult> LoginWithGoogleAsync(string googleToken)
        {
            try
            {
                var request = new ApiTokenDto
                {
                    ProviderToken = googleToken,
                    TimeZoneId = TimeZoneInfo.Local.Id
                };

                var response = await _httpClient.PostAsJsonAsync("auth/google", request);

                if (!response.IsSuccessStatusCode)
                    return AuthResult.Failure("Error autenticando");

                var data = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

                if (data == null || string.IsNullOrEmpty(data.AccessToken))
                    return AuthResult.Failure("No se recibió token");

                var expiration = DateTime.TryParse(data.ExpiresAt, out var parsedExpiry)
                    ? parsedExpiry
                    : DateTime.UtcNow.AddHours(1);

                await _tokenService.SetTokensAsync(
                    data.AccessToken,
                    data.RefreshToken ?? "",
                    expiration);

                return AuthResult.Success(data.AccessToken);
            }
            catch (TaskCanceledException)
            {
                return AuthResult.Failure("El usuario canceló");
            }
            catch (Exception ex)
            {
                return AuthResult.Failure(ex.Message);
            }
        }

        public async Task<bool> TryRefreshAsync()
        {
            var refreshToken = await _tokenService.GetRefreshTokenAsync();
            if (string.IsNullOrEmpty(refreshToken)) return false;

            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "auth/refresh",
                    new
                    {
                        tokenRefresh = refreshToken,
                        timeZoneId = TimeZoneInfo.Local.Id
                    });

                if (!response.IsSuccessStatusCode) return false;

                var result = await response.Content.ReadFromJsonAsync<RefreshResponse>();
                if (result is null || string.IsNullOrEmpty(result.AccessToken)) return false;

                await _tokenService.SetTokensAsync(
                    result.AccessToken,
                    result.RefreshToken,
                    result.ExpiresAt);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            await _tokenService.ClearAsync();
        }
    }

    public record AuthResult(bool IsSuccess, string? Token, string? Error)
    {
        public static AuthResult Success(string token) => new(true, token, null);
        public static AuthResult Failure(string error) => new(false, null, error);
    }


    /// <summary>
    /// Dtos de pruebas
    /// </summary>
    public class ApiTokenDto
    {
        public string ProviderToken { get; set; } = default!;
        public string TimeZoneId { get; set; } = default!;
    }

    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = default!;
        public string? RefreshToken { get; set; }
        public string? ExpiresAt { get; set; }
    }
}