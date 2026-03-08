using LevelUp.Mobile.Core.Entities;
using LevelUp.Mobile.Core.Enums;

namespace LevelUp.Mobile.Core.Abstractions
{
    public interface ISyncQueue
    {
        Task EnqueueAsync<T>(T entity, SyncOperation operation)
            where T : LocalEntity;
    }
}
