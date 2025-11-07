namespace InfluenciAI.Domain.Entities;

public class Publication
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public Guid ContentId { get; set; }
    public Guid SocialProfileId { get; set; }
    public string ExternalId { get; set; } = string.Empty; // ID from social network
    public string ExternalUrl { get; set; } = string.Empty; // URL to view post
    public PublicationStatus Status { get; set; } = PublicationStatus.Pending;
    public DateTime? PublishedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Content? Content { get; set; }
    public SocialProfile? SocialProfile { get; set; }
    public ICollection<MetricSnapshot> MetricSnapshots { get; set; } = new List<MetricSnapshot>();
}

public enum PublicationStatus
{
    Pending = 1,
    Publishing = 2,
    Published = 3,
    Failed = 4
}
