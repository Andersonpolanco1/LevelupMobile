using System;
using System.Collections.Generic;
using System.Text;

namespace LevelUp.Mobile.Services;

/// <summary>
/// Estado global compartido entre ViewModels.
/// Singleton — vive durante toda la sesión.
/// </summary>
public class AppState
{
    // ── Flags de invalidación ─────────────────────────────────────

    /// <summary>True cuando el plan activo cambió y Home debe recargarse.</summary>
    public bool HomePlanChanged { get; set; } = true; // true = carga al inicio

    /// <summary>Fecha de la última carga de Home. Detecta cambio de día.</summary>
    public DateTime? HomeLastLoaded { get; set; }

    // ── Métodos de invalidación ───────────────────────────────────

    /// <summary>Llamar cuando el usuario activa/desactiva un plan.</summary>
    public void InvalidateHome() => HomePlanChanged = true;

    // ── Lógica de decisión ────────────────────────────────────────

    /// <summary>
    /// Determina si Home debe recargar sus datos.
    /// </summary>
    public bool HomeNeedsRefresh()
    {
        // Nunca cargó
        if (HomeLastLoaded is null) return true;

        // El plan cambió
        if (HomePlanChanged) return true;

        // Cambió el día del calendario
        if (HomeLastLoaded.Value.Date != DateTime.Today) return true;

        return false;
    }

    /// <summary>Llamar cuando Home termina de cargar exitosamente.</summary>
    public void HomeLoaded()
    {
        HomeLastLoaded = DateTime.Now;
        HomePlanChanged = false;
    }
}