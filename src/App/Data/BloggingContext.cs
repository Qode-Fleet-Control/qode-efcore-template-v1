using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace App.Data;

/// <summary>
/// The model. It is shared by one context subclass PER PROVIDER, each with its own
/// migrations (Migrations/Sqlite, Migrations/Postgres) — the "separate DbContext type per
/// provider" approach from EF Core's "Migrations with Multiple Providers" docs. Migrations
/// carry provider-specific SQL (identity columns, types), so one set cannot serve both.
/// </summary>
public abstract class BloggingContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Blog> Blogs => Set<Blog>();
    public DbSet<Post> Posts => Set<Post>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>(b =>
        {
            b.Property(x => x.Url).HasMaxLength(500);
            b.HasIndex(x => x.Url).IsUnique();
        });
        modelBuilder.Entity<Post>(p => p.Property(x => x.Title).HasMaxLength(200));
    }
}

/// <summary>Local default: a SQLite file.</summary>
public sealed class SqliteBloggingContext(DbContextOptions<SqliteBloggingContext> options)
    : BloggingContext(options);

/// <summary>Used when DATABASE_URL is set: PostgreSQL via Npgsql.</summary>
public sealed class PostgresBloggingContext(DbContextOptions<PostgresBloggingContext> options)
    : BloggingContext(options);

// Design-time factories: `dotnet ef migrations add X --context <Context>` builds the
// context through these. Adding a migration never connects to the database.
public sealed class SqliteDesignTimeFactory : IDesignTimeDbContextFactory<SqliteBloggingContext>
{
    public SqliteBloggingContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<SqliteBloggingContext>()
            .UseSqlite(Database.SqliteConnectionString()).Options);
}

public sealed class PostgresDesignTimeFactory : IDesignTimeDbContextFactory<PostgresBloggingContext>
{
    public PostgresBloggingContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<PostgresBloggingContext>()
            .UseNpgsql("Host=localhost;Database=app;Username=app;Password=app").Options);
}
