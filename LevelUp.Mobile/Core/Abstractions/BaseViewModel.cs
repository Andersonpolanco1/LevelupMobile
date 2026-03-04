using CommunityToolkit.Mvvm.ComponentModel;

namespace LevelUp.Mobile.Core.Abstractions
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        bool isBusy;
    }
}
