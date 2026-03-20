using LevelUp.Mobile.Core.Enums;
using SQLite;

namespace LevelUp.Mobile.Core.Entities
{
    /// <summary>
    /// Perfil y configuraciones del usuario almacenadas localmente.
    /// Esta entidad se sincroniza en ambas direcciones (pull/push).
    /// </summary>
    public class UserProfile : ILocalEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
        public bool IsDeleted { get; set; }

        // ── Identidad (solo lectura desde el JWT) ────────────────────
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? ProfilePictureUrl { get; set; }

        // ── Configuraciones editables por el usuario ─────────────────

        /// <summary>Peso corporal actual SIEMPRE en libras.</summary>
        public decimal? CurrentBodyWeightInLb { get; set; }

        /// <summary>Idioma preferido de la app.</summary>
        public Language PreferredLanguage { get; set; } = Language.English;

        /// <summary>Tema visual: System / Light / Dark.</summary>
        public AppTheme PreferredTheme { get; set; } = AppTheme.Dark;

        /// <summary>Unidad preferida para mostrar pesos.</summary>
        public WeightUnit PreferredWeightUnit { get; set; } = WeightUnit.Lb;

        /// <summary>Zona horaria IANA o Windows, p.ej. "America/Santo_Domingo".</summary>
        public string? TimeZoneId { get; set; }
    }
}