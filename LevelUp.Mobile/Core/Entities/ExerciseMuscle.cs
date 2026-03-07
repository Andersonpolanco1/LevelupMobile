using LevelUp.Mobile.Core.Enums;

namespace LevelUp.Mobile.Core.Entities
{
    public class ExerciseMuscle
    {
        public Guid ExerciseId { get; set; }

        public Guid MuscleId { get; set; }

        public MuscleRole Role { get; set; }
    }
}
