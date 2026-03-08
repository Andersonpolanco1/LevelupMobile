namespace LevelUp.Mobile.Infrastructure.Api.Dtos
{
    public class WeeklyPlanDaySyncDto
    {
        public Guid Id { get; set; }
        public Guid WeeklyPlanId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public string? Notes { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<WeeklyPlanExerciseSyncDto> Exercises { get; set; } = [];
    }
}
