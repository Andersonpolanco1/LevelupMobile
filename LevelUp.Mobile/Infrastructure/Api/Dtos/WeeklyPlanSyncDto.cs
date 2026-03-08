namespace LevelUp.Mobile.Infrastructure.Api.Dtos
{
    public class WeeklyPlanSyncDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = "";
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<WeeklyPlanDaySyncDto> Days { get; set; } = [];
    }
}
