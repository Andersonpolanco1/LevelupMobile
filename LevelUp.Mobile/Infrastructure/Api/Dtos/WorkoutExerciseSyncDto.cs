namespace LevelUp.Mobile.Infrastructure.Api.Dtos
{
    public class WorkoutExerciseSyncDto
    {
        public Guid Id { get; set; }
        public Guid WorkoutId { get; set; }
        public Guid ExerciseId { get; set; }
        public Guid? WeeklyPlanExerciseId { get; set; }
        public int Order { get; set; }
        public string? Notes { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<ExerciseSetSyncDto> Sets { get; set; } = [];
    }
}
