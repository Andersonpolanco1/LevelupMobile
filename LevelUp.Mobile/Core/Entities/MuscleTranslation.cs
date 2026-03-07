using LevelUp.Mobile.Core.Enums;

namespace LevelUp.Mobile.Core.Entities
{
    public class MuscleTranslation
    {
        public Guid Id { get; set; }

        public Guid MuscleId { get; set; }

        public Language Language { get; set; }

        public string Name { get; set; } = "";
    }
}
