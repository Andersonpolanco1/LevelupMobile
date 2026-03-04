using LevelUp.Mobile.Core.Extensions;
using LevelUp.Mobile.Features.Auth.Services;
using LevelUp.Mobile.Infrastructure.Token;
using System.Net;
using System.Net.Http.Headers;

namespace LevelUp.Mobile.Infrastructure.Api
{
    public class RefreshHandler : DelegatingHandler
    {
        private readonly AuthService _authService;
        private readonly ITokenService _tokenService;

        private static readonly SemaphoreSlim _refreshLock = new(1, 1);

        public RefreshHandler(
            AuthService authService,
            ITokenService tokenService)
        {
            _authService = authService;
            _tokenService = tokenService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
    HttpRequestMessage request,
    CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode != HttpStatusCode.Unauthorized)
                return response;

            await _refreshLock.WaitAsync(cancellationToken);

            try
            {
                // Verificar si otro request ya refrescó
                var expiration = await _tokenService.GetExpirationAsync();

                if (expiration.HasValue && expiration > DateTime.UtcNow)
                {
                    var newToken = await _tokenService.GetAccessTokenAsync();
                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", newToken);

                    return await base.SendAsync(request, cancellationToken);
                }

                var refreshed = await _authService.TryRefreshAsync();

                if (!refreshed)
                {
                    await _tokenService.ClearAsync();
                    await Shell.Current.GoToAsync("//Login");
                    return response;
                }

                var updatedToken = await _tokenService.GetAccessTokenAsync();

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", updatedToken);

                return await base.SendAsync(request, cancellationToken);
            }
            finally
            {
                _refreshLock.Release();
            }
        }
    }
}
