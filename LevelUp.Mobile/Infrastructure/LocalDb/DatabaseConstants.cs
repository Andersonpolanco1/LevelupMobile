namespace LevelUp.Mobile.Infrastructure.LocalDb
{
    public static class DatabaseConstants
    {
        public const string DatabaseFilename = "levelup.db";

        public const SQLite.SQLiteOpenFlags Flags =
            SQLite.SQLiteOpenFlags.ReadWrite |
            SQLite.SQLiteOpenFlags.Create |
            SQLite.SQLiteOpenFlags.SharedCache;

        public static string DatabasePath =>
            Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
    }
}
