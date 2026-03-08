using LevelUp.Mobile.Core.Enums;

namespace LevelUp.Mobile.Infrastructure.Api.Dtos
{
    public class ExerciseDto
    {
        public Guid Id { get; set; }
        public bool IncludesBodyWeight { get; set; }
        public double BodyWeightFactor { get; set; }
        public string? ImageUrl { get; set; }
        public ExerciseType Type { get; set; }
        public Guid? CreatorId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<ExerciseTranslationDto> Translations { get; set; } = [];
        public List<ExerciseMuscleDto> Muscles { get; set; } = [];
    }
}
