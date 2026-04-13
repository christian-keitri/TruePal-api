using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TruePal.Api.Data;

namespace TruePal.Api.Tests.Helpers;

public class TestDbContext : IDisposable
{
    private readonly SqliteConnection _connection;

    public AppDbContext Context { get; }

    public TestDbContext()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new AppDbContext(options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
