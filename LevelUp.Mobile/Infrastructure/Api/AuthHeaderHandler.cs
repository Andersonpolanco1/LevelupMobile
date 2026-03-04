using LevelUp.Mobile.Infrastructure.Token;
using System.Net.Http.Headers;

namespace LevelUp.Mobile.Infrastructure.Api
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly ITokenService _tokenService;

        public AuthHeaderHandler(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = await _tokenService.GetAccessTokenAsync();

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
