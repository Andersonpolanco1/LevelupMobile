// Services/HomeService.cs
using LevelUp.Mobile.Core.Entities;
using LevelUp.Mobile.Core.Enums;
using LevelUp.Mobile.Infrastructure.Repositories;

namespace LevelUp.Mobile.Services;

public class HomeService(WeeklyPlanRepository plans, ExerciseRepository exercises)
{

    /// <summary>Lee 100% desde local. El sync ya llenó la DB.</summary>
    public async Task<TodayLocalDto?> GetTodayAsync(Guid userId, Language language)
    {
        var day = await plans.GetTodayDayAsync(userId);
        if (day is null) return null;

        var planExercises = await plans.GetExercisesForDayAsync(day.Id);
        var exerciseIds = planExercises.Select(e => e.ExerciseId).ToList();

        var withTranslations = await exercises.GetWithTranslationAsync(language);
        var relevant = withTranslations
            .Where(t => exerciseIds.Contains(t.Exercise.Id))
            .ToList();

        return new TodayLocalDto
        {
            DayOfWeek = day.DayOfWeek,
            Notes = day.Notes,
            Exercises = planExercises.Select(pe =>
            {
                var match = relevant.FirstOrDefault(r => r.Exercise.Id == pe.ExerciseId);
                return new TodayExerciseLocalDto
                {
                    ExerciseId = pe.ExerciseId,
                    Name = match.Translation?.Name ?? "—",
                    SetsPlanned = pe.SetsPlanned,
                    RepsPlanned = pe.RepsPlanned,
                    Order = pe.Order
                };
            }).OrderBy(e => e.Order).ToList()
        };
    }
}

// DTOs locales (no de API)
public record TodayLocalDto
{
    public DayOfWeek DayOfWeek { get; init; }
    public string? Notes { get; init; }
    public List<TodayExerciseLocalDto> Exercises { get; init; } = [];
}

public record TodayExerciseLocalDto
{
    public Guid ExerciseId { get; init; }
    public string Name { get; init; } = "";
    public int? SetsPlanned { get; init; }
    public int? RepsPlanned { get; init; }
    public int Order { get; init; }
}