using LevelUp.Mobile.Features.Exercises.ViewModels;

namespace LevelUp.Mobile.Features.Exercises.Pages;

public partial class ExerciseDetailPage : ContentPage
{
    public ExerciseDetailPage(ExerciseDetailViewModel vm)
    {
        BindingContext = vm;
        InitializeComponent();
    }
}