using LevelUp.Mobile.Core.Enums;

namespace LevelUp.Mobile.Infrastructure.Api.Dtos
{
    public class ExerciseMuscleDto
    {
        public Guid MuscleId { get; set; }
        public MuscleRole Role { get; set; }
    }
}
