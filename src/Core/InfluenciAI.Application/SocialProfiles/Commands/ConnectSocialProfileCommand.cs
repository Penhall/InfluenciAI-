using InfluenciAI.Application.SocialProfiles.DTOs;
using InfluenciAI.Domain.Entities;
using MediatR;

namespace InfluenciAI.Application.SocialProfiles.Commands;

public record ConnectSocialProfileCommand(
    SocialNetwork Network,
    string AccessToken,
    string? RefreshToken,
    DateTime TokenExpiresAt
) : IRequest<SocialProfileDto>;
