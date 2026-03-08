using LevelUp.Mobile.Core.Abstractions;
using System.Net.Http.Json;
using System.Text.Json;

namespace LevelUp.Mobile.Infrastructure.Api
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TResponse> GetAsync<TResponse>(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ApiError>(_jsonOptions);
                throw new ApiException(
                    error?.Message ?? "Error inesperado",
                    error?.Code,
                    error?.FieldErrors);
            }

            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        public async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, data);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ApiError>(_jsonOptions);
                throw new ApiException(
                    error?.Message ?? "Error inesperado",
                    error?.Code,
                    error?.FieldErrors);
            }

            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        public async Task<TResponse?> GetOptionalAsync<TResponse>(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ApiError>(_jsonOptions);
                throw new ApiException(
                    error?.Message ?? "Error inesperado",
                    error?.Code,
                    error?.FieldErrors);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                return default;

            return await response.Content.ReadFromJsonAsync<TResponse>();
        }
    }
}
