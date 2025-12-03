using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InfluenciAI.Application.Common.Interfaces;
using InfluenciAI.Application.SocialProfiles.Queries;
using InfluenciAI.Application.SocialProfiles.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InfluenciAI.Application.SocialProfiles.Handlers;

public class ListSocialProfilesQueryHandler : IRequestHandler<ListSocialProfilesQuery, List<SocialProfileDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ListSocialProfilesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<SocialProfileDto>> Handle(ListSocialProfilesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");
        var tenantId = _currentUserService.TenantId ?? throw new UnauthorizedAccessException("Tenant not found");

        var profiles = await _context.SocialProfiles
            .Where(p => p.TenantId == tenantId && p.UserId == userId.ToString())
            .OrderBy(p => p.DisplayName)
            .Select(p => new SocialProfileDto(
                p.Id,
                p.TenantId,
                p.UserId,
                p.Network,
                p.ProfileId,
                p.Username,
                p.DisplayName,
                p.ProfileImageUrl,
                p.IsActive,
                p.ConnectedAt,
                p.LastSyncAt,
                p.TokenExpiresAt
            ))
            .ToListAsync(cancellationToken);

        return profiles;
    }
}
