namespace LevelUp.Mobile.Core.Entities
{
    public class WeeklyPlanExercise : LocalEntity
    {
        public Guid WeeklyPlanDayId { get; set; }

        public Guid ExerciseId { get; set; }

        public int Order { get; set; }

        public int? SetsPlanned { get; set; }

        public int? RepsPlanned { get; set; }

        public TimeSpan? DurationPlanned { get; set; }

        public TimeSpan? RestTimePlanned { get; set; }

        public string? Notes { get; set; }
    }
}
