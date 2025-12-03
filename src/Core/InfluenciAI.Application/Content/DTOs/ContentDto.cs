using InfluenciAI.Domain.Entities;

namespace InfluenciAI.Application.Content.DTOs;

public record ContentDto(
    Guid Id,
    Guid TenantId,
    string UserId,
    string Title,
    string Body,
    ContentType Type,
    ContentStatus Status,
    DateTime? ScheduledFor,
    DateTime? PublishedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
