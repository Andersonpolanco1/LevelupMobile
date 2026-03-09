using SQLite;

namespace LevelUp.Mobile.Core.Entities
{
    public class WeeklyPlan : ILocalEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
        public bool IsDeleted { get; set; }

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

        public Guid UserId { get;  set; }
    }
}
