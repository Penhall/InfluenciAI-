using InfluenciAI.Application.Common.Interfaces;
using InfluenciAI.Application.SocialProfiles.Commands;
using InfluenciAI.Application.SocialProfiles.DTOs;
using InfluenciAI.Domain.Entities;
using InfluenciAI.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InfluenciAI.Infrastructure.Handlers.SocialProfiles;

public class ConnectSocialProfileHandler : IRequestHandler<ConnectSocialProfileCommand, SocialProfileDto>
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITwitterService _twitterService;

    public ConnectSocialProfileHandler(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        ITwitterService twitterService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _twitterService = twitterService;
    }

    public async Task<SocialProfileDto> Handle(ConnectSocialProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var tenantId = _currentUserService.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        // Get profile info from Twitter API
        var profileInfo = await _twitterService.GetProfileInfoAsync(request.AccessToken);

        // Check if profile already exists
        var existing = await _context.SocialProfiles
            .FirstOrDefaultAsync(sp =>
                sp.TenantId == tenantId &&
                sp.UserId == userId &&
                sp.Network == request.Network &&
                sp.ProfileId == profileInfo.ProfileId,
                cancellationToken);

        if (existing != null)
        {
            // Update existing profile
            existing.AccessToken = request.AccessToken;
            existing.RefreshToken = request.RefreshToken ?? string.Empty;
            existing.TokenExpiresAt = request.TokenExpiresAt;
            existing.Username = profileInfo.Username;
            existing.DisplayName = profileInfo.DisplayName;
            existing.ProfileImageUrl = profileInfo.ProfileImageUrl;
            existing.IsActive = true;
            existing.LastSyncAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return MapToDto(existing);
        }

        // Create new profile
        var socialProfile = new SocialProfile
        {
            TenantId = tenantId,
            UserId = userId,
            Network = request.Network,
            ProfileId = profileInfo.ProfileId,
            Username = profileInfo.Username,
            DisplayName = profileInfo.DisplayName,
            ProfileImageUrl = profileInfo.ProfileImageUrl,
            AccessToken = request.AccessToken,
            RefreshToken = request.RefreshToken ?? string.Empty,
            TokenExpiresAt = request.TokenExpiresAt,
            IsActive = true,
            ConnectedAt = DateTime.UtcNow,
            LastSyncAt = DateTime.UtcNow
        };

        _context.SocialProfiles.Add(socialProfile);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(socialProfile);
    }

    private static SocialProfileDto MapToDto(SocialProfile profile)
    {
        return new SocialProfileDto(
            profile.Id,
            profile.TenantId,
            profile.UserId,
            profile.Network,
            profile.ProfileId,
            profile.Username,
            profile.DisplayName,
            profile.ProfileImageUrl,
            profile.IsActive,
            profile.ConnectedAt,
            profile.LastSyncAt
        );
    }
}
