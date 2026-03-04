namespace LevelUp.Mobile.Features.Auth.Models
{
    public class GoogleLoginRequest
    {
        public string ProviderToken { get; set; } = default!;
        public string TimeZoneId { get; set; } = default!;
    }
}
