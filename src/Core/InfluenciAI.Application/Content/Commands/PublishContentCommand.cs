using InfluenciAI.Application.Content.DTOs;
using MediatR;

namespace InfluenciAI.Application.Content.Commands;

public record PublishContentCommand(
    Guid ContentId,
    Guid SocialProfileId
) : IRequest<PublicationDto>;
