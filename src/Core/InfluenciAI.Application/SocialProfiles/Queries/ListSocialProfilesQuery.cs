using InfluenciAI.Application.SocialProfiles.DTOs;
using MediatR;

namespace InfluenciAI.Application.SocialProfiles.Queries;

public record ListSocialProfilesQuery : IRequest<List<SocialProfileDto>>;
