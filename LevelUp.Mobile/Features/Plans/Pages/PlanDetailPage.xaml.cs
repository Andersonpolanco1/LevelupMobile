// Features/Plans/Pages/PlanDetailPage.xaml.cs
using LevelUp.Mobile.Features.Plans.ViewModels;

namespace LevelUp.Mobile.Features.Plans.Pages;

public partial class PlanDetailPage : ContentPage
{
    public PlanDetailPage(PlanDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Recarga al volver de PlanEditPage o PlanDayDetailPage
        if (BindingContext is PlanDetailViewModel vm)
            vm.ReloadIfNeeded();
    }
}