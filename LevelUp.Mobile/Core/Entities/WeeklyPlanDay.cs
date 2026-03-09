using SQLite;

namespace LevelUp.Mobile.Core.Entities
{
    public class WeeklyPlanDay : ILocalEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
        public bool IsDeleted { get; set; }

        public Guid WeeklyPlanId { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public string? Notes { get; set; }
    }
}
