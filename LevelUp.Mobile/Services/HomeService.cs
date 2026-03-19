using LevelUp.Mobile.Core.Enums;
using LevelUp.Mobile.Features.Home.Models;
using LevelUp.Mobile.Infrastructure.Repositories;

namespace LevelUp.Mobile.Services;

public class HomeService(
    WeeklyPlanRepository plans,
    ExerciseRepository exercises,
    MuscleRepository muscles)
{
    public async Task<TodayPlanDto?> GetTodayAsync(Guid userId, Language language)
    {
        System.Diagnostics.Debug.WriteLine($"[HomeService] GetTodayAsync userId={userId}");

        var activePlan = await plans.GetActivePlanAsync(userId);
        System.Diagnostics.Debug.WriteLine($"[HomeService] activePlan={activePlan?.Name ?? "NULL"}");

        if (activePlan is null) return null;

        var day = await plans.GetTodayDayAsync(userId);
        System.Diagnostics.Debug.WriteLine($"[HomeService] todayDay={day?.DayOfWeek.ToString() ?? "NULL"}");

        if (day is null)
        {
            return new TodayPlanDto
            {
                PlanName = activePlan.Name,
                DayName = DateTime.Now.DayOfWeek.ToString(),
                DayOfWeek = DateTime.Now.DayOfWeek,
                Exercises = []
            };
        }

        var planExercises = await plans.GetExercisesForDayAsync(day.Id);
        var exerciseIds = planExercises.Select(e => e.ExerciseId).ToList();
        var withTranslations = await exercises.GetWithTranslationAsync(language);
        var muscleGroupMap = await muscles.GetPrimaryMuscleGroupByExerciseIdsAsync(exerciseIds, language);

        var relevant = withTranslations
            .Where(t => exerciseIds.Contains(t.Exercise.Id))
            .ToDictionary(t => t.Exercise.Id);

        return new TodayPlanDto
        {
            PlanName = activePlan.Name,
            DayName = day.DayOfWeek.ToString(),
            DayOfWeek = day.DayOfWeek,
            Notes = day.Notes,
            Exercises = planExercises.Select(pe =>
            {
                relevant.TryGetValue(pe.ExerciseId, out var match);
                muscleGroupMap.TryGetValue(pe.ExerciseId, out var muscleGroupName);

                return new TodayExerciseDto
                {
                    ExerciseId = pe.ExerciseId,
                    ExerciseName = match.Translation?.Name ?? "—",
                    ImageUrl = match.Exercise.ImageUrl,
                    MuscleGroupName = muscleGroupName,
                    SetsPlanned = pe.SetsPlanned,
                    RepsPlanned = pe.RepsPlanned,
                    Order = pe.Order
                };
            })
            .OrderBy(e => e.MuscleGroupName ?? "zzz")
            .ThenBy(e => e.Order)
            .ToList()
        };
    }
}