using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LevelUp.Mobile.Core.Settings;
using LevelUp.Mobile.Features.Plans.Models;
using LevelUp.Mobile.Infrastructure.Repositories;
using LevelUp.Mobile.Services;
using System.Collections.ObjectModel;

namespace LevelUp.Mobile.Features.Plans.ViewModels;

[QueryProperty(nameof(DayId), "dayId")]
public partial class ExercisePickerViewModel(
    ExerciseRepository exerciseRepo,
    MuscleRepository muscleRepo,
    WeeklyPlanService planService) : ObservableObject
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
    private bool _isBusy;

    [ObservableProperty]
    private ObservableCollection<MuscleGroupRow> _muscleGroups = [];

    [ObservableProperty]
    private ObservableCollection<ExercisePickerRow> _filteredExercises = [];

    // SelectedCount es int simple, no usa [NotifyPropertyChangedFor] sobre sí mismo
    private int _selectedCount;
    public int SelectedCount
    {
        get => _selectedCount;
        private set => SetProperty(ref _selectedCount, value);
    }

    public bool IsEmpty => !IsBusy && FilteredExercises.Count == 0;

    // Filter state
    private string _searchText = "";
    private Guid? _selectedGroupId;

    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            ApplyFilters();
        }
    }

    public MuscleGroupRow? SelectedGroup
        => _muscleGroups.FirstOrDefault(g => g.Id == _selectedGroupId);

    // ── Internal data ─────────────────────────────────────────────────

    private List<ExercisePickerRow> _allExercises = [];
    private Dictionary<Guid, HashSet<Guid>> _groupExerciseCache = [];
    private HashSet<Guid> _preExistingIds = [];

    // ── Load ──────────────────────────────────────────────────────────

    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var language = AppPreferences.GetLanguage();

            // 1. Grupos musculares
            var groups = await muscleRepo.GetGroupsWithTranslationAsync(language);
            MuscleGroups = new ObservableCollection<MuscleGroupRow>(
                groups
                    .Select(g => new MuscleGroupRow
                    {
                        Id = g.Group.Id,
                        Name = g.Translation?.Name ?? g.Group.Id.ToString()
                    })
                    .OrderBy(g => g.Name));

            // 2. Ejercicios con nombre traducido
            var withTranslation = await exerciseRepo.GetWithTranslationAsync(language);
            _allExercises = withTranslation
                .Select(x => new ExercisePickerRow
                {
                    Exercise = x.Exercise,
                    Name = x.Translation?.Name ?? x.Exercise.Id.ToString()
                })
                .OrderBy(x => x.Name)
                .ToList();

            // 3. Pre-marcar los que ya están en el día
            var existingInDay = await planService.GetExercisesForDayAsync(_dayId);
            _preExistingIds = existingInDay.Select(e => e.ExerciseId).ToHashSet();

            foreach (var row in _allExercises.Where(r => _preExistingIds.Contains(r.Exercise.Id)))
                row.IsSelected = true;

            SelectedCount = _allExercises.Count(r => r.IsSelected);

            ApplyFilters();
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ── Filtering ─────────────────────────────────────────────────────

    private async void ApplyFilters()
    {
        IEnumerable<ExercisePickerRow> result = _allExercises;

        if (_selectedGroupId.HasValue)
        {
            if (!_groupExerciseCache.TryGetValue(_selectedGroupId.Value, out var ids))
            {
                ids = await muscleRepo.GetExerciseIdsByMuscleGroupAsync(_selectedGroupId.Value);
                _groupExerciseCache[_selectedGroupId.Value] = ids;
            }
            result = result.Where(r => ids.Contains(r.Exercise.Id));
        }

        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            var q = _searchText.Trim().ToLowerInvariant();
            result = result.Where(r => r.Name.ToLowerInvariant().Contains(q));
        }

        FilteredExercises = new ObservableCollection<ExercisePickerRow>(result);
        OnPropertyChanged(nameof(IsEmpty));
    }

    // ── Commands ──────────────────────────────────────────────────────

    [RelayCommand]
    private void SelectGroup(MuscleGroupRow group)
    {
        // Toggle: segundo tap en el mismo chip lo deselecciona
        _selectedGroupId = _selectedGroupId == group.Id ? null : group.Id;
        OnPropertyChanged(nameof(SelectedGroup));
        ApplyFilters();
    }

    [RelayCommand]
    private void ClearGroupFilter()
    {
        _searchText = "";
        OnPropertyChanged(nameof(SearchText));
        _selectedGroupId = null;
        OnPropertyChanged(nameof(SelectedGroup));
        ApplyFilters();
    }

    [RelayCommand]
    private async Task ToggleExercise(ExercisePickerRow row)
    {
        row.IsSelected = !row.IsSelected;

        // Mantener sincronizado _allExercises si el item viene del filtro
        var master = _allExercises.FirstOrDefault(r => r.Exercise.Id == row.Exercise.Id);
        if (master is not null && !ReferenceEquals(master, row))
            master.IsSelected = row.IsSelected;

        if (row.IsSelected)
        {
            if (!_preExistingIds.Contains(row.Exercise.Id))
            {
                await planService.AddExerciseToDayAsync(_dayId, row.Exercise.Id);
                _preExistingIds.Add(row.Exercise.Id);
            }
        }
        else
        {
            await planService.RemoveExerciseFromDayAsync(_dayId, row.Exercise.Id);
            _preExistingIds.Remove(row.Exercise.Id);
        }

        SelectedCount = _allExercises.Count(r => r.IsSelected);
    }

    [RelayCommand]
    private async Task GoBack()
        => await Shell.Current.GoToAsync("..");
}