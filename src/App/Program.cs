using App.Data;
using Microsoft.EntityFrameworkCore;

// A console job: apply pending migrations, seed one blog the first time, then report.
// Exits 0 on success; any exception exits non-zero.
await using var db = Database.Open();
Console.WriteLine($"provider: {db.Database.ProviderName}");

var pending = (await db.Database.GetPendingMigrationsAsync()).ToList();
Console.WriteLine(pending.Count == 0 ? "migrations: up to date" : $"migrations: applying {string.Join(", ", pending)}");
await db.Database.MigrateAsync();

const string url = "https://example.com/fleet";
var blog = await db.Blogs.Include(b => b.Posts).SingleOrDefaultAsync(b => b.Url == url);
if (blog is null)
{
    blog = new Blog { Url = url };
    db.Blogs.Add(blog);
}
blog.Posts.Add(new Post
{
    Title = $"Run at {DateTime.UtcNow:O}",
    Content = "Written by the EF Core template job.",
    PublishedAt = DateTime.UtcNow,
});
await db.SaveChangesAsync();

var summary = await db.Blogs
    .Select(b => new { b.Url, Posts = b.Posts.Count, Latest = b.Posts.Max(p => (DateTime?)p.PublishedAt) })
    .ToListAsync();
foreach (var row in summary)
{
    Console.WriteLine($"blog {row.Url}: {row.Posts} post(s), latest {row.Latest:O}");
}
Console.WriteLine("done");
