namespace LevelUp.Mobile.Infrastructure.Token
{
    public interface ITokenService
    {
        Task<string?> GetAccessTokenAsync();
        Task<string?> GetRefreshTokenAsync();
        Task<DateTime?> GetExpirationAsync();

        Task SetTokensAsync(string accessToken, string refreshToken, DateTime expiresAt);
        Task ClearAsync();

        Task<IDictionary<string, string>> GetUserClaimsAsync();
        Task<IDictionary<string, string>> GetUserClaimsAsync(string token);
    }
}
