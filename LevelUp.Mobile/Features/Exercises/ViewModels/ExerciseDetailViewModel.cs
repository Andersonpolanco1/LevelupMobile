// Features/Exercises/ViewModels/ExerciseDetailViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LevelUp.Mobile.Core.Constants;
using LevelUp.Mobile.Core.Enums;
using LevelUp.Mobile.Core.Settings;
using LevelUp.Mobile.Features.Exercises.Models;
using LevelUp.Mobile.Infrastructure.Repositories;
using LevelUp.Mobile.Services;

namespace LevelUp.Mobile.Features.Exercises.ViewModels;

[QueryProperty(nameof(ExerciseId), "exerciseId")]
public partial class ExerciseDetailViewModel(
    ExerciseRepository exerciseRepo,
    MuscleRepository muscleRepo) : ObservableObject
{
    public string ExerciseId
    {
        get => _exerciseId.ToString();
        set
        {
            if (!Guid.TryParse(value, out var id)) return;
            _exerciseId = id;
            _ = LoadAsync();
        }
    }

    private Guid _exerciseId;

    [ObservableProperty] private bool _isBusy;

    private ExerciseDetailModel? _detail;
    public ExerciseDetailModel? Detail
    {
        get => _detail;
        private set => SetProperty(ref _detail, value);
    }

    private async Task LoadAsync()
    {
        if (_exerciseId == Guid.Empty) return;

        // ── Capturar todos los strings localizados en el MAIN THREAD ──
        // ResourceManager lanza MissingManifestResourceException si se accede
        // por primera vez desde un hilo de background en MAUI/Android.
        var language = AppPreferences.GetLanguage();
        var l = LocalizationService.Instance;

        var typeLabels = new Dictionary<ExerciseType, string>
        {
            [ExerciseType.WeightedStrength] = l["TypeWeightedStrength"],
            [ExerciseType.BodyweightStrength] = l["TypeBodyweightStrength"],
            [ExerciseType.RepetitionOnly] = l["TypeRepetitionOnly"],
            [ExerciseType.Isometric] = l["TypeIsometric"],
            [ExerciseType.Cardio] = l["TypeCardio"],
            [ExerciseType.Mobility] = l["TypeMobility"],
        };

        var roleLabels = new Dictionary<MuscleRole, string>
        {
            [MuscleRole.Primary] = l["MusclePrimary"],
            [MuscleRole.Secondary] = l["MuscleSecondary"],
            [MuscleRole.Stabilizer] = l["MuscleStabilizer"],
        };

        var requiredFieldsMap = new Dictionary<ExerciseType, List<string>>
        {
            [ExerciseType.WeightedStrength] = [l["FieldWeight"], l["FieldReps"]],
            [ExerciseType.BodyweightStrength] = [l["FieldReps"]],
            [ExerciseType.RepetitionOnly] = [l["FieldReps"]],
            [ExerciseType.Isometric] = [l["FieldDuration"]],
            [ExerciseType.Cardio] = [l["FieldDurationOrDistance"]],
            [ExerciseType.Mobility] = [l["FieldDuration"]],
        };

        var metricsMap = new Dictionary<ExerciseType, List<string>>
        {
            [ExerciseType.WeightedStrength] = [l["MetricVolume"], l["Metric1RM"], l["MetricWeightPR"], l["MetricWeightProgress"]],
            [ExerciseType.BodyweightStrength] = [l["MetricMaxReps"], l["MetricWeightProgress"]],
            [ExerciseType.RepetitionOnly] = [l["MetricTotalReps"], l["MetricWeeklyProgress"]],
            [ExerciseType.Isometric] = [l["MetricTotalTUT"], l["MetricDurationProgress"]],
            [ExerciseType.Cardio] = [l["MetricTotalTime"], l["MetricPace"], l["MetricDistance"], l["MetricCalories"]],
            [ExerciseType.Mobility] = [l["MetricMobilityTime"], l["MetricWeeklyConsistency"]],
        };

        var bodyweightLabel = l["Bodyweight"];

        IsBusy = true;
        try
        {
            // Trabajo pesado en background — ya no tocamos LocalizationService aquí
            var result = await Task.Run(async () =>
            {
                var exercise = await exerciseRepo.GetByIdAsync(_exerciseId);
                if (exercise is null) return null;

                var translation = await exerciseRepo.GetTranslationAsync(_exerciseId, language);
                var muscles = await muscleRepo.GetMusclesForExerciseAsync(_exerciseId, language);

                var muscleRoles = muscles
                    .GroupBy(m => m.Role)
                    .OrderBy(g => (int)g.Key)
                    .Select(g => new MuscleRoleGroup
                    {
                        RoleLabel = roleLabels.TryGetValue(g.Key, out var rl) ? rl : g.Key.ToString(),
                        RoleIcon = GetRoleIcon(g.Key),
                        MuscleNames = g.Select(m => m.MuscleName).OrderBy(n => n).ToList()
                    })
                    .ToList();

                var type = exercise.Type;

                return new ExerciseDetailModel
                {
                    Id = exercise.Id,
                    Name = translation?.Name ?? exercise.Id.ToString(),
                    ImageUrl = exercise.ImageUrl,
                    HasImage = !string.IsNullOrEmpty(exercise.ImageUrl),
                    TypeLabel = typeLabels.TryGetValue(type, out var tl) ? tl : type.ToString(),
                    TypeIcon = ExercisesViewModel.GetTypeIcon(type),
                    TypeBadgeColor = ExercisesViewModel.GetTypeBadgeColor(type),
                    Instructions = translation?.Instructions ?? "",
                    Tips = translation?.Tips,
                    CommonMistakes = translation?.CommonMistakes,
                    IncludesBodyWeight = exercise.IncludesBodyWeight,
                    BodyWeightFactor = exercise.BodyWeightFactor,
                    RequiredFields = requiredFieldsMap.TryGetValue(type, out var rf) ? rf : [],
                    MetricsAvailable = metricsMap.TryGetValue(type, out var m) ? m : [],
                    MuscleRoles = muscleRoles
                };
            });

            Detail = result;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string GetRoleIcon(MuscleRole role) => role switch
    {
        MuscleRole.Primary => FA.StarFilled,
        MuscleRole.Secondary => FA.Star,
        MuscleRole.Stabilizer => FA.Shield,
        _ => FA.Circle
    };

    [RelayCommand]
    private async Task GoBack()
        => await Shell.Current.GoToAsync("..");
}