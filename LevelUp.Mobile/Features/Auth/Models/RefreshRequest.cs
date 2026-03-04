namespace LevelUp.Mobile.Features.Auth.Models
{
    public class RefreshRequest
    {
        public string RefreshToken { get; set; } = default!;
        public string TimeZoneId { get; set; } = default!;

    }
}
