using SQLite;

namespace LevelUp.Mobile.Core.Entities
{
    public class WeeklyPlan : LocalEntity
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = "";
        public string? Notes { get; set; }
        public bool IsActive { get; set; }

        // SQLite guarda un string separado por comas
        public string DaysOfWeekShortNameRaw { get; set; } = "";

        [Ignore]
        public string[] DaysOfWeekShortName
        {
            get => string.IsNullOrEmpty(DaysOfWeekShortNameRaw)
                ? []
                : DaysOfWeekShortNameRaw.Split(',');
            set => DaysOfWeekShortNameRaw = value is { Length: > 0 }
                ? string.Join(",", value)
                : "";
        }

        [Ignore]
        public string DaysText => DaysOfWeekShortName.Length > 0
            ? string.Join(" • ", DaysOfWeekShortName)
            : "Sin días";
    }
}
