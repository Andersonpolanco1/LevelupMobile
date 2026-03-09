namespace LevelUp.Mobile.Features.Home.Models
{
    public class TodayPlanDto
    {
        public Guid PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public string DayName { get; set; } = string.Empty;
        public DayOfWeek DayOfWeek { get; set; }
        public string? Notes { get; set; }
        public List<TodayExerciseDto> Exercises { get; set; } = [];
    }

    public class TodayExerciseDto
    {
        public Guid ExerciseId { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int Order { get; set; }
        public int? SetsPlanned { get; set; }
        public int? RepsPlanned { get; set; }
        public TimeSpan? DurationPlanned { get; set; }
        public TimeSpan? RestTimePlanned { get; set; }
        public string? Notes { get; set; }
    }
}