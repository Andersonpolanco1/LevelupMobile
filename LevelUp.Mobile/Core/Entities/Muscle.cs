using SQLite;

namespace LevelUp.Mobile.Core.Entities
{
    public class Muscle : ILocalEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
        public bool IsDeleted { get; set; }

        public Guid MuscleGroupId { get; set; }
    }
}
