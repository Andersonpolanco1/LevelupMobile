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
    // El catálogo se cachea entre aperturas. Solo se recarga si el usuario
    // crea un ejercicio propio (funcionalidad pendiente).
    private bool _exercisePickerChanged = true;

    /// <summary>Llamar cuando el usuario crea un ejercicio propio.</summary>
    public void InvalidateExercisePicker() => _exercisePickerChanged = true;
    public bool ExercisePickerNeedsRefresh() => _exercisePickerChanged;
    public void ExercisePickerLoaded() => _exercisePickerChanged = false;

    // ── Exercises (tab de catálogo) ───────────────────────────────────
    // Mismo patrón que ExercisePicker — catálogo compartido.
    // Se invalida junto al picker cuando el usuario crea un ejercicio propio.
    private bool _exercisesChanged = true;

    /// <summary>
    /// Llamar cuando el usuario crea o edita un ejercicio propio,
    /// de modo que el tab Exercises recargue el catálogo en su próxima visita.
    /// </summary>
    public void InvalidateExercises()
    {
        _exercisesChanged = true;
        // Invalidar también el picker — comparten el mismo catálogo
        InvalidateExercisePicker();
    }

    public bool ExercisesNeedsRefresh() => _exercisesChanged;
    public void ExercisesLoaded() => _exercisesChanged = false;
}