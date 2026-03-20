// Features/Plans/Pages/PlanDetailPage.xaml.cs
using LevelUp.Mobile.Features.Plans.ViewModels;

namespace LevelUp.Mobile.Features.Plans.Pages;

public partial class PlanDetailPage : ContentPage
{
    private readonly PlanDetailViewModel _vm;

    public PlanDetailPage(PlanDetailViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.ReloadAsync();
    }
}