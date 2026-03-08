using LevelUp.Mobile.Core.Enums;

namespace LevelUp.Mobile.Infrastructure.Api.Dtos
{
    public class ExerciseTranslationDto
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
