using LevelUp.Mobile.Core.Entities;
using LevelUp.Mobile.Services;

namespace LevelUp.Mobile.Features.Plans.Models
{
    public class DayWithCount
    {
        public WeeklyPlanDay Day { get; set; } = null!;
        public int ExerciseCount { get; set; }
        public string DayName { get; set; } = "";
        // Models/DayWithCount.cs
        public string ExercisesText =>
            string.Format(LocalizationService.Instance["ExercisesCount"], ExerciseCount);
    }
}
