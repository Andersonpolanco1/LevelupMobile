using CommunityToolkit.Mvvm.ComponentModel;

namespace LevelUp.Mobile.Features.Exercises.Models
{
    public partial class FilterChip : ObservableObject
    {
        /// <summary>Valor tipado del filtro: ExerciseType, MuscleRole, etc.</summary>
        public object? Key { get; set; }
        public string Icon { get; set; } = "";
        public string Label { get; set; } = "";

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
