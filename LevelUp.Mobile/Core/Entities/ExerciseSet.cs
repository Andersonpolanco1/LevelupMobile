using LevelUp.Mobile.Core.Enums;
using SQLite;

namespace LevelUp.Mobile.Core.Entities
{
    public class ExerciseSet : ILocalEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
        public bool IsDeleted { get; set; }

        public Guid WorkoutExerciseId { get; set; }

        public int? Reps { get; set; }

        public double? WeightInLb { get; set; }

        public int? DurationSeconds { get; set; }

        public decimal? Distance { get; set; }

        public decimal? Rpe { get; set; }

        public SetType Type { get; set; }

        public int? RestTimeSeconds { get; set; }
    }
}
