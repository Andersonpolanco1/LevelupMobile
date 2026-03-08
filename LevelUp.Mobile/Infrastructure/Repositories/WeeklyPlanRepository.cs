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
        var db = await GetDbAsync();
        var plan = await GetActivePlanAsync(userId);
        if (plan is null) return null;

        var today = DateTime.Now.DayOfWeek;
        return await db.Table<WeeklyPlanDay>()
            .Where(d => d.WeeklyPlanId == plan.Id
                     && d.DayOfWeek == today
                     && !d.IsDeleted)
            .FirstOrDefaultAsync();
    }
}