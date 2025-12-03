using InfluenciAI.Domain.Entities;
using System;

namespace InfluenciAI.Application.SocialProfiles.DTOs;

public record SocialProfileDto(
    Guid Id,
    Guid TenantId,
    string UserId,
    SocialNetwork Network,
    string ProfileId,
    string Username,
    string DisplayName,
    string ProfileImageUrl,
    bool IsActive,
    DateTime ConnectedAt,
    DateTime? LastSyncAt,
    DateTime TokenExpiresAt
);
