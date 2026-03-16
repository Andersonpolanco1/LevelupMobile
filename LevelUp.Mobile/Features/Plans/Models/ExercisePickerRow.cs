using CommunityToolkit.Mvvm.ComponentModel;
using LevelUp.Mobile.Core.Entities;

namespace LevelUp.Mobile.Features.Plans.Models;

public partial class ExercisePickerRow : ObservableObject
{
    public Exercise Exercise { get; init; } = null!;
    public string Name { get; init; } = "";
    public string? ImageUrl => Exercise.ImageUrl;

    /// <summary>
    /// True cuando este ejercicio ya está en el día (check visible).
    /// </summary>
    [ObservableProperty]
    private bool _isSelected;
}