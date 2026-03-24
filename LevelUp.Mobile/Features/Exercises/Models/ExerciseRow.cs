using CommunityToolkit.Mvvm.ComponentModel;
using LevelUp.Mobile.Core.Entities;

namespace LevelUp.Mobile.Features.Exercises.Models
{
    public partial class ExerciseRow : ObservableObject
    {
        public required Exercise Exercise { get; init; }
        public string Name { get; set; } = "";
        public string PrimaryMuscles { get; set; } = "";
        public string? ImageUrl { get; set; }
        public bool HasImage { get; set; }
        /// <summary>FontAwesome glyph según ExerciseType.</summary>
        public string TypeIcon { get; set; } = "";
        /// <summary>Color hex semántico del badge (naranja, azul, verde, etc.).</summary>
        public string TypeBadgeColor { get; set; } = "#FF6600";
    }
}
