using InfluenciAI.Domain.Entities;

namespace InfluenciAI.Application.SocialProfiles.DTOs;

public record SocialProfileDto(
    Guid Id,
    Guid TenantId,
    Guid UserId,
    SocialNetwork Network,
    string ProfileId,
    string Username,
    string DisplayName,
    string ProfileImageUrl,
    bool IsActive,
    DateTime ConnectedAt,
    DateTime? LastSyncAt
);
