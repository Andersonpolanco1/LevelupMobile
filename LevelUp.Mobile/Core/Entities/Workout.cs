using LevelUp.Mobile.Core.Enums;

namespace LevelUp.Mobile.Core.Entities
{
    public class Workout : LocalEntity
    {
        public Guid UserId { get; set; }

        public string? Notes { get; set; }

        public Guid? WeeklyPlanDayId { get; set; }

        public double? BodyWeightInLbSnapshot { get; set; }

        public WorkoutState WorkoutState { get; set; }

        public DateTime? FinishedAt { get; set; }
    }
}
