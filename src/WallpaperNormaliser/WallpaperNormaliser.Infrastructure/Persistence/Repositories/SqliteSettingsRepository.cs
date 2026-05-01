using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Models.Common;
using WallpaperNormaliser.Core.Models.Settings;
using WallpaperNormaliser.Infrastructure.Persistence.Database;

namespace WallpaperNormaliser.Infrastructure.Persistence.Repositories;
public sealed class SqliteSettingsRepository : ISettingsRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteSettingsRepository(SqliteConnectionFactory connectionFactory) 
        => _connectionFactory = connectionFactory;

    public async Task<AppSettings> GetAsync(CancellationToken cancellationToken = default)
    {
        using var db = _connectionFactory.Create();

        string query = "SELECT [Key], [Value] FROM [AppSettings]";

        var rows = await db.QueryAsync<(string Key, string Value)>(query);

        if (!(rows?.Any() ?? true))
            return AppSettings.Default;

        var dict = rows!.ToDictionary(x=>x.Key, x=>x.Value);

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
                                    res ?? @default.DefaultResolution,
                                    qualityParseResult ? quality : @default.DefaultJpegQuality,
                                    scan ?? @default.ScanSettings,
                                    cache ?? @default.CacheSettings,
                                    logging ?? @default.LoggingSettings
                                );

        return result;
    }

    private Resolution? ParseResolution(string input)   //TEMPORARY!!!
    {
        if(!input.Contains('x'))
            return null;
        
        string[] inputChunks = input.Split('x', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        bool parseResult = Int32.TryParse(inputChunks[0], out int width) & Int32.TryParse(inputChunks[1], out int height);

        if (!parseResult)
            return null;

        return new Resolution(width, height);
    }

    private ScanSettings? ParseScanSettings(string input)   //TEMPORARY!!!
    {
        if(!input.Contains(';'))
            return null;

        string[] inputChunks = input.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        bool parseResult = Boolean.TryParse(inputChunks[0], out bool isRecursive) & 
                           Boolean.TryParse(inputChunks[1], out bool isWatchEnabled) & 
                           Int32.TryParse(inputChunks[2], out int debounce);

        if(!parseResult)
            return null;

        return new ScanSettings(isRecursive, isWatchEnabled, debounce);
    }

    private CacheSettings? ParseCacheSettings(string input) //TEMPORARY!!!
    {
        if (!input.Contains(';'))
            return null;

        string[] inputChunks = input.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        bool parseResult = Boolean.TryParse(inputChunks[0], out bool isEnabled) &
                       Int32.TryParse(inputChunks[1], out int maxitems) &
                       Int32.TryParse(inputChunks[2], out int expirationminutes);

        if (!parseResult)
            return null;

        return new CacheSettings(isEnabled, maxitems, expirationminutes);
    }

    private LoggingSettings? ParseLoggingSettings(string input) //TEMPORARY!!!
    {
        if (!input.Contains(';'))
            return null;

        string[] inputChunks = input.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        bool parseResult = Boolean.TryParse(inputChunks[0], out bool isFileLoggingEnabled) &
                           Boolean.TryParse(inputChunks[1], out bool isDatabaseLoggingEnabled) &
                           Int32.TryParse(inputChunks[2], out int retentionDays) &
                           Int32.TryParse(inputChunks[3], out int maxRows);

        if (!parseResult)
            return null;

        return new LoggingSettings(isFileLoggingEnabled, isDatabaseLoggingEnabled, retentionDays, maxRows);
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        using var db = _connectionFactory.Create();
        using var transaction = db.BeginTransaction();

        string deleteScript = "DELETE FROM [AppSettings]";

        await db.ExecuteAsync(deleteScript, transaction).ConfigureAwait(false);

        string insertScript = "INSERT INTO [AppSettings] ([Key], [Value], [UpdatedUtc]) VALUES (@Key, @Value, @UpdatedUtc)";
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
        result.Add(resolutionKey, $"{input.DefaultResolution.Width}x{input.DefaultResolution.Height}");
        result.Add(jpegQualityKey, $"{input.DefaultJpegQuality}");
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
        using var db = _connectionFactory.Create();

        string queryString = "SELECT COUNT(*) FROM [AppSettings]";
        
        long count = await db.ExecuteScalarAsync<long>(queryString, cancellationToken);
        bool result = count > 0;
        return result;
    }
}
