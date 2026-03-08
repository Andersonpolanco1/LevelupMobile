namespace LevelUp.Mobile.Infrastructure.Api.Dtos
{
    public class MuscleDto
    {
        public Guid Id { get; set; }
        public Guid MuscleGroupId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<MuscleTranslationDto> Translations { get; set; } = [];
    }
}
