using LevelUp.Mobile.Core.Enums;

namespace LevelUp.Mobile.Core.Entities
{
    public class ExerciseTranslation
    {
        public Guid Id { get; set; }

        public Guid ExerciseId { get; set; }

        public Language Language { get; set; }

        public string Name { get; set; } = "";

        public string Instructions { get; set; } = "";

        public string? Tips { get; set; }

        public string? CommonMistakes { get; set; }
    }
}
