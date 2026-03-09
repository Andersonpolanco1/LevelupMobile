using LevelUp.Mobile.Core.Enums;
using SQLite;

namespace LevelUp.Mobile.Core.Entities
{
    public class Workout : ILocalEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
        public bool IsDeleted { get; set; }

        public Guid UserId { get; set; }

        public string? Notes { get; set; }

        public Guid? WeeklyPlanDayId { get; set; }

        public double? BodyWeightInLbSnapshot { get; set; }

        public WorkoutState WorkoutState { get; set; }

        public DateTime? FinishedAt { get; set; }
    }
}
