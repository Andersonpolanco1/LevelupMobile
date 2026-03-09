using SQLite;

namespace LevelUp.Mobile.Core.Entities
{
    public class WorkoutExercise : ILocalEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
        public bool IsDeleted { get; set; }

        public Guid WorkoutId { get; set; }

        public Guid ExerciseId { get; set; }

        public Guid? WeeklyPlanExerciseId { get; set; }

        public int Order { get; set; }

        public string? Notes { get; set; }
    }
}
