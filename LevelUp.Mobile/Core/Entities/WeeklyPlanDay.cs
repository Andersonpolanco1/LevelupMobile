namespace LevelUp.Mobile.Core.Entities
{
    public class WeeklyPlanDay : LocalEntity
    {
        public Guid WeeklyPlanId { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public string? Notes { get; set; }
    }
}
