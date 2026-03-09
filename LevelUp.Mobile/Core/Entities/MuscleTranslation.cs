using LevelUp.Mobile.Core.Enums;
using SQLite;

namespace LevelUp.Mobile.Core.Entities
{
    public class MuscleTranslation : ILocalEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
        public bool IsDeleted { get; set; }

        public Guid MuscleId { get; set; }

        public Language Language { get; set; }

        public string Name { get; set; } = "";
    }
}
