using InfluenciAI.Domain.Entities;

namespace InfluenciAI.Application.Content.DTOs;

public record PublicationDto(
    Guid Id,
    Guid ContentId,
    Guid SocialProfileId,
    string? ExternalId,
    string? ExternalUrl,
    PublicationStatus Status,
    DateTime? PublishedAt,
    string? ErrorMessage
);
