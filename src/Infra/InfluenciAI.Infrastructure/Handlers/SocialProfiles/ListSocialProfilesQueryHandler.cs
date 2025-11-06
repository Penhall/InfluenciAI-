using InfluenciAI.Application.Common.Interfaces;
using InfluenciAI.Application.SocialProfiles.DTOs;
using InfluenciAI.Application.SocialProfiles.Queries;
using InfluenciAI.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InfluenciAI.Infrastructure.Handlers.SocialProfiles;

public class ListSocialProfilesQueryHandler : IRequestHandler<ListSocialProfilesQuery, List<SocialProfileDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ListSocialProfilesQueryHandler(
        ApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<SocialProfileDto>> Handle(ListSocialProfilesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var tenantId = _currentUserService.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var profiles = await _context.SocialProfiles
            .Where(sp => sp.TenantId == tenantId && sp.UserId == userId && sp.IsActive)
            .OrderByDescending(sp => sp.ConnectedAt)
            .Select(sp => new SocialProfileDto(
                sp.Id,
                sp.TenantId,
                sp.UserId,
                sp.Network,
                sp.ProfileId,
                sp.Username,
                sp.DisplayName,
                sp.ProfileImageUrl,
                sp.IsActive,
                sp.ConnectedAt,
                sp.LastSyncAt
            ))
            .ToListAsync(cancellationToken);

        return profiles;
    }
}
