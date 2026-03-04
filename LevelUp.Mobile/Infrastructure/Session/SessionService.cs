using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Infrastructure.Token;

namespace LevelUp.Mobile.Infrastructure.Session
{
    public class SessionService : ISessionService
    {
        private readonly ITokenService _tokenService;

        public SessionService(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public async Task<bool> HasValidSessionAsync()
        {
            var accessToken = await _tokenService.GetAccessTokenAsync();
            var expiration = await _tokenService.GetExpirationAsync();

            if (string.IsNullOrWhiteSpace(accessToken) || expiration == null)
                return false;

            int margenMinutes = 1;
            if (expiration <= DateTime.UtcNow.AddMinutes(margenMinutes))
                return false;

            return true;
        }
    }
}
