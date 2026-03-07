namespace LevelUp.Mobile.Core.Entities
{
    public abstract class LocalEntity
    {
        public Guid Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool IsSynced { get; set; }

        public bool IsDeleted { get; set; }
    }
}
