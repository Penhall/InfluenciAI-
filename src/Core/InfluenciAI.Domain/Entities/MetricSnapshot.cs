namespace InfluenciAI.Domain.Entities;

public class MetricSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PublicationId { get; set; }
    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
    public int Views { get; set; }
    public int Likes { get; set; }
    public int Shares { get; set; } // Retweets/Reposts/Shares
    public int Comments { get; set; } // Replies/Comments
    public decimal EngagementRate { get; set; }
    public int Impressions { get; set; } // If available from API
    public int Clicks { get; set; } // Link clicks if applicable
}
