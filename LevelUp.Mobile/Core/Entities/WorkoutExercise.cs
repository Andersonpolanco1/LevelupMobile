namespace LevelUp.Mobile.Core.Entities
{
    public class WorkoutExercise : LocalEntity
    {
        public Guid WorkoutId { get; set; }

        public Guid ExerciseId { get; set; }

        public Guid? WeeklyPlanExerciseId { get; set; }

        public int Order { get; set; }

        public string? Notes { get; set; }
    }
}
