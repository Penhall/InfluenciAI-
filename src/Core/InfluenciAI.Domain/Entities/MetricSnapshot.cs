namespace InfluenciAI.Domain.Entities;

public class MetricSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PublicationId { get; set; }
    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
    public int Views { get; set; }
    public int Likes { get; set; }
    public int Retweets { get; set; }
    public int Replies { get; set; }
    public decimal EngagementRate { get; set; }

    // Relacionamento
    public Publication Publication { get; set; }
}
