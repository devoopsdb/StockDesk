using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StockDesk.Data;

namespace StockDesk.Tests;

public static class TestDbContextFactory
{
    public static (StockDbContext Context, SqliteConnection Connection) CreateInMemoryDbContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<StockDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new StockDbContext(options);
        context.Database.EnsureCreated();

        return (context, connection);
    }
}
