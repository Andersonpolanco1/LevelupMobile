namespace LevelUp.Mobile.Core.Abstractions
{
    public interface IApiClient
    {
        Task<TResponse> GetAsync<TResponse>(string endpoint);
        Task<TResponse?> GetOptionalAsync<TResponse>(string endpoint);
        Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest data);
    }
}
