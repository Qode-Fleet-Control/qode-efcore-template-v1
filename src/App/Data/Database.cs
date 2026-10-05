using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace App.Data;

/// <summary>Picks the provider: PostgreSQL when DATABASE_URL is set, else SQLite.</summary>
public static class Database
{
    public static BloggingContext Open()
    {
        var url = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrWhiteSpace(url))
        {
            return new PostgresBloggingContext(new DbContextOptionsBuilder<PostgresBloggingContext>()
                .UseNpgsql(NpgsqlConnectionString(url)).Options);
        }
        return new SqliteBloggingContext(new DbContextOptionsBuilder<SqliteBloggingContext>()
            .UseSqlite(SqliteConnectionString()).Options);
    }

    /// <summary>SQLITE_PATH, default data/app.db (relative to the working directory).</summary>
    public static string SqliteConnectionString()
    {
        var path = Environment.GetEnvironmentVariable("SQLITE_PATH");
        if (string.IsNullOrWhiteSpace(path)) path = Path.Combine("data", "app.db");
        var dir = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        return $"Data Source={path}";
    }

    /// <summary>
    /// Accepts a URL (postgres://user:pass@host:5432/db?sslmode=require), which is what the
    /// fleet injects, or an Npgsql key=value connection string as is.
    /// </summary>
    public static string NpgsqlConnectionString(string url)
    {
        if (!url.Contains("://")) return url;
        var uri = new Uri(url);
        var userInfo = uri.UserInfo.Split(':', 2);
        var csb = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort || uri.Port <= 0 ? 5432 : uri.Port,
            Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
            Username = Uri.UnescapeDataString(userInfo[0]),
        };
        if (userInfo.Length > 1) csb.Password = Uri.UnescapeDataString(userInfo[1]);
        foreach (var pair in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = pair.Split('=', 2);
            if (kv[0].Equals("sslmode", StringComparison.OrdinalIgnoreCase) && kv.Length == 2
                && Enum.TryParse<SslMode>(kv[1].Replace("-", ""), ignoreCase: true, out var mode))
            {
                csb.SslMode = mode;
            }
        }
        return csb.ConnectionString;
    }
}
