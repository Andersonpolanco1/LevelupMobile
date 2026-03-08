using LevelUp.Mobile.Core.Enums;

namespace LevelUp.Mobile.Infrastructure.Api.Dtos
{
    public class ExerciseSetSyncDto
    {
        public Guid Id { get; set; }
        public Guid WorkoutExerciseId { get; set; }
        public int? Reps { get; set; }
        public double? WeightInLb { get; set; }
        public int? Duration { get; set; }
        public decimal? Distance { get; set; }
        public decimal? Rpe { get; set; }
        public SetType Type { get; set; }
        public int? RestTimeSeconds { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
