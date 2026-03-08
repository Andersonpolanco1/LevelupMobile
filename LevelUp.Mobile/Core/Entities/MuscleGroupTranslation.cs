using LevelUp.Mobile.Core.Enums;

namespace LevelUp.Mobile.Core.Entities
{
    public class MuscleGroupTranslation : LocalEntity
    {
        public Guid MuscleGroupId { get; set; }

        public Language Language { get; set; }

        public string Name { get; set; } = "";
    }
}
