using LevelUp.Mobile.Core.Enums;

namespace LevelUp.Mobile.Core.Entities
{
    /// <summary>DTO recibido del servidor al hacer pull del perfil.</summary>
    public class ProfileSyncDto
    {
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? ProfilePictureUrl { get; set; }

        public decimal? CurrentBodyWeightInLb { get; set; }
        public Language PreferredLanguage { get; set; }
        public ThemeMode PreferredTheme { get; set; }
        public WeightUnit PreferredWeightUnit { get; set; }
        public string? TimeZoneId { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
