using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PortalCopa26.Data;

namespace PortalCopa26.Tests.Data;

/// <summary>
/// D10: SQLite in-memory com a conexão mantida aberta, para que as constraints
/// reais (FK, índice único, CHECK) sejam aplicadas — ao contrário do provider
/// InMemory do EF.
/// </summary>
public sealed class SqliteInMemoryFixture : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteInMemoryFixture()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        return new AppDbContext(options);
    }

    public void Dispose() => _connection.Dispose();
}
