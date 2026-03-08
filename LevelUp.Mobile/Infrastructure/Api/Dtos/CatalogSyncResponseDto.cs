namespace LevelUp.Mobile.Infrastructure.Api.Dtos
{
    public class CatalogSyncResponseDto
    {
        public List<ExerciseDto> Exercises { get; set; } = [];
        public List<MuscleGroupDto> MuscleGroups { get; set; } = [];
    }
}
