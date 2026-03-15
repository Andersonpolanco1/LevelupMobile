using LevelUp.Mobile.Core.Abstractions;
using LevelUp.Mobile.Core.Entities;
using LevelUp.Mobile.Core.Enums;
using LevelUp.Mobile.Infrastructure.Repositories;

namespace LevelUp.Mobile.Services;

public class WeeklyPlanService(
    WeeklyPlanRepository repo, ISyncQueue queue, ISyncService sync)
{
    // ── Planes ────────────────────────────────────────────────────────

    public Task<List<WeeklyPlan>> GetPlansAsync()
        => repo.GetAllAsync();

    public Task<WeeklyPlan?> GetByIdAsync(Guid id)
        => repo.GetByIdAsync(id);

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

    public async Task UpdatePlanInfoAsync(Guid planId, string name, string? notes)
    {
        var plan = await repo.GetByIdAsync(planId);
        if (plan is null) return;
        plan.Name = name;
        plan.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes;
        plan.UpdatedAt = DateTime.UtcNow;
        plan.IsSynced = false;
        await repo.UpdatePlanAsync(plan);
        await queue.EnqueueAsync(plan, SyncOperation.Update);
        _ = Task.Run(() => sync.ProcessSyncQueueAsync());
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

    // Kept for backwards compat
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

    // ── Días ──────────────────────────────────────────────────────────

    public Task<List<WeeklyPlanDay>> GetDaysAsync(Guid planId)
        => repo.GetDaysAsync(planId);

    public Task<WeeklyPlanDay?> GetDayByIdAsync(Guid dayId)
        => repo.GetDayByIdAsync(dayId);

    public async Task<WeeklyPlanDay> AddDayAsync(Guid planId, DayOfWeek day, string? notes)
    {
        var existing = await repo.GetDaysAsync(planId);
        var duplicate = existing.FirstOrDefault(d => d.DayOfWeek == day);
        if (duplicate is not null)
            return duplicate;

        var newDay = new WeeklyPlanDay
        {
            Id = Guid.NewGuid(),
            WeeklyPlanId = planId,
            DayOfWeek = day,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes,
            CreatedAt = DateTime.UtcNow,
            IsSynced = false
        };
        await repo.InsertDayAsync(newDay);
        await queue.EnqueueAsync(newDay, SyncOperation.Create);
        _ = Task.Run(() => sync.ProcessSyncQueueAsync());
        return newDay;
    }

    /// <summary>
    /// Elimina un día. Si tiene ejercicios retorna false — el caller
    /// debe pedir confirmación y llamar ForceRemoveDayAsync si confirma.
    /// </summary>
    public async Task<bool> RemoveDayAsync(Guid dayId)
    {
        var count = await repo.GetExerciseCountForDayAsync(dayId);
        if (count > 0) return false;

        await repo.DeleteDayAsync(dayId);
        var deleted = await repo.GetDayByIdRawAsync(dayId);
        if (deleted is not null)
            await queue.EnqueueAsync(deleted, SyncOperation.Delete);
        _ = Task.Run(() => sync.ProcessSyncQueueAsync());
        return true;
    }

    /// <summary>
    /// Elimina un día aunque tenga ejercicios (tras confirmación del usuario).
    /// </summary>
    public async Task ForceRemoveDayAsync(Guid dayId)
    {
        await repo.DeleteDayAsync(dayId);
        var deleted = await repo.GetDayByIdRawAsync(dayId);
        if (deleted is not null)
            await queue.EnqueueAsync(deleted, SyncOperation.Delete);
        _ = Task.Run(() => sync.ProcessSyncQueueAsync());
    }

    public async Task UpdateDayNotesAsync(Guid dayId, string? notes)
    {
        var day = await repo.GetDayByIdAsync(dayId);
        if (day is null) return;
        day.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes;
        day.UpdatedAt = DateTime.UtcNow;
        day.IsSynced = false;
        await repo.UpdateDayAsync(day);
        await queue.EnqueueAsync(day, SyncOperation.Update);
        _ = Task.Run(() => sync.ProcessSyncQueueAsync());
    }

    public Task<int> GetExerciseCountForDayAsync(Guid dayId)
        => repo.GetExerciseCountForDayAsync(dayId);

    // WeeklyPlanService.cs — método temporal de limpieza
    public async Task DeduplicateDaysAsync(Guid planId)
    {
        var days = await repo.GetDaysAsync(planId);
        var seen = new HashSet<DayOfWeek>();

        foreach (var day in days.OrderBy(d => d.CreatedAt))
        {
            if (!seen.Add(day.DayOfWeek))
            {
                // Eliminar de la tabla WeeklyPlanDay
                await repo.DeleteDayAsync(day.Id);

                // Eliminar de la cola de sync — no queremos sincronizar basura
                await queue.RemoveByEntityIdAsync(day.Id);
            }
        }
    }
}