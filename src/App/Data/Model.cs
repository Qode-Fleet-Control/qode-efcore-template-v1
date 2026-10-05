namespace App.Data;

public class Blog
{
    public int BlogId { get; set; }
    public required string Url { get; set; }
    public List<Post> Posts { get; } = [];
}

public class Post
{
    public int PostId { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public DateTime PublishedAt { get; set; }

    public int BlogId { get; set; }
    public Blog Blog { get; set; } = null!;
}
