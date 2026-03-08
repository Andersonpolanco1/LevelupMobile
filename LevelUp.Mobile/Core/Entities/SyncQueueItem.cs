using LevelUp.Mobile.Core.Enums;

namespace LevelUp.Mobile.Core.Entities
{
    public class SyncQueueItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Nombre de la entidad: "WeeklyPlan", "Workout", "ExerciseSet", etc.
        public string EntityType { get; set; } = "";  // tu índice usa EntityType ✓

        public Guid EntityId { get; set; }

        // Create / Update / Delete
        public SyncOperation Operation { get; set; }

        // JSON del objeto completo al momento de encolar
        public string PayloadJson { get; set; } = "";

        public int RetryCount { get; set; } = 0;

        // Pending / Failed — tu índice usa Status ✓
        public SyncItemStatus Status { get; set; } = SyncItemStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
