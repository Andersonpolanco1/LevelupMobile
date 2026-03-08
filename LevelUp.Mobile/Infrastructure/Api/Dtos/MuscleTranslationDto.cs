using LevelUp.Mobile.Core.Enums;

namespace LevelUp.Mobile.Infrastructure.Api.Dtos
{
    public class MuscleTranslationDto
    {
        public Guid Id { get; set; }
        public Guid MuscleId { get; set; }
        public Language Language { get; set; }
        public string Name { get; set; } = "";
    }
}
