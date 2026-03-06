using LevelUp.Mobile.Features.Plans.ViewModels;

namespace LevelUp.Mobile.Features.Plans.Pages;

public partial class CreatePlanPage : ContentPage
{
    public CreatePlanPage(CreatePlanViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}