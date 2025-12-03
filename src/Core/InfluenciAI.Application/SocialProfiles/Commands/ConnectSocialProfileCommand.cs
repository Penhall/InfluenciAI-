using System;
using InfluenciAI.Domain.Entities;
using MediatR;
using InfluenciAI.Application.SocialProfiles.DTOs;

namespace InfluenciAI.Application.SocialProfiles.Commands;

public record ConnectSocialProfileCommand(
    SocialNetwork Network,
    string AccessToken,
    string? RefreshToken,
    DateTime TokenExpiresAt
) : IRequest<SocialProfileDto>;