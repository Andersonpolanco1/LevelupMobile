// Features/Exercises/ViewModels/ExercisesViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LevelUp.Mobile.Core.Constants;
using LevelUp.Mobile.Core.Enums;
using LevelUp.Mobile.Core.Settings;
using LevelUp.Mobile.Features.Exercises.Models;
using LevelUp.Mobile.Infrastructure.Repositories;
using LevelUp.Mobile.Services;
using System.Collections.ObjectModel;

namespace LevelUp.Mobile.Features.Exercises.ViewModels;

public partial class ExercisesViewModel(
    ExerciseRepository exerciseRepo,
    MuscleRepository muscleRepo,
    AppState appState) : ObservableObject
{
    // ── State ─────────────────────────────────────────────────────────

    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isLoadingMore;
    [ObservableProperty] private bool _isFiltersExpanded;

    [ObservableProperty]
    private ObservableCollection<MuscleGroupChip> _muscleGroups = [];

    [ObservableProperty]
    private ObservableCollection<FilterChip> _exerciseTypeFilters = [];

    [ObservableProperty]
    private ObservableCollection<ExerciseRow> _filteredExercises = [];

    [ObservableProperty] private bool _filterBodyweight;

    partial void OnFilterBodyweightChanged(bool value) => ApplyFilters();

    // ── Contadores ────────────────────────────────────────────────────

    private int _totalCount;
    public int TotalCount
    {
        get => _totalCount;
        private set => SetProperty(ref _totalCount, value);
    }

    public int ActiveFilterCount
    {
        get
        {
            int count = 0;
            if (_selectedGroupId.HasValue) count++;
            if (_selectedTypeKeys.Count > 0) count++;
            if (_filterBodyweight) count++;
            return count;
        }
    }

    public bool IsEmpty => !IsBusy && FilteredExercises.Count == 0;

    // ── Paginación ────────────────────────────────────────────────────

    private const int PageSize = 30;
    private int _currentPage = 0;
    private List<ExerciseRow> _filteredCache = [];

    // ── Cache / filtros ───────────────────────────────────────────────

    private List<ExerciseRow> _allExercises = [];
    private Dictionary<Guid, HashSet<Guid>> _groupExerciseCache = [];

    private string _searchText = "";
    private Guid? _selectedGroupId;
    private readonly HashSet<ExerciseType> _selectedTypeKeys = [];

    public string SearchText
    {
        get => _searchText;
        set { _searchText = value; ApplyFilters(); }
    }

    // ── Inicialización ────────────────────────────────────────────────

    public async Task InitializeAsync()
    {
        if (!appState.ExercisesNeedsRefresh() && _allExercises.Count > 0) return;
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var language = AppPreferences.GetLanguage();

            var (groupChips, exerciseRows) = await Task.Run(async () =>
            {
                var groups = await muscleRepo.GetGroupsWithTranslationAsync(language);
                var chips = groups
                    .Select(g => new MuscleGroupChip
                    {
                        Id = g.Group.Id,
                        Name = g.Translation?.Name ?? g.Group.Id.ToString()
                    })
                    .OrderBy(g => g.Name)
                    .ToList();

                var withTranslation = await exerciseRepo.GetWithTranslationAsync(language);
                var primaryMusclesMap = await exerciseRepo.GetPrimaryMuscleNamesAsync(language);

                var rows = withTranslation
                    .Select(x =>
                    {
                        primaryMusclesMap.TryGetValue(x.Exercise.Id, out var muscles);
                        return new ExerciseRow
                        {
                            Exercise = x.Exercise,
                            Name = x.Translation?.Name ?? x.Exercise.Id.ToString(),
                            PrimaryMuscles = muscles ?? "",
                            ImageUrl = x.Exercise.ImageUrl,
                            HasImage = !string.IsNullOrEmpty(x.Exercise.ImageUrl),
                            TypeIcon = GetTypeIcon(x.Exercise.Type),
                            TypeBadgeColor = GetTypeBadgeColor(x.Exercise.Type)
                        };
                    })
                    .OrderBy(x => x.Name)
                    .ToList();

                return (chips, rows);
            });

            _allExercises = exerciseRows;
            _groupExerciseCache.Clear();

            // BuildTypeFilters ahora usa el singleton que ya funciona en toda la app
            var typeFilters = BuildTypeFilters();

            MuscleGroups = new ObservableCollection<MuscleGroupChip>(groupChips);
            ExerciseTypeFilters = new ObservableCollection<FilterChip>(typeFilters);
            ApplyFilters();

            appState.ExercisesLoaded();
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ── Helpers de tipo ───────────────────────────────────────────────

    public static string GetTypeIcon(ExerciseType type) => type switch
    {
        ExerciseType.WeightedStrength => FA.Dumbbell,
        ExerciseType.BodyweightStrength => FA.PersonRunning,
        ExerciseType.RepetitionOnly => FA.ArrowsRotate,
        ExerciseType.Isometric => FA.HourglassHalf,
        ExerciseType.Cardio => FA.HeartPulse,
        ExerciseType.Mobility => FA.PersonArmsOut,
        _ => FA.Dumbbell
    };

    public static string GetTypeBadgeColor(ExerciseType type) => type switch
    {
        ExerciseType.WeightedStrength => "#FF6600",
        ExerciseType.BodyweightStrength => "#3B82F6",
        ExerciseType.RepetitionOnly => "#8B5CF6",
        ExerciseType.Isometric => "#F59E0B",
        ExerciseType.Cardio => "#EF4444",
        ExerciseType.Mobility => "#22C55E",
        _ => "#FF6600"
    };

    // ── BuildTypeFilters: usa el singleton, igual que el resto de la app ──
    private static List<FilterChip> BuildTypeFilters()
    {
        var l = LocalizationService.Instance;
        return
        [
            new FilterChip { Key = ExerciseType.WeightedStrength,   Icon = FA.Dumbbell,      Label = l["TypeWeightedStrength"] },
            new FilterChip { Key = ExerciseType.BodyweightStrength, Icon = FA.PersonRunning, Label = l["TypeBodyweightStrength"] },
            new FilterChip { Key = ExerciseType.RepetitionOnly,     Icon = FA.ArrowsRotate,  Label = l["TypeRepetitionOnly"] },
            new FilterChip { Key = ExerciseType.Isometric,          Icon = FA.HourglassHalf, Label = l["TypeIsometric"] },
            new FilterChip { Key = ExerciseType.Cardio,             Icon = FA.HeartPulse,    Label = l["TypeCardio"] },
            new FilterChip { Key = ExerciseType.Mobility,           Icon = FA.PersonArmsOut, Label = l["TypeMobility"] },
        ];
    }

    // ── Filtros ───────────────────────────────────────────────────────

    private async void ApplyFilters()
    {
        IEnumerable<ExerciseRow> result = _allExercises;

        if (_selectedGroupId.HasValue)
        {
            if (!_groupExerciseCache.TryGetValue(_selectedGroupId.Value, out var ids))
            {
                ids = await muscleRepo.GetExerciseIdsByMuscleGroupAsync(_selectedGroupId.Value);
                _groupExerciseCache[_selectedGroupId.Value] = ids;
            }
            result = result.Where(r => ids.Contains(r.Exercise.Id));
        }

        if (_selectedTypeKeys.Count > 0)
            result = result.Where(r => _selectedTypeKeys.Contains(r.Exercise.Type));

        if (_filterBodyweight)
            result = result.Where(r =>
                r.Exercise.Type == ExerciseType.BodyweightStrength ||
                r.Exercise.IncludesBodyWeight);

        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            var q = _searchText.Trim().ToLowerInvariant();
            result = result.Where(r => r.Name.ToLowerInvariant().Contains(q));
        }

        _filteredCache = result.OrderBy(r => r.Name).ToList();
        _currentPage = 0;

        TotalCount = _filteredCache.Count;
        FilteredExercises = new ObservableCollection<ExerciseRow>(_filteredCache.Take(PageSize));
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(ActiveFilterCount));
    }

    // ── Commands ──────────────────────────────────────────────────────

    [RelayCommand]
    private void ToggleFilters() => IsFiltersExpanded = !IsFiltersExpanded;

    [RelayCommand]
    private void ClearSearch()
    {
        _searchText = "";
        OnPropertyChanged(nameof(SearchText));
        ApplyFilters();
    }

    [RelayCommand]
    private void SelectGroup(MuscleGroupChip group)
    {
        var newId = _selectedGroupId == group.Id ? null : (Guid?)group.Id;
        foreach (var g in MuscleGroups) g.IsSelected = g.Id == newId;

        var temp = MuscleGroups; MuscleGroups = []; MuscleGroups = temp;
        _selectedGroupId = newId;
        ApplyFilters();
    }

    [RelayCommand]
    private void ToggleTypeFilter(FilterChip chip)
    {
        chip.IsSelected = !chip.IsSelected;

        if (chip.Key is ExerciseType t)
        {
            if (chip.IsSelected) _selectedTypeKeys.Add(t);
            else _selectedTypeKeys.Remove(t);
        }

        var temp = ExerciseTypeFilters; ExerciseTypeFilters = []; ExerciseTypeFilters = temp;
        ApplyFilters();
    }

    [RelayCommand]
    private void ClearAllFilters()
    {
        _selectedGroupId = null;
        _selectedTypeKeys.Clear();
        FilterBodyweight = false;
        _searchText = "";
        OnPropertyChanged(nameof(SearchText));

        foreach (var g in MuscleGroups) g.IsSelected = false;
        foreach (var f in ExerciseTypeFilters) f.IsSelected = false;

        var tempG = MuscleGroups; MuscleGroups = []; MuscleGroups = tempG;
        var tempT = ExerciseTypeFilters; ExerciseTypeFilters = []; ExerciseTypeFilters = tempT;

        ApplyFilters();
    }

    [RelayCommand]
    private void LoadMore()
    {
        if (IsLoadingMore) return;
        var next = _filteredCache.Skip((_currentPage + 1) * PageSize).Take(PageSize).ToList();
        if (next.Count == 0) return;

        IsLoadingMore = true;
        _currentPage++;
        foreach (var item in next) FilteredExercises.Add(item);
        IsLoadingMore = false;
    }

    [RelayCommand]
    private async Task OpenDetail(ExerciseRow row)
        => await Shell.Current.GoToAsync($"///Exercises/Detail?exerciseId={row.Exercise.Id}");
}