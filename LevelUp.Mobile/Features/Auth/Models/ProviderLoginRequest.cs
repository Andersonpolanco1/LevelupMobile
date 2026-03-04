namespace LevelUp.Mobile.Features.Auth.Models
{
    public class ProviderLoginRequest
    {
        public string AccessToken { get; set; } = default!;
        public string TimeZoneId { get; set; } = default!;
    }
}
