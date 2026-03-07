using LevelUp.Mobile.Core.Enums;

namespace LevelUp.Mobile.Core.Entities
{
    public class MuscleGroupTranslation
    {
        public Guid Id { get; set; }

        public Guid MuscleGroupId { get; set; }

        public Language Language { get; set; }

        public string Name { get; set; } = "";
    }
}
