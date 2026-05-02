using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using Dapper;

namespace WallpaperNormaliser.Infrastructure.Persistence.Database;
public sealed class MigrationRunner
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public MigrationRunner(SqliteConnectionFactory connectionFactory) 
        => _connectionFactory = connectionFactory;

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        using var db = _connectionFactory.Create();

        var sql = await File.ReadAllTextAsync(MigrationConstants._relativeDbScriptPath_Pragmas, cancellationToken);

        await db.ExecuteAsync(sql);
    }
}

internal static class MigrationConstants
{
    internal static readonly string _relativeDbScriptPath_Pragmas = "Persistence/Sql/001_Initial.sql";
}
