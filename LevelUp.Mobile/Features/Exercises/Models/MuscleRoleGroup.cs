namespace LevelUp.Mobile.Features.Exercises.Models
{
    /// <summary>Agrupa músculos por rol (Primary / Secondary / Stabilizer).</summary>
    public class MuscleRoleGroup
    {
        public string RoleLabel { get; set; } = "";
        public string RoleIcon { get; set; } = "";
        public List<string> MuscleNames { get; set; } = [];
    }
}
