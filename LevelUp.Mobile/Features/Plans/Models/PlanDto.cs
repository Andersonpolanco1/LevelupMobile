namespace LevelUp.Mobile.Features.Plans.Models
{
    public class PlanDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int DaysCount { get; set; }

        public bool IsActive { get; set; }
    }
}
