using LevelUp.Mobile.Features.Plans.ViewModels;

namespace LevelUp.Mobile.Features.Plans.Pages;

public partial class ExercisePickerPage : ContentPage
{
    public ExercisePickerPage(ExercisePickerViewModel vm)
    {
        try
        {
            InitializeComponent();
            BindingContext = vm;
            System.Diagnostics.Debug.WriteLine(">>> ExercisePickerPage: created OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($">>> ExercisePickerPage CRASH: {ex.Message}");
            throw;
        }
    }
}