using LevelUp.Mobile.Core.Enums;
using SQLite;

namespace LevelUp.Mobile.Core.Entities
{
    public class Exercise : ILocalEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsSynced { get; set; }
        public bool IsDeleted { get; set; }


        public bool IncludesBodyWeight { get; set; }

        public double BodyWeightFactor { get; set; }

        public string? ImageUrl { get; set; }

        public ExerciseType Type { get; set; }

        public Guid? CreatorId { get; set; }
    }
}
