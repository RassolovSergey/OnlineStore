using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Moq;
using OnlineStore.Infrastructure.Persistence;

namespace OnlineStore.Auth.Tests;

/// <summary>
/// Вспомогалки для юнит-тестов Auth.
/// </summary>
public static class TestHelpers
{
    // Создаём DbContext на SQLite In-Memory, чтобы работали транзакции.
    public static (AppDbContext Db, SqliteConnection Conn) CreateSqliteDb(bool ensureCreated = false)
    {
        var conn = new SqliteConnection("DataSource=:memory:");
        conn.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(conn)
            .EnableSensitiveDataLogging()
            .Options;

        var db = new AppDbContext(options);

        // Нам это не нужно для unit-тестов; оставим опционально
        if (ensureCreated)
            db.Database.EnsureCreated();

        return (db, conn);
    }


    // Поддельное окружение (для различий Dev/Prod в настройках cookie).
    public static IHostEnvironment FakeEnv(bool isProduction = false)
    {
        var env = new Mock<IHostEnvironment>();
        env.SetupGet(e => e.EnvironmentName).Returns(isProduction ? "Production" : "Development");
        return env.Object;
    }


    // HttpContext для контроллеров (чтобы писать/читать cookie/заголовки).
    public static DefaultHttpContext HttpWithCookies() => new DefaultHttpContext();
}
