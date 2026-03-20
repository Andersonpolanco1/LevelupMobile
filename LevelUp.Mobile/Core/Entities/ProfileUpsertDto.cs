using LevelUp.Mobile.Core.Enums;

namespace LevelUp.Mobile.Core.Entities
{
    /// <summary>DTO enviado al servidor al hacer push de cambios del perfil.</summary>
    public class ProfileUpsertDto
    {
        public decimal? CurrentBodyWeightInLb { get; set; }
        public Language PreferredLanguage { get; set; }
        public AppTheme PreferredTheme { get; set; }
        public WeightUnit PreferredWeightUnit { get; set; }
        public string? TimeZoneId { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
