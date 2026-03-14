namespace LevelUp.Mobile.Core.Entities
{
    public interface ILocalEntity
    {
        Guid Id { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime? UpdatedAt { get; set; }
        bool IsSynced { get; set; }
        bool IsDeleted { get; set; }
    }
}
