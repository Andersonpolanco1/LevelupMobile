using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Core.Entities;
using LevelUp.Mobile.Core.Enums;
using LevelUp.Mobile.Infrastructure.Repositories;
using LevelUp.Mobile.Infrastructure.Sync;

namespace LevelUp.Mobile.Services;

public class WeeklyPlanService(
    WeeklyPlanRepository repo, ISyncQueue queue, ISyncService sync)
{
    public Task<List<WeeklyPlan>> GetPlansAsync()
        => repo.GetAllAsync();

    public async Task<WeeklyPlan> CreateAsync(Guid userId, string name, string? notes)
    {
        var plan = new WeeklyPlan
        {
            UserId = userId,
            Name = name,
            Notes = notes
        };
        await repo.InsertAsync(plan); // IsSynced = false lo pone InsertAsync
        await queue.EnqueueAsync(plan, SyncOperation.Create);
        _ = Task.Run(() => sync.ProcessSyncQueueAsync());
        return plan;
    }

    public async Task ActivatePlanAsync(Guid planId, Guid userId)
    {
        var all = await repo.GetAllAsync();
        foreach (var p in all.Where(p => p.UserId == userId))
        {
            p.IsActive = p.Id == planId;
            await repo.UpdateAsync(p);
            await queue.EnqueueAsync(p, SyncOperation.Update);
        }
        _ = Task.Run(() => sync.ProcessSyncQueueAsync());
    }

    public async Task DeleteAsync(Guid planId)
    {
        await repo.DeleteAsync(planId);  // IsDeleted = true + IsSynced = false

        var deletedEntity = await repo.GetByIdRawAsync(planId);
        if (deletedEntity is not null)
            await queue.EnqueueAsync(deletedEntity, SyncOperation.Delete);

        _ = Task.Run(() => sync.ProcessSyncQueueAsync());
    }
}