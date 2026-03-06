namespace LevelUp.Mobile.Features.Plans.Models
{
    public class WeeklyPlanListItemDto
    {
        public Guid Id { get; internal set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string[] DaysOfWeekShortName { get; set; } = [];
        public string? Notes { get; internal set; }

        public string DaysText => DaysOfWeekShortName?.Length > 0
            ? string.Join(" • ", DaysOfWeekShortName)
            : "Sin días";
    }
}
