namespace LevelUp.Mobile.Features.Exercises.Models
{
    public partial class MuscleGroupChip : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
