using LevelUp.Mobile.Features.Profile.ViewModels;

namespace LevelUp.Mobile.Features.Profile.Pages
{
    public partial class ProfilePage : ContentPage
    {
        private readonly ProfileViewModel _vm;

        public ProfilePage(ProfileViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            BindingContext = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _vm.LoadAsync();
        }
    }
}