// Infrastructure/Repositories/WeeklyPlanRepository.cs
using LevelUp.Mobile.Core.Entities;
using LevelUp.Mobile.Infrastructure.LocalDb;

namespace LevelUp.Mobile.Infrastructure.Repositories;

public class WeeklyPlanRepository : BaseRepository<WeeklyPlan>
{
    public WeeklyPlanRepository(LocalDatabase db) : base(db) { }

    public async Task<WeeklyPlan?> GetActivePlanAsync(Guid userId)
    {
        var db = await GetDbAsync();
        return await db.Table<WeeklyPlan>()
            .Where(p => p.UserId == userId && p.IsActive && !p.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<List<WeeklyPlanDay>> GetDaysAsync(Guid planId)
    {
        var db = await GetDbAsync();
        return await db.Table<WeeklyPlanDay>()
            .Where(d => d.WeeklyPlanId == planId && !d.IsDeleted)
            .ToListAsync();
    }

    public async Task<List<WeeklyPlanExercise>> GetExercisesForDayAsync(Guid dayId)
    {
        var db = await GetDbAsync();
        return await db.Table<WeeklyPlanExercise>()
            .Where(e => e.WeeklyPlanDayId == dayId && !e.IsDeleted)
            .OrderBy(e => e.Order)
            .ToListAsync();
    }

    public async Task<WeeklyPlanDay?> GetTodayDayAsync(Guid userId)
    {
        var today = DateTime.Now.DayOfWeek;
        var db = await GetDbAsync();

        var activePlan = await GetActivePlanAsync(userId);
        if (activePlan is null) return null;

        return await db.Table<WeeklyPlanDay>()
            .Where(d => d.WeeklyPlanId == activePlan.Id
                     && d.DayOfWeek == today
                     && !d.IsDeleted)
            .FirstOrDefaultAsync();
    }

    // WeeklyPlanRepository.cs

    public async Task InsertDayAsync(WeeklyPlanDay day)
    {
        var db = await GetDbAsync();
        await db.InsertAsync(day);
    }

    public async Task DeleteDayAsync(Guid dayId)
    {
        var db = await GetDbAsync();
        var day = await db.Table<WeeklyPlanDay>()
            .Where(d => d.Id == dayId)
            .FirstOrDefaultAsync();
        if (day is null) return;
        day.IsDeleted = true;
        day.UpdatedAt = DateTime.UtcNow;
        day.IsSynced = false;
        await db.UpdateAsync(day);
    }

    public async Task UpdateDayAsync(WeeklyPlanDay day)
    {
        var db = await GetDbAsync();
        await db.UpdateAsync(day);
    }

    public async Task UpdatePlanAsync(WeeklyPlan plan)
    {
        var db = await GetDbAsync();
        await db.UpdateAsync(plan);
    }

    // Cuenta ejercicios activos de un día
    public async Task<int> GetExerciseCountForDayAsync(Guid dayId)
    {
        var db = await GetDbAsync();
        return await db.Table<WeeklyPlanExercise>()
            .Where(e => e.WeeklyPlanDayId == dayId && !e.IsDeleted)
            .CountAsync();
    }

    // WeeklyPlanRepository.cs
    public async Task<WeeklyPlanDay?> GetDayByIdAsync(Guid dayId)
    {
        var db = await GetDbAsync();
        return await db.Table<WeeklyPlanDay>()
            .Where(d => d.Id == dayId && !d.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<WeeklyPlanDay?> GetDayByIdRawAsync(Guid dayId)
    {
        var db = await GetDbAsync();
        return await db.Table<WeeklyPlanDay>()
            .Where(d => d.Id == dayId)
            .FirstOrDefaultAsync();
    }
}