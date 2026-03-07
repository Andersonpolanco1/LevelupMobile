using LevelUp.Mobile.Core.Enums;

namespace LevelUp.Mobile.Core.Entities
{
    public class SyncQueueItem
    {
        public Guid Id { get; set; }

        public string EntityName { get; set; } = "";

        public Guid EntityId { get; set; }

        public SyncOperation Operation { get; set; }

        public string PayloadJson { get; set; } = "";

        public int RetryCount { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
