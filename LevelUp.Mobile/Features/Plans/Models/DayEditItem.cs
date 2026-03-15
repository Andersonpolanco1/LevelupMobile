// Features/Plans/Models/DayEditItem.cs
using CommunityToolkit.Mvvm.ComponentModel;

namespace LevelUp.Mobile.Features.Plans.Models;

public partial class DayEditItem : ObservableObject
{
    public DayOfWeek DayOfWeek { get; init; }
    public string DisplayName { get; init; } = "";

    [ObservableProperty] private bool _isEnabled;
    [ObservableProperty] private bool _isExpandedNotes;
    [ObservableProperty] private string? _notes;
    [ObservableProperty] private int _exerciseCount;

    public Guid? ExistingDayId { get; set; }
}