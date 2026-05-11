using System.Data;
using System.Text.Json;

using Dapper;

using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Common;
using WallpaperNormaliser.Core.Models.Settings;
using WallpaperNormaliser.Infrastructure.Persistence.Database;

namespace WallpaperNormaliser.Infrastructure.Persistence.Repositories;
public sealed class SqliteSettingsRepository(SqliteConnectionFactory connectionFactory) : ISettingsRepository
{
    private readonly SqliteConnectionFactory _connectionFactory = connectionFactory;

    public async Task<AppSettings> GetAsync(CancellationToken cancellationToken = default)
    {
        using IDbConnection db = _connectionFactory.Create();

        const string query = "SELECT [Key], [Value] FROM [AppSettings]";

        IEnumerable<(string Key, string Value)> rows = await db.QueryAsync<(string Key, string Value)>(query);

        if (!(rows?.Any() ?? true))
            return AppSettings.Default;

        Dictionary<string, string> dict = rows!.ToDictionary(x=>x.Key, x=>x.Value);

        AppSettings result = FromDictionary(dict);  //TEMPORARY!!!

        return result;
    }

    private AppSettings FromDictionary(Dictionary<string, string> input)    //TEMPORARY!!!
    {
        AppSettings @default = AppSettings.Default;

        const string rootDirectoryKey = "AppSettings_RootDirectory";
        const string resolutionKey = "AppSettings_Resolution";
        const string jpegQualityKey = "AppSettings_jpegQuality";
        const string scanSettingsKey = "AppSettings_Scan";
        const string cacheSettingsKey = "AppSettings_Cache";
        const string loggingSettingsKey = "AppSettings_Logging";

        string? rootDirectoryValue = input[rootDirectoryKey];
        string? resolutionValue = input[resolutionKey];
        string? jpegQualityValue = input[jpegQualityKey];
        string? scanSettingsValue = input[scanSettingsKey];
        string? cacheSettingsValue = input[cacheSettingsKey];
        string? loggingSettingsValue = input[loggingSettingsKey];

        Resolution? res = ParseResolution(resolutionValue);
        bool qualityParseResult = Int32.TryParse(jpegQualityValue, out int quality);
        ScanSettings? scan = ParseScanSettings(scanSettingsValue);
        CacheSettings? cache = ParseCacheSettings(cacheSettingsValue);
        LoggingSettings? logging = ParseLoggingSettings(loggingSettingsValue);

        AppSettings result = new(
                                    !String.IsNullOrEmpty(rootDirectoryValue) ? rootDirectoryValue : @default.RootDirectory,
                                    res ?? @default.Resolution,
                                    qualityParseResult ? quality : @default.Quality,
                                    scan ?? @default.ScanSettings,
                                    cache ?? @default.CacheSettings,
                                    logging ?? @default.LoggingSettings
                                );

        return result;
    }

    private Resolution? ParseResolution(string input)   //TEMPORARY!!!
    {
        const char separator = 'x';
        const StringSplitOptions options = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;
        Resolution? result = null;

        if (!input.Contains(separator))
            return result;
        
        string[] inputChunks = input.Split(separator, options);
        bool parseResult = UInt32.TryParse(inputChunks[0], out uint width) & 
                           UInt32.TryParse(inputChunks[1], out uint height);

        if (parseResult)
            result = new Resolution(width, height);

        return result;
    }

    private ScanSettings? ParseScanSettings(string input)   //TEMPORARY!!!
    {
        const char separator = ';';
        const StringSplitOptions options = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;
        ScanSettings? result = null;

        if (!input.Contains(separator))
            return result;

        string[] inputChunks = input.Split(separator, options);

        bool parseResult = Boolean.TryParse(inputChunks[0], out bool isRecursive) & 
                           Boolean.TryParse(inputChunks[1], out bool isWatchEnabled) & 
                           Int32.TryParse(inputChunks[2], out int debounce);

        if (parseResult)
            result = new ScanSettings(isRecursive, isWatchEnabled, debounce);

        return result;
    }

    private CacheSettings? ParseCacheSettings(string input) //TEMPORARY!!!
    {
        const char separator = ';';
        const StringSplitOptions options = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;
        CacheSettings? result = null;

        if (!input.Contains(separator))
            return result;

        string[] inputChunks = input.Split(separator, options);

        bool parseResult = Boolean.TryParse(inputChunks[0], out bool isEnabled) &
                           Int32.TryParse(inputChunks[1], out int maxitems) &
                           Int32.TryParse(inputChunks[2], out int expirationminutes);

        if (parseResult)
            result = new CacheSettings(isEnabled, maxitems, expirationminutes);

        return result;
    }

    private LoggingSettings? ParseLoggingSettings(string input) //TEMPORARY!!!
    {
        const char separator = ';';
        const StringSplitOptions options = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;
        LoggingSettings? result = null;

        if (!input.Contains(separator))
            return result;

        string[] inputChunks = input.Split(separator, options);

        bool parseResult = Boolean.TryParse(inputChunks[0], out bool isFileLoggingEnabled) &
                           Boolean.TryParse(inputChunks[1], out bool isDatabaseLoggingEnabled) &
                           Int32.TryParse(inputChunks[2], out int retentionDays) &
                           Int32.TryParse(inputChunks[3], out int maxRows);

        if (parseResult)
            result = new LoggingSettings(isFileLoggingEnabled, isDatabaseLoggingEnabled, retentionDays, maxRows);

        return result;
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        using IDbConnection db = _connectionFactory.Create();
        using IDbTransaction transaction = db.BeginTransaction();

        const string deleteScript = "DELETE FROM [AppSettings]";

        await db.ExecuteAsync(deleteScript, transaction).ConfigureAwait(false);

        const string insertScript = "INSERT INTO [AppSettings] ([Key], [Value], [UpdatedUtc]) VALUES (@Key, @Value, @UpdatedUtc)";
        DateTime now = DateTime.UtcNow;
        var records = ToDictionary(settings).Select(x=>new { Key = x.Key, Value = x.Value, UpdatedUtc = now }); //TEMPORARY!!!

        await db.ExecuteAsync(insertScript, records, transaction).ConfigureAwait(false);

        transaction.Commit();
    }

    private Dictionary<string, string> ToDictionary(AppSettings input)  //TEMPORARY!!!
    {
        Dictionary<string,string> result = new();

        const string rootDirectoryKey = "AppSettings_RootDirectory";
        const string resolutionKey = "AppSettings_Resolution";
        const string jpegQualityKey = "AppSettings_jpegQuality";
        const string scanSettingsKey = "AppSettings_Scan";
        const string cacheSettingsKey = "AppSettings_Cache";
        const string loggingSettingsKey = "AppSettings_Logging";

        result.Add(rootDirectoryKey, input.RootDirectory);
        result.Add(resolutionKey, $"{input.Resolution.Width}x{input.Resolution.Height}");
        result.Add(jpegQualityKey, $"{input.Quality}");
        result.Add(scanSettingsKey, $"{input.ScanSettings.IsRecursive};{input.ScanSettings.IsWatchEnabled};{input.ScanSettings.DebounceMilliseconds}");
        result.Add(cacheSettingsKey, $"{input.CacheSettings.IsEnabled};{input.CacheSettings.MaxItems};{input.CacheSettings.ExpirationMinutes}");
        result.Add(loggingSettingsKey, $"{input.LoggingSettings.IsFileLoggingEnabled};{input.LoggingSettings.IsDatabaseLoggingEnabled};{input.LoggingSettings.RetentionDays};{input.LoggingSettings.MaxRows}");

        return result;
    }

    public async Task<string> ExportJsonAsync(CancellationToken cancellationToken = default) 
        => JsonSerializer.Serialize(
                                       await GetAsync(cancellationToken), 
                                       new JsonSerializerOptions { WriteIndented = true }
                                   );

    public async Task ImportJsonAsync(string json, CancellationToken cancellationToken = default)
        => await SaveAsync(
                              JsonSerializer.Deserialize<AppSettings>(json) ?? throw new InvalidOperationException("Invalid settings json"),
                              cancellationToken
                          );

    public async Task ResetToDefaultsAsync(CancellationToken cancellationToken = default) 
        => await SaveAsync(AppSettings.Default, cancellationToken);

    public async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
    {
        using IDbConnection db = _connectionFactory.Create();

        const string queryString = "SELECT COUNT(*) FROM [AppSettings]";
        
        long count = await db.ExecuteScalarAsync<long>(queryString, cancellationToken);
        bool result = count > 0;
        
        return result;
    }

    public async Task UpdateAsync(Func<AppSettings, AppSettings> updateDelegate, CancellationToken cancellationToken = default)
    {
        var current = await GetAsync(cancellationToken);
        var next = updateDelegate(current);
        await SaveAsync(next, cancellationToken);
    }
}
