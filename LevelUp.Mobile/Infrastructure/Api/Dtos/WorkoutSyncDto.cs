using LevelUp.Mobile.Core.Enums;

namespace LevelUp.Mobile.Infrastructure.Api.Dtos
{
    public class WorkoutSyncDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Notes { get; set; }
        public Guid? WeeklyPlanDayId { get; set; }
        public double? BodyWeightInLbSnapshot { get; set; }
        public WorkoutState WorkoutState { get; set; }
        public DateTime? FinishedAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<WorkoutExerciseSyncDto> WorkoutExercises { get; set; } = [];
    }
}
