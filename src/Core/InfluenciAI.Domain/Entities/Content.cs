namespace InfluenciAI.Domain.Entities;

public class Content
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public ContentType Type { get; set; } = ContentType.Text;
    public ContentStatus Status { get; set; } = ContentStatus.Draft;
    public DateTime? ScheduledFor { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum ContentType
{
    Text = 1,
    Image = 2,
    Video = 3,
    Link = 4
}

public enum ContentStatus
{
    Draft = 1,
    Scheduled = 2,
    Publishing = 3,
    Published = 4,
    Failed = 5
}
