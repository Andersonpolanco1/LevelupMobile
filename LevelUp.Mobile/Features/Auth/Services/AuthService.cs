namespace LevelUp.Mobile.Features.Auth.Services
{
    public class AuthService
    {
        // En Android usar la IP especial del host
#if ANDROID
        private const string BackendBaseUrl = "https://levelup.tryasp.net";
#else
private const string BackendBaseUrl = "https://levelup.tryasp.net";
#endif
        private const string CallbackScheme = "levelup";

        public async Task<AuthResult> LoginWithGoogleAsync()
        {
            try
            {
                var authUrl = new Uri($"{BackendBaseUrl}/api/mobileauth/google");
                var callbackUrl = new Uri($"{CallbackScheme}://callback");

                WebAuthenticatorResult result = await WebAuthenticator.Default.AuthenticateAsync(
                    new WebAuthenticatorOptions
                    {
                        Url = authUrl,
                        CallbackUrl = callbackUrl,
                        PrefersEphemeralWebBrowserSession = true
                    });

                result.Properties.TryGetValue("token", out string? accessToken);
                result.Properties.TryGetValue("refresh", out string? refreshToken);
                result.Properties.TryGetValue("expires", out string? expiresAt);

                if (string.IsNullOrEmpty(accessToken))
                    return AuthResult.Failure("No se recibió token");

                await SecureStorage.Default.SetAsync("access_token", accessToken);
                await SecureStorage.Default.SetAsync("refresh_token", refreshToken ?? "");

                result.Properties.TryGetValue("message", out string? errorMessage);
                if (result.Properties.ContainsKey("error") || errorMessage != null)
                {
                    return AuthResult.Failure(errorMessage ?? "Error desconocido");
                }

                return AuthResult.Success(accessToken);

            }
            catch (TaskCanceledException)
            {
                return AuthResult.Failure("El usuario canceló");
            }
            catch (Exception ex)
            {
                return AuthResult.Failure($"Error: {ex.Message}");
            }
        }

        public async Task LogoutAsync()
        {
            SecureStorage.Default.Remove("auth_token");
            SecureStorage.Default.Remove("user_email");
        }

        public async Task<string?> GetTokenAsync()
            => await SecureStorage.Default.GetAsync("auth_token");
    }

    // Modelo simple de resultado
    public record AuthResult(bool IsSuccess, string? Token, string? Error)
    {
        public static AuthResult Success(string token) => new(true, token, null);
        public static AuthResult Failure(string error) => new(false, null, error);
    }

}
