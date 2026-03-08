namespace LevelUp.Mobile.Infrastructure.Api.Dtos
{
    public class WeeklyPlanExerciseSyncDto
    {
        public Guid Id { get; set; }
        public Guid WeeklyPlanDayId { get; set; }
        public Guid ExerciseId { get; set; }
        public int Order { get; set; }
        public int? SetsPlanned { get; set; }
        public int? RepsPlanned { get; set; }
        public TimeSpan? DurationPlanned { get; set; }
        public TimeSpan? RestTimePlanned { get; set; }
        public string? Notes { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
