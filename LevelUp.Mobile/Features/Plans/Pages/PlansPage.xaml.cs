using LevelUp.Mobile.Features.Plans.ViewModels;

namespace LevelUp.Mobile.Features.Plans.Pages;

public partial class PlansPage : ContentPage
{
    private readonly PlansViewModel _vm;

    public PlansPage(PlansViewModel vm)
    {
        BindingContext = vm;
        _vm = vm;
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_vm.LoadPlansCommand.CanExecute(null))
            _vm.LoadPlansCommand.Execute(null);
    }
}