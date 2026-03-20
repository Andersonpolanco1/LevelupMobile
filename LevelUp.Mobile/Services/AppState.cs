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
    private bool _homePlanChanged = true; // true = carga al inicio

    public void InvalidateHome() => _homePlanChanged = true;

    public bool HomeNeedsRefresh()
    {
        if (_homeLastLoaded is null) return true; // nunca cargó
        if (_homePlanChanged) return true; // plan cambió
        if (_homeLastLoaded.Value.Date != DateTime.Today) return true; // nuevo día
        return false;
    }

    public void HomeLoaded()
    {
        _homeLastLoaded = DateTime.Now;
        _homePlanChanged = false;
    }

    // ── Plans (lista principal) ───────────────────────────────────────

    private bool _plansChanged = true; // true = carga al inicio

    /// <summary>Llamar cuando se crea, elimina o activa un plan.</summary>
    public void InvalidatePlans() => _plansChanged = true;

    public bool PlansNeedsRefresh() => _plansChanged;

    public void PlansLoaded() => _plansChanged = false;

    // ── ExercisePicker ────────────────────────────────────────────────
    // El catálogo de 100+ ejercicios se cachea entre aperturas.
    // Solo se recarga si se agregó un ejercicio de usuario
    // (funcionalidad pendiente — por ahora solo carga una vez por sesión).

    private bool _exercisePickerChanged = true; // true = carga al inicio

    /// <summary>
    /// Llamar cuando el usuario crea un ejercicio propio.
    /// Fuerza recarga del catálogo en el próximo uso del picker.
    /// </summary>
    public void InvalidateExercisePicker() => _exercisePickerChanged = true;

    public bool ExercisePickerNeedsRefresh() => _exercisePickerChanged;

    public void ExercisePickerLoaded() => _exercisePickerChanged = false;
}