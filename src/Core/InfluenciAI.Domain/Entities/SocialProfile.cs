namespace InfluenciAI.Domain.Entities;

public class SocialProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public SocialNetwork Network { get; set; }
    public string ProfileId { get; set; } = string.Empty; // External ID from social network
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string ProfileImageUrl { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty; // TODO: Encrypt
    public string RefreshToken { get; set; } = string.Empty; // TODO: Encrypt
    public DateTime TokenExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastSyncAt { get; set; }
}

public enum SocialNetwork
{
    Twitter = 1,
    Instagram = 2,
    LinkedIn = 3,
    TikTok = 4,
    YouTube = 5
}
