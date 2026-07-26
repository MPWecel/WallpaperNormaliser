using System.Data;
using System.Text.Json;

using Dapper;

using WallpaperNormaliser.Core.Contracts;
using WallpaperNormaliser.Core.Exceptions;
using WallpaperNormaliser.Core.Models.Common;
using WallpaperNormaliser.Core.Models.Settings;
using WallpaperNormaliser.Infrastructure.Persistence.Database;

namespace WallpaperNormaliser.Infrastructure.Persistence.Repositories;
public sealed class SqliteSettingsRepository(
                                                SqliteConnectionFactory connectionFactory,
                                                ISettingsChangeNotifier changeNotifier,
                                                ISettingsValidator settingsValidator
                                            ) : ISettingsRepository
{
    private const string _keyRootDirectory = "AppSettings_RootDirectory";
    private const string _keyResolution = "AppSettings_Resolution";
    private const string _keyQuality = "AppSettings_Quality";
    private const string _keyScan = "AppSettings_Scan";
    private const string _keyCache = "AppSettings_Cache";
    private const string _keyLogging = "AppSettings_Logging";

    private readonly SqliteConnectionFactory _connectionFactory = connectionFactory;
    private readonly ISettingsChangeNotifier _changeNotifier = changeNotifier;
    private readonly ISettingsValidator _settingsValidator = settingsValidator;

    public async Task<AppSettings> GetAsync(CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConn = _connectionFactory.Create();

        const string query = """
                                SELECT [Key], [Value] 
                                FROM [AppSettings]
                             """;

        IEnumerable<(string Key, string Value)> rows = await dbConn.QueryAsync<(string Key, string Value)>(query);

        Dictionary<string, string> dict = rows.ToDictionary(x => x.Key, x => x.Value);

        AppSettings defaults = AppSettings.Default;

        AppSettings result = new(
                                    RootDirectory: ReadOrDefault(dict, _keyRootDirectory, defaults.RootDirectory),
                                    Resolution: ReadOrDefault(dict, _keyResolution, defaults.Resolution),
                                    Quality: ReadOrDefault(dict, _keyQuality, defaults.Quality),
                                    ScanSettings: ReadOrDefault(dict, _keyScan, defaults.ScanSettings),
                                    CacheSettings: ReadOrDefault(dict, _keyCache, defaults.CacheSettings),
                                    LoggingSettings: ReadOrDefault(dict, _keyLogging, defaults.LoggingSettings)
                                );
        return result;
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConn = _connectionFactory.Create();
        using IDbTransaction transaction = dbConn.BeginTransaction();

        DateTime now = DateTime.UtcNow;

        var rows = new[]
        {
            new { Key = _keyRootDirectory, Value = JsonSerializer.Serialize(settings.RootDirectory), UpdatedUtc = now },
            new { Key = _keyResolution, Value = JsonSerializer.Serialize(settings.Resolution), UpdatedUtc = now },
            new { Key = _keyQuality, Value = JsonSerializer.Serialize(settings.Quality), UpdatedUtc = now },
            new { Key = _keyScan, Value = JsonSerializer.Serialize(settings.ScanSettings), UpdatedUtc = now },
            new { Key = _keyCache, Value = JsonSerializer.Serialize(settings.CacheSettings), UpdatedUtc = now },
            new { Key = _keyLogging, Value = JsonSerializer.Serialize(settings.LoggingSettings), UpdatedUtc = now },
        };

        const string upsert = """
                                 INSERT INTO [AppSettings] ([Key], [Value], [UpdatedUtc])
                                 VALUES (@Key, @Value, @UpdatedUtc)
                                 ON CONFLICT([Key]) DO UPDATE SET
                                                                   [Value] = excluded.[Value],
                                                                   [UpdatedUtc] = excluded.[UpdatedUtc];
                              """;

        try
        {
            await dbConn.ExecuteAsync(upsert, rows, transaction);
            transaction.Commit();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            transaction.Rollback();
            throw new InvalidOperationException(
                $"[SqliteSettingsRepository.SaveAsync] Transaction failed. SQL: {upsert}",
                ex);
        }

        _changeNotifier.Notify(settings);
    }

    public async Task<string> ExportJsonAsync(CancellationToken cancellationToken = default)
        => JsonSerializer.Serialize(
                                       await GetAsync(cancellationToken),
                                       new JsonSerializerOptions { WriteIndented = true }
                                   );

    public async Task ImportJsonAsync(string json, CancellationToken cancellationToken = default)
    {
        AppSettings settings = JsonSerializer.Deserialize<AppSettings>(json)
            ?? throw new InvalidOperationException("Invalid settings json");

        SettingsValidationResult validation = _settingsValidator.Validate(settings);
        if (!validation.IsValid)
            throw new SettingsValidationException(validation.Errors);

        await SaveAsync(settings, cancellationToken);
    }

    public async Task ResetToDefaultsAsync(CancellationToken cancellationToken = default) => await SaveAsync(AppSettings.Default, cancellationToken);

    public async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConn = _connectionFactory.Create();

        const string queryString = "SELECT COUNT(*) FROM [AppSettings]";
        long count = await dbConn.ExecuteScalarAsync<long>(queryString, cancellationToken);
        bool result = count > 0;
        
        return result;
    }

    public async Task UpdateAsync(Func<AppSettings, AppSettings> updateDelegate, CancellationToken cancellationToken = default)
    {
        AppSettings current = await GetAsync(cancellationToken);
        AppSettings next = updateDelegate(current);
        await SaveAsync(next, cancellationToken);
    }

    private static T ReadOrDefault<T>(
                                         Dictionary<string, string> dict, 
                                         string key, 
                                         T fallback
                                     )
    {
        T result;

        bool isNotInDictionary = !dict.TryGetValue(key, out string? value) || 
                                 String.IsNullOrWhiteSpace(value);
        if (isNotInDictionary)
        {
            result = fallback;
        }
        else
        {
            try
            {
                T? parsed = JsonSerializer.Deserialize<T>(value!);
                result = parsed ?? fallback;
            }
            catch (JsonException)
            {
                result = fallback;
            }
        }

        return result;
    }
}
