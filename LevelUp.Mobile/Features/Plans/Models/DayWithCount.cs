using LevelUp.Mobile.Core.Entities;

namespace LevelUp.Mobile.Features.Plans.Models
{
    public class DayWithCount
    {
        public WeeklyPlanDay Day { get; set; } = null!;
        public int ExerciseCount { get; set; }
        public string DayName { get; set; } = "";
    }
}
