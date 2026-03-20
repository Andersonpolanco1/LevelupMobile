// Features/Plans/ViewModels/ExercisePickerViewModel.cs
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
    WeeklyPlanService planService,
    AppState appState) : ObservableObject
{
    // ── Query property ────────────────────────────────────────────────

    private Guid _dayId;

    public string DayId
    {
        get => _dayId.ToString();
        set
        {
            if (!Guid.TryParse(value, out var id)) return;
            _dayId = id;
            _ = LoadIfNeededAsync();
        }
    }

    // ── State ─────────────────────────────────────────────────────────

    [ObservableProperty] private bool _isBusy;

    [ObservableProperty]
    private ObservableCollection<MuscleGroupRow> _muscleGroups = [];

    [ObservableProperty]
    private ObservableCollection<ExercisePickerRow> _filteredExercises = [];

    private int _selectedCount;
    public int SelectedCount
    {
        get => _selectedCount;
        private set => SetProperty(ref _selectedCount, value);
    }

    public bool IsEmpty => !IsBusy && FilteredExercises.Count == 0;

    // ── Filter state ──────────────────────────────────────────────────

    private string _searchText = "";
    private Guid? _selectedGroupId;

    public string SearchText
    {
        get => _searchText;
        set { _searchText = value; ApplyFilters(); }
    }

    public MuscleGroupRow? SelectedGroup
        => _muscleGroups.FirstOrDefault(g => g.Id == _selectedGroupId);

    // ── Cache interno ─────────────────────────────────────────────────

    private List<ExercisePickerRow> _allExercises = [];
    private Dictionary<Guid, HashSet<Guid>> _groupExerciseCache = [];
    private HashSet<Guid> _preExistingIds = [];

    /// <summary>Día para el que se cargaron las selecciones.</summary>
    private Guid _loadedDayId;

    // ── Carga inteligente ─────────────────────────────────────────────

    private async Task LoadIfNeededAsync()
    {
        if (_dayId == Guid.Empty) return;

        // Limpiar filtros siempre al entrar — evita estado sucio entre aperturas
        ResetFilters();

        var catalogNeedsRefresh = appState.ExercisePickerNeedsRefresh();
        var selectionsNeedRefresh = _loadedDayId != _dayId;

        if (catalogNeedsRefresh)
        {
            // Recarga completa: catálogo + grupos + selecciones
            await LoadAsync();
        }
        else if (selectionsNeedRefresh)
        {
            // Solo actualizar checkmarks del nuevo día
            await MainThread.InvokeOnMainThreadAsync(() => IsBusy = true);
            await RefreshDaySelectionsAsync();
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                ApplyFilters();
                IsBusy = false;
            });
        }
        else
        {
            // Todo cacheado, mismo día — solo repoblar el CollectionView
            // (el ViewModel es Singleton pero la UI se reconstruyó al navegar)
            await MainThread.InvokeOnMainThreadAsync(ApplyFilters);
        }
    }

    /// <summary>
    /// Carga completa: catálogo + grupos musculares + selecciones del día.
    /// Solo se ejecuta cuando AppState.ExercisePickerNeedsRefresh() == true.
    /// </summary>
    private async Task LoadAsync()
    {
        if (IsBusy) return;

        await MainThread.InvokeOnMainThreadAsync(() => IsBusy = true);
        try
        {
            var language = AppPreferences.GetLanguage();

            // ── 1. Grupos musculares ──────────────────────────────────
            var groups = await muscleRepo.GetGroupsWithTranslationAsync(language);
            var groupRows = groups
                .Select(g => new MuscleGroupRow
                {
                    Id = g.Group.Id,
                    Name = g.Translation?.Name ?? g.Group.Id.ToString()
                })
                .OrderBy(g => g.Name)
                .ToList();

            // ── 2. Catálogo de ejercicios ─────────────────────────────
            var withTranslation = await exerciseRepo.GetWithTranslationAsync(language);
            _allExercises = withTranslation
                .Select(x => new ExercisePickerRow
                {
                    Exercise = x.Exercise,
                    Name = x.Translation?.Name ?? x.Exercise.Id.ToString()
                })
                .OrderBy(x => x.Name)
                .ToList();

            // Limpiar cache de grupos al recargar el catálogo
            _groupExerciseCache.Clear();

            // ── 3. Selecciones del día actual ─────────────────────────
            await RefreshDaySelectionsAsync();

            // ── Actualizar UI en main thread ──────────────────────────
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                MuscleGroups = new ObservableCollection<MuscleGroupRow>(groupRows);
                ApplyFilters();
            });

            appState.ExercisePickerLoaded();
        }
        finally
        {
            await MainThread.InvokeOnMainThreadAsync(() => IsBusy = false);
        }
    }

    /// <summary>
    /// Operación liviana — solo actualiza checkmarks del día actual.
    /// No toca el catálogo ni los grupos musculares.
    /// </summary>
    private async Task RefreshDaySelectionsAsync()
    {
        var existingInDay = await planService.GetExercisesForDayAsync(_dayId);
        _preExistingIds = existingInDay.Select(e => e.ExerciseId).ToHashSet();

        foreach (var row in _allExercises)
            row.IsSelected = _preExistingIds.Contains(row.Exercise.Id);

        SelectedCount = _allExercises.Count(r => r.IsSelected);
        _loadedDayId = _dayId;
    }

    // ── Reset de filtros ──────────────────────────────────────────────

    /// <summary>
    /// Limpia texto de búsqueda, grupo seleccionado y chips visuales.
    /// Fuerza refresco del BindableLayout para que los DataTrigger
    /// del borde naranja se re-evalúen con IsSelected = false.
    /// </summary>
    private void ResetFilters()
    {
        _searchText = "";
        _selectedGroupId = null;
        OnPropertyChanged(nameof(SearchText));
        OnPropertyChanged(nameof(SelectedGroup));

        // Desmarcar todos los chips
        foreach (var g in _muscleGroups)
            g.IsSelected = false;

        // Forzar re-render del BindableLayout — mismo patrón que SelectGroup
        // Sin este swap los DataTrigger no re-evalúan aunque IsSelected cambie
        var temp = MuscleGroups;
        MuscleGroups = [];
        MuscleGroups = temp;
    }

    // ── Filtros ───────────────────────────────────────────────────────

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

        result = result
            .OrderByDescending(r => r.IsSelected)
            .ThenBy(r => r.Name);

        FilteredExercises = new ObservableCollection<ExercisePickerRow>(result);
        OnPropertyChanged(nameof(IsEmpty));
    }

    // ── Commands ──────────────────────────────────────────────────────

    [RelayCommand]
    private void SelectGroup(MuscleGroupRow group)
    {
        var newId = _selectedGroupId == group.Id ? null : (Guid?)group.Id;

        foreach (var g in MuscleGroups)
            g.IsSelected = g.Id == newId;

        // Forzar refresco del BindableLayout de chips
        var temp = MuscleGroups;
        MuscleGroups = [];
        MuscleGroups = temp;

        _selectedGroupId = newId;
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

        // Sincronizar con _allExercises si el item viene del filtro
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

        // Ejercicios del día cambiaron → invalidar Home y Plans
        // NO invalidar ExercisePicker — el catálogo no cambió
        appState.InvalidateHome();
        appState.InvalidatePlans();

        SelectedCount = _allExercises.Count(r => r.IsSelected);
    }

    [RelayCommand]
    private async Task GoBack()
        => await Shell.Current.GoToAsync("..");
}