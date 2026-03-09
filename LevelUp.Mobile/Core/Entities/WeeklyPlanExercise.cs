using SQLite;

namespace LevelUp.Mobile.Core.Entities
{
    public class WeeklyPlanExercise : ILocalEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
        public bool IsDeleted { get; set; }

        public Guid WeeklyPlanDayId { get; set; }

        public Guid ExerciseId { get; set; }

        public int Order { get; set; }

        public int? SetsPlanned { get; set; }

        public int? RepsPlanned { get; set; }

        public TimeSpan? DurationPlanned { get; set; }

        public TimeSpan? RestTimePlanned { get; set; }

        public string? Notes { get; set; }
    }
}
