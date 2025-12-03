using System;
using System.Threading;
using System.Threading.Tasks;
using InfluenciAI.Application.Common.Interfaces;
using InfluenciAI.Application.SocialProfiles.Commands;
using InfluenciAI.Application.SocialProfiles.DTOs;
using InfluenciAI.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InfluenciAI.Application.SocialProfiles.Handlers;

public class ConnectSocialProfileHandler : IRequestHandler<ConnectSocialProfileCommand, SocialProfileDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITwitterService _twitterService;
    private readonly ICurrentUserService _currentUser;
    private readonly ITokenProtectionService _tokenProtection;

    public ConnectSocialProfileHandler(
        IApplicationDbContext context,
        ITwitterService twitterService,
        ICurrentUserService currentUserService,
        ITokenProtectionService tokenProtection)
    {
        _context = context;
        _twitterService = twitterService;
        _currentUser = currentUserService;
        _tokenProtection = tokenProtection;
    }

    public async Task<SocialProfileDto> Handle(ConnectSocialProfileCommand request, CancellationToken ct)
    {
        if (_currentUser.TenantId == null || _currentUser.UserId == null)
        {
            throw new UnauthorizedAccessException();
        }
        var userId = _currentUser.UserId.Value.ToString();

        // 1. Obter info do perfil da API do Twitter
        var twitterProfile = await _twitterService.GetUserProfileAsync(request.AccessToken);

        // 2. Verificar se já existe conexão
        var existing = await _context.SocialProfiles
            .FirstOrDefaultAsync(sp =>
                sp.Network == request.Network &&
                sp.ProfileId == twitterProfile.Id &&
                sp.TenantId == _currentUser.TenantId &&
                sp.UserId == userId, ct);

        if (existing != null)
        {
            // Atualizar tokens
            existing.AccessToken = EncryptToken(request.AccessToken);
            if (request.RefreshToken is not null)
                existing.RefreshToken = EncryptToken(request.RefreshToken);
            existing.TokenExpiresAt = request.TokenExpiresAt;
            existing.IsActive = true;
        }
        else
        {
            // Criar nova conexão
            existing = new SocialProfile
            {
                TenantId = _currentUser.TenantId.Value,
                UserId = _currentUser.UserId.Value.ToString(),
                Network = request.Network,
                ProfileId = twitterProfile.Id,
                Username = twitterProfile.Username,
                DisplayName = twitterProfile.Name,
                ProfileImageUrl = twitterProfile.ProfileImageUrl,
                AccessToken = EncryptToken(request.AccessToken),
                RefreshToken = request.RefreshToken is not null ? EncryptToken(request.RefreshToken) : string.Empty,
                TokenExpiresAt = request.TokenExpiresAt,
                IsActive = true,
                ConnectedAt = DateTime.UtcNow
            };
            _context.SocialProfiles.Add(existing);
        }

        await _context.SaveChangesAsync(ct);
        return MapToDto(existing);
    }

    private string EncryptToken(string token)
    {
        return _tokenProtection.Protect(token);
    }

    private SocialProfileDto MapToDto(SocialProfile profile)
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
            profile.LastSyncAt,
            profile.TokenExpiresAt
        );
    }
}
