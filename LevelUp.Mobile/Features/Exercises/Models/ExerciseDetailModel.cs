namespace LevelUp.Mobile.Features.Exercises.Models
{
    /// <summary>
    /// Modelo compuesto que lleva todos los datos de la pantalla de detalle.
    /// </summary>
    public class ExerciseDetailModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string? ImageUrl { get; set; }
        public bool HasImage { get; set; }
        public string TypeLabel { get; set; } = "";
        public string TypeIcon { get; set; } = "";
        /// <summary>Color hex semántico por tipo (naranja, azul, rojo, etc.).</summary>
        public string TypeBadgeColor { get; set; } = "#FF6600";
        public string Instructions { get; set; } = "";
        public string? Tips { get; set; }
        public bool HasTips => !string.IsNullOrWhiteSpace(Tips);
        public string? CommonMistakes { get; set; }
        public bool HasCommonMistakes => !string.IsNullOrWhiteSpace(CommonMistakes);
        public bool IncludesBodyWeight { get; set; }
        public double BodyWeightFactor { get; set; }
        public bool HasBodyweightFactor => IncludesBodyWeight && BodyWeightFactor > 0;
        /// <summary>Campos requeridos al registrar un set (ej: "Weight", "Reps").</summary>
        public List<string> RequiredFields { get; set; } = [];
        public bool HasRequiredFields => RequiredFields.Count > 0;
        /// <summary>Métricas de progreso disponibles para este tipo.</summary>
        public List<string> MetricsAvailable { get; set; } = [];
        public bool HasMetrics => MetricsAvailable.Count > 0;
        public List<MuscleRoleGroup> MuscleRoles { get; set; } = [];
        public bool HasMuscles => MuscleRoles.Count > 0;
    }
}
