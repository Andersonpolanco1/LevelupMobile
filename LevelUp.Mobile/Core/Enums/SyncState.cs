namespace LevelUp.Mobile.Core.Enums
{
    public class SyncState
    {
        public string EntityName { get; set; } = "";  // "Catalog", "UserData"

        public DateTime LastSync { get; set; }
    }
}
