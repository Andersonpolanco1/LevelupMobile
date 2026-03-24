using LevelUp.Mobile.Core.Enums;

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
        public string TypeBadgeColor { get; set; } = "#FF6600";
        public ExerciseType Type { get; set; }
        public string Instructions { get; set; } = "";
        public string? Tips { get; set; }
        public bool HasTips => !string.IsNullOrWhiteSpace(Tips);
        public string? CommonMistakes { get; set; }
        public bool HasCommonMistakes => !string.IsNullOrWhiteSpace(CommonMistakes);
        public bool IncludesBodyWeight { get; set; }
        public double BodyWeightFactor { get; set; }

        public bool ShowBodyweightBadge =>
            IncludesBodyWeight && Type != ExerciseType.BodyweightStrength;

        public List<string> RequiredFields { get; set; } = [];
        public bool HasRequiredFields => RequiredFields.Count > 0;
        public List<string> MetricsAvailable { get; set; } = [];
        public bool HasMetrics => MetricsAvailable.Count > 0;
        public List<MuscleRoleGroup> MuscleRoles { get; set; } = [];
        public bool HasMuscles => MuscleRoles.Count > 0;
    }

}
