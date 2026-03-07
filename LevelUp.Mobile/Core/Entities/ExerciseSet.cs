using LevelUp.Mobile.Core.Enums;

namespace LevelUp.Mobile.Core.Entities
{
    public class ExerciseSet : LocalEntity
    {
        public Guid WorkoutExerciseId { get; set; }

        public int? Reps { get; set; }

        public double? WeightInLb { get; set; }

        public TimeSpan? Duration { get; set; }

        public decimal? Distance { get; set; }

        public decimal? Rpe { get; set; }

        public SetType Type { get; set; }

        public TimeSpan? RestTimeSeconds { get; set; }
    }
}
