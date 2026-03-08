namespace LevelUp.Mobile.Infrastructure.Api.Dtos
{
    public class MuscleGroupDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<MuscleGroupTranslationDto> Translations { get; set; } = [];
        public List<MuscleDto> Muscles { get; set; } = [];
    }
}
