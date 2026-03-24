// Services/AppState.cs
namespace LevelUp.Mobile.Services;

/// <summary>
/// Estado global compartido entre ViewModels.
/// Singleton — vive durante toda la sesión.
/// Centraliza las decisiones de recarga de cada pantalla.
/// </summary>
public class AppState
{
    // ── Home ──────────────────────────────────────────────────────────
    private DateTime? _homeLastLoaded;
    private bool _homePlanChanged = true;

    public void InvalidateHome() => _homePlanChanged = true;

    public bool HomeNeedsRefresh()
    {
        if (_homeLastLoaded is null) return true;
        if (_homePlanChanged) return true;
        if (_homeLastLoaded.Value.Date != DateTime.Today) return true;
        return false;
    }

    public void HomeLoaded()
    {
        _homeLastLoaded = DateTime.Now;
        _homePlanChanged = false;
    }

    // ── Plans (lista principal) ───────────────────────────────────────
    private bool _plansChanged = true;

    /// <summary>Llamar cuando se crea, elimina o activa un plan.</summary>
    public void InvalidatePlans() => _plansChanged = true;
    public bool PlansNeedsRefresh() => _plansChanged;
    public void PlansLoaded() => _plansChanged = false;

    // ── ExercisePicker ────────────────────────────────────────────────
    private bool _exercisePickerChanged = true;

    /// <summary>Llamar cuando el usuario crea un ejercicio propio.</summary>
    public void InvalidateExercisePicker() => _exercisePickerChanged = true;
    public bool ExercisePickerNeedsRefresh() => _exercisePickerChanged;
    public void ExercisePickerLoaded() => _exercisePickerChanged = false;

    // ── Exercises (tab de catálogo) ───────────────────────────────────
    private bool _exercisesChanged = true;

    /// <summary>
    /// Llamar cuando el usuario crea o edita un ejercicio propio.
    /// Invalida también el picker — comparten el mismo catálogo.
    /// </summary>
    public void InvalidateExercises()
    {
        _exercisesChanged = true;
        InvalidateExercisePicker();
    }

    public bool ExercisesNeedsRefresh() => _exercisesChanged;
    public void ExercisesLoaded() => _exercisesChanged = false;

    // ── Invalidación global por cambio de idioma ──────────────────────

    /// <summary>
    /// Llamar desde ProfileViewModel.SaveAsync() cuando el idioma cambia.
    /// Invalida todos los ViewModels que resuelven strings localizados
    /// en memoria (labels de chips, nombres de músculos, tipo de ejercicio, etc.).
    /// <br/><br/>
    /// Los bindings XAML con {local:Localize} se actualizan solos vía
    /// LocalizationService.PropertyChanged — solo necesitan invalidación
    /// los ViewModels que cachean strings resueltos como propiedades C#.
    /// </summary>
    public void InvalidateAllOnLanguageChange()
    {
        // Exercises: recarga catálogo + labels de tipo en chips y cards
        _exercisesChanged = true;

        // ExercisePicker: mismos labels de tipo
        _exercisePickerChanged = true;

        // Home: puede tener el nombre del día o plan cacheado
        _homePlanChanged = true;

        // Plans: nombres de días si están resueltos en el ViewModel
        _plansChanged = true;

        // _homeLastLoaded se conserva para no perder la lógica de "nuevo día"
    }
}