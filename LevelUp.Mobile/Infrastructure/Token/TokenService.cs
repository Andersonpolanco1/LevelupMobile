namespace LevelUp.Mobile.Infrastructure.Token
{
    using Microsoft.Maui.Storage;

    public class TokenService : ITokenService
    {
        private const string AccessTokenKey = "access_token";
        private const string RefreshTokenKey = "refresh_token";
        private const string ExpirationKey = "expires_at";

        public async Task<string?> GetAccessTokenAsync()
            => await SecureStorage.GetAsync(AccessTokenKey);

        public async Task<string?> GetRefreshTokenAsync()
            => await SecureStorage.GetAsync(RefreshTokenKey);

        public async Task<DateTime?> GetExpirationAsync()
        {
            var value = await SecureStorage.GetAsync(ExpirationKey);

            if (DateTime.TryParse(value, out var date))
                return date;

            return null;
        }

        public async Task SetTokensAsync(string accessToken, string refreshToken, DateTime expiresAt)
        {
            await SecureStorage.SetAsync(AccessTokenKey, accessToken);
            await SecureStorage.SetAsync(RefreshTokenKey, refreshToken);
            await SecureStorage.SetAsync(ExpirationKey, expiresAt.ToString("O"));
        }

        public Task ClearAsync()
        {
            SecureStorage.Remove(AccessTokenKey);
            SecureStorage.Remove(RefreshTokenKey);
            SecureStorage.Remove(ExpirationKey);
            return Task.CompletedTask;
        }
    }
}
