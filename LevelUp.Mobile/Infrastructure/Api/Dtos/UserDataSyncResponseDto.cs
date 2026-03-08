namespace LevelUp.Mobile.Infrastructure.Api.Dtos
{
    // Para sync de datos del usuario

    public class UserDataSyncResponseDto
    {
        public List<WeeklyPlanSyncDto> WeeklyPlans { get; set; } = [];
        public List<WorkoutSyncDto> Workouts { get; set; } = [];
    }
}
