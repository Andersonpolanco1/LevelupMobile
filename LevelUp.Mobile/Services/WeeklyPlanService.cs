using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Core.Entities;
using LevelUp.Mobile.Core.Enums;
using LevelUp.Mobile.Infrastructure.Repositories;

namespace LevelUp.Mobile.Services;

public class WeeklyPlanService(
    WeeklyPlanRepository repo, ISyncQueue queue, ISyncService sync)
{
    public Task<List<WeeklyPlan>> GetPlansAsync()
        => repo.GetAllAsync();

    public Task<WeeklyPlan?> GetByIdAsync(Guid id)
        => repo.GetByIdAsync(id);

    public async Task<List<WeeklyPlanDay>> GetDaysAsync(Guid planId)
        => await repo.GetDaysAsync(planId);

    public async Task<WeeklyPlan> CreateAsync(Guid userId, string name, string? notes)
    {
        var plan = new WeeklyPlan
        {
            UserId = userId,
            Name = name,
            Notes = notes
        };
        await repo.InsertAsync(plan);
        await queue.EnqueueAsync(plan, SyncOperation.Create);
        _ = Task.Run(() => sync.ProcessSyncQueueAsync());
        return plan;
    }

    /// <summary>
    /// Desactiva todos los planes del usuario y activa el indicado.
    /// Encola un Update por cada plan modificado.
    /// </summary>
    public async Task ActivateAsync(Guid userId, Guid planId)
    {
        var all = await repo.GetAllAsync();

        foreach (var p in all.Where(p => p.UserId == userId))
        {
            var wasActive = p.IsActive;
            p.IsActive = p.Id == planId;

            // Solo persistir si cambió algo
            if (p.IsActive != wasActive)
            {
                p.UpdatedAt = DateTime.UtcNow;
                p.IsSynced = false;
                await repo.UpdateAsync(p);
                await queue.EnqueueAsync(p, SyncOperation.Update);
            }
        }

        _ = Task.Run(() => sync.ProcessSyncQueueAsync());
    }

    // Kept for backwards compat with older callers (userId, planId order)
    public async Task ActivatePlanAsync(Guid planId, Guid userId)
        => await ActivateAsync(userId, planId);

    public async Task DeleteAsync(Guid planId)
    {
        await repo.DeleteAsync(planId);
        var deletedEntity = await repo.GetByIdRawAsync(planId);
        if (deletedEntity is not null)
            await queue.EnqueueAsync(deletedEntity, SyncOperation.Delete);
        _ = Task.Run(() => sync.ProcessSyncQueueAsync());
    }
}