using LevelUp.Mobile.Features.Plans.Models;
using LevelUp.Mobile.Features.Plans.ViewModels;

namespace LevelUp.Mobile.Features.Plans.Pages;

public partial class PlanEditPage : ContentPage
{
    public PlanEditPage(PlanEditViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PlanEditViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}