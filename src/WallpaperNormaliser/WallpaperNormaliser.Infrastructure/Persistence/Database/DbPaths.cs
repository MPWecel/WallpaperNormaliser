namespace WallpaperNormaliser.Infrastructure.Persistence.Database;
public sealed class DbPaths
{
    private readonly string _databaseDirectoryName = "db";
    private readonly string _databaseFileName = "wallpaper-normaliser.db";

    public string DatabaseDirectory => Path.Combine(AppContext.BaseDirectory, _databaseDirectoryName);
    public string DatabaseFilePath => Path.Combine(DatabaseDirectory, _databaseFileName);
    public string ConnectionString => $"Data Source={DatabaseFilePath}";

    public void EnsureDatabaseDirectoryExists()
    {
        if (!Directory.Exists(DatabaseDirectory))
            Directory.CreateDirectory(DatabaseDirectory);
    }
}
