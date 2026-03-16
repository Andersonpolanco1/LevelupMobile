using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LevelUp.Mobile.Core.Entities;
using LevelUp.Mobile.Core.Settings;
using LevelUp.Mobile.Features.Plans.Models;
using LevelUp.Mobile.Infrastructure.Repositories;
using LevelUp.Mobile.Services;
using System.Collections.ObjectModel;

namespace LevelUp.Mobile.Features.Plans.ViewModels;

[QueryProperty(nameof(DayId), "dayId")]
public partial class PlanDayDetailViewModel(
    WeeklyPlanService planService,
    ExerciseRepository exerciseRepo) : ObservableObject
{
    // ── Query property ────────────────────────────────────────────────

    private Guid _dayId;
    public string DayId
    {
        get => _dayId.ToString();
        set
        {
            if (Guid.TryParse(value, out var id))
            {
                _dayId = id;
                _ = LoadAsync();
            }
        }
    }

    // ── State ─────────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DayName))]
    private WeeklyPlanDay? _day;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasExercises))]
    [NotifyPropertyChangedFor(nameof(ExerciseCount))]
    private ObservableCollection<PlanExerciseRow> _exercises = [];

    [ObservableProperty]
    private bool _isBusy;

    public bool HasExercises => Exercises.Count > 0;
    public int ExerciseCount => Exercises.Count;

    public string DayName => Day?.DayOfWeek switch
    {
        DayOfWeek.Monday => LocalizationService.Instance["DayMonFull"],
        DayOfWeek.Tuesday => LocalizationService.Instance["DayTueFull"],
        DayOfWeek.Wednesday => LocalizationService.Instance["DayWedFull"],
        DayOfWeek.Thursday => LocalizationService.Instance["DayThuFull"],
        DayOfWeek.Friday => LocalizationService.Instance["DayFriFull"],
        DayOfWeek.Saturday => LocalizationService.Instance["DaySatFull"],
        DayOfWeek.Sunday => LocalizationService.Instance["DaySunFull"],
        _ => ""
    };

    // ── Load ──────────────────────────────────────────────────────────

    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            Day = await planService.GetDayByIdAsync(_dayId);
            if (Day is null) return;
            await RefreshExercisesAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task RefreshExercisesAsync()
    {
        var language = AppPreferences.GetLanguage();

        var withTranslation = await exerciseRepo.GetWithTranslationAsync(language);
        var nameMap = withTranslation
            .ToDictionary(x => x.Exercise.Id, x => x.Translation?.Name ?? x.Exercise.Id.ToString());

        var planExercises = await planService.GetExercisesForDayAsync(_dayId);

        var rows = planExercises
            .OrderBy(pe => pe.Order)
            .Select(pe => new PlanExerciseRow
            {
                Exercise = pe,
                ExerciseName = nameMap.TryGetValue(pe.ExerciseId, out var name) ? name : ""
            })
            .ToList();

        Exercises = new ObservableCollection<PlanExerciseRow>(rows);
        OnPropertyChanged(nameof(HasExercises));
        OnPropertyChanged(nameof(ExerciseCount));
    }

    // ── Refresh al volver del picker ──────────────────────────────────

    // Llama este método desde el code-behind de PlanDayDetailPage
    // en OnAppearing para refrescar cuando el usuario vuelve del picker.
    public async Task OnAppearingAsync()
    {
        if (_dayId == Guid.Empty) return;
        await RefreshExercisesAsync();
    }

    // ── Commands ──────────────────────────────────────────────────────

    [RelayCommand]
    private async Task GoBack()
        => await Shell.Current.GoToAsync("..");

    [RelayCommand]
    private async Task EditNotes()
    {
        if (Day is null) return;

        var result = await Shell.Current.DisplayPromptAsync(
            "Day Notes",
            "Add a note for this day:",
            initialValue: Day.Notes ?? "",
            maxLength: 200,
            keyboard: Keyboard.Text);

        if (result is null) return;

        await planService.UpdateDayNotesAsync(_dayId, result);
        Day = await planService.GetDayByIdAsync(_dayId);
    }

    [RelayCommand]
    private async Task AddExercise()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($">>> AddExercise: navigating, dayId={_dayId}");
            await Shell.Current.GoToAsync($"exercisePicker?dayId={_dayId}");
            System.Diagnostics.Debug.WriteLine(">>> AddExercise: navigation done");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($">>> AddExercise CRASH: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($">>> INNER: {ex.InnerException?.Message}");
            System.Diagnostics.Debug.WriteLine($">>> STACK: {ex.StackTrace}");
        }
    }

    [RelayCommand]
    private async Task RemoveExercise(PlanExerciseRow row)
    {
        bool confirm = await Shell.Current.DisplayAlert(
            "Remove exercise",
            $"Remove \"{row.ExerciseName}\" from this day?",
            "Remove", "Cancel");

        if (!confirm) return;

        await planService.RemovePlanExerciseAsync(row.Exercise.Id);
        await RefreshExercisesAsync();
    }
}