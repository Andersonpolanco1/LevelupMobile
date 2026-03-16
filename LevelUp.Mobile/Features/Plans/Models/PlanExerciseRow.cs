using LevelUp.Mobile.Core.Entities;

namespace LevelUp.Mobile.Features.Plans.Models;

/// <summary>
/// Modelo de presentación que une WeeklyPlanExercise con el nombre traducido del ejercicio.
/// </summary>
public class PlanExerciseRow
{
    public WeeklyPlanExercise Exercise { get; init; } = null!;

    /// <summary>Nombre localizado del ejercicio.</summary>
    public string ExerciseName { get; init; } = "";

    /// <summary>Número de orden visible (1-based).</summary>
    public int Order => Exercise.Order + 1;

    // ── Display helpers ───────────────────────────────────────────────

    public string? SetsText => Exercise.SetsPlanned.HasValue
        ? $"{Exercise.SetsPlanned} sets"
        : null;

    public string? RepsText => Exercise.RepsPlanned.HasValue
        ? $"{Exercise.RepsPlanned} reps"
        : null;

    public string? DurationText
    {
        get
        {
            if (Exercise.DurationPlanned is not { } d) return null;
            return d.TotalMinutes >= 1
                ? $"{(int)d.TotalMinutes} min"
                : $"{d.Seconds} s";
        }
    }

    public string? RestText
    {
        get
        {
            if (Exercise.RestTimePlanned is not { } r) return null;
            return r.TotalMinutes >= 1
                ? $"{(int)r.TotalMinutes} min rest"
                : $"{r.Seconds} s rest";
        }
    }
}