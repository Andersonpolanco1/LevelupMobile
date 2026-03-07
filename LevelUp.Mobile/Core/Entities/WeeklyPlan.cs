namespace LevelUp.Mobile.Core.Entities
{
    public class WeeklyPlan : LocalEntity
    {
        public Guid UserId { get; set; }

        public string Name { get; set; } = "";

        public string? Notes { get; set; }

        public bool IsActive { get; set; }
    }
}
