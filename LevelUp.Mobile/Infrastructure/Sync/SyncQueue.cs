using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Core.Entities;
using LevelUp.Mobile.Core.Enums;
using LevelUp.Mobile.Infrastructure.LocalDb;
using System.Text.Json;

namespace LevelUp.Mobile.Infrastructure.Sync;

public class SyncQueue(LocalDatabase db) : ISyncQueue
{
    public async Task EnqueueAsync<T>(T entity, SyncOperation operation)
        where T : LocalEntity
    {
        var item = new SyncQueueItem
        {
            Id = Guid.NewGuid(),
            EntityType = typeof(T).Name,
            EntityId = entity.Id,
            Operation = operation,
            PayloadJson = JsonSerializer.Serialize(entity),
            Status = SyncItemStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await db.Connection.InsertAsync(item);
    }
}