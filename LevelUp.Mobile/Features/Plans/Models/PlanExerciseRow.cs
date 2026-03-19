using LevelUp.Mobile.Core.Entities;

namespace LevelUp.Mobile.Features.Plans.Models;

public class PlanExerciseRow
{
    public WeeklyPlanExercise Exercise { get; init; } = null!;
    public string ExerciseName { get; init; } = "";
    public string? ImageUrl { get; init; }
    public string? MuscleGroupName { get; init; }

    // IExerciseRow
    public Guid ExerciseId => Exercise.ExerciseId;
    public int Order { get; set; }

    public string? SetsText => Exercise.SetsPlanned.HasValue
        ? $"{Exercise.SetsPlanned} sets" : null;

    public string? RepsText => Exercise.RepsPlanned.HasValue
        ? $"{Exercise.RepsPlanned} reps" : null;

    public string? DurationText
    {
        get
        {
            if (Exercise.DurationPlanned is not { } d) return null;
            return d.TotalMinutes >= 1 ? $"{(int)d.TotalMinutes} min" : $"{d.Seconds} s";
        }
    }

    public string? RestText
    {
        get
        {
            if (Exercise.RestTimePlanned is not { } r) return null;
            return r.TotalMinutes >= 1 ? $"{(int)r.TotalMinutes} min rest" : $"{r.Seconds} s rest";
        }
    }
}

public class PlanExerciseGroup : List<PlanExerciseRow>
{
    public string MuscleGroupName { get; }
    public PlanExerciseGroup(string name, IEnumerable<PlanExerciseRow> items) : base(items)
        => MuscleGroupName = name;
}