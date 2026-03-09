using LevelUp.Mobile.Core.Enums;
using LevelUp.Mobile.Features.Home.Models;
using LevelUp.Mobile.Infrastructure.Repositories;

namespace LevelUp.Mobile.Services;

public class HomeService(WeeklyPlanRepository plans, ExerciseRepository exercises)
{
    public async Task<TodayPlanDto?> GetTodayAsync(Guid userId, Language language)
    {
        System.Diagnostics.Debug.WriteLine($"[HomeService] GetTodayAsync userId={userId}");

        // ✅ Primero verificar si existe un plan activo
        var activePlan = await plans.GetActivePlanAsync(userId);
        System.Diagnostics.Debug.WriteLine($"[HomeService] activePlan={activePlan?.Name ?? "NULL"}");

        // Sin plan activo → null → HasNoPlan = true
        if (activePlan is null) return null;

        // Buscar el día de hoy en el plan
        var day = await plans.GetTodayDayAsync(userId);
        System.Diagnostics.Debug.WriteLine($"[HomeService] todayDay={day?.DayOfWeek.ToString() ?? "NULL"} (hoy={DateTime.Now.DayOfWeek})");

        // Plan existe pero hoy no tiene día → día de descanso
        if (day is null)
        {
            return new TodayPlanDto
            {
                PlanName = activePlan.Name,
                DayName = DateTime.Now.DayOfWeek.ToString(),
                DayOfWeek = DateTime.Now.DayOfWeek,
                Exercises = []   // lista vacía → HasRestDay = true
            };
        }

        // Hay día configurado → cargar ejercicios
        var planExercises = await plans.GetExercisesForDayAsync(day.Id);
        var exerciseIds = planExercises.Select(e => e.ExerciseId).ToList();
        var withTranslations = await exercises.GetWithTranslationAsync(language);
        var relevant = withTranslations
            .Where(t => exerciseIds.Contains(t.Exercise.Id))
            .ToList();

        return new TodayPlanDto
        {
            PlanName = activePlan.Name,
            DayName = day.DayOfWeek.ToString(),
            DayOfWeek = day.DayOfWeek,
            Notes = day.Notes,
            Exercises = planExercises.Select(pe =>
            {
                var match = relevant.FirstOrDefault(r => r.Exercise.Id == pe.ExerciseId);
                return new TodayExerciseDto
                {
                    ExerciseId = pe.ExerciseId,
                    ExerciseName = match.Translation?.Name ?? "—",
                    SetsPlanned = pe.SetsPlanned,
                    RepsPlanned = pe.RepsPlanned,
                    Order = pe.Order
                };
            }).OrderBy(e => e.Order).ToList()
        };
    }
}