using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WallpaperNormaliser.Infrastructure.Persistence.Database;

namespace WallpaperNormaliser.Infrastructure.Persistence.Repositories;
public sealed class SqliteRunRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteRunRepository(SqliteConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task CreateRunAsync(ProcessingRun run, CancellationToken cancellationToken = default)
    {
        using var db = _connectionFactory.Create();
        string insertScript = "";

        await db.ExecuteAsync(insertScript, run);
    }

    public async Task AddRunItemAsync(ProcessingRunItem item, CancellationToken cancellationToken = default)
    {
        using var db = _connectionFactory.Create();
        string insertScript = "";

        await db.ExecuteAsync(insertScript, item);
    }

    public async Task FinaliseRunAsync()

}
