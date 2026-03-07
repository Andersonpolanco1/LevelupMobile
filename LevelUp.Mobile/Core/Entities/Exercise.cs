using LevelUp.Mobile.Core.Enums;

namespace LevelUp.Mobile.Core.Entities
{
    public class Exercise : LocalEntity
    {
        public bool IncludesBodyWeight { get; set; }

        public double BodyWeightFactor { get; set; }

        public string? ImageUrl { get; set; }

        public ExerciseType Type { get; set; }

        public Guid? CreatorId { get; set; }
    }
}
