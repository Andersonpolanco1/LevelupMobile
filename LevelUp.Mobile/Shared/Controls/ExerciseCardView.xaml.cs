namespace LevelUp.Mobile.Shared.Controls;

public partial class ExerciseCardView : ContentView
{
    public ExerciseCardView()
    {
        InitializeComponent();
    }

    // ── Bindable Properties ───────────────────────────────────────────

    public static readonly BindableProperty ExerciseNameProperty =
        BindableProperty.Create(nameof(ExerciseName), typeof(string), typeof(ExerciseCardView));

    public static readonly BindableProperty ImageUrlProperty =
        BindableProperty.Create(nameof(ImageUrl), typeof(string), typeof(ExerciseCardView));

    public static readonly BindableProperty OrderProperty =
        BindableProperty.Create(nameof(Order), typeof(int), typeof(ExerciseCardView), 0);

    public static readonly BindableProperty ShowOrderProperty =
        BindableProperty.Create(nameof(ShowOrder), typeof(bool), typeof(ExerciseCardView), false);

    public static readonly BindableProperty IsSelectedProperty =
        BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(ExerciseCardView), false);

    public static readonly BindableProperty SetsPlannedProperty =
        BindableProperty.Create(nameof(SetsPlanned), typeof(int?), typeof(ExerciseCardView));

    public static readonly BindableProperty RepsPlannedProperty =
        BindableProperty.Create(nameof(RepsPlanned), typeof(int?), typeof(ExerciseCardView));

    public static readonly BindableProperty ShowSetsRepsProperty =
        BindableProperty.Create(nameof(ShowSetsReps), typeof(bool), typeof(ExerciseCardView), false);

    // ── Propiedades CLR ───────────────────────────────────────────────

    public string? ExerciseName
    {
        get => (string?)GetValue(ExerciseNameProperty);
        set => SetValue(ExerciseNameProperty, value);
    }

    public string? ImageUrl
    {
        get => (string?)GetValue(ImageUrlProperty);
        set => SetValue(ImageUrlProperty, value);
    }

    public int Order
    {
        get => (int)GetValue(OrderProperty);
        set => SetValue(OrderProperty, value);
    }

    public bool ShowOrder
    {
        get => (bool)GetValue(ShowOrderProperty);
        set => SetValue(ShowOrderProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public int? SetsPlanned
    {
        get => (int?)GetValue(SetsPlannedProperty);
        set => SetValue(SetsPlannedProperty, value);
    }

    public int? RepsPlanned
    {
        get => (int?)GetValue(RepsPlannedProperty);
        set => SetValue(RepsPlannedProperty, value);
    }

    public bool ShowSetsReps
    {
        get => (bool)GetValue(ShowSetsRepsProperty);
        set => SetValue(ShowSetsRepsProperty, value);
    }
}