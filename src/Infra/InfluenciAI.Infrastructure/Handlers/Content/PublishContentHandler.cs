using InfluenciAI.Application.Common.Interfaces;
using InfluenciAI.Application.Content.Commands;
using InfluenciAI.Application.Content.DTOs;
using InfluenciAI.Domain.Entities;
using InfluenciAI.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InfluenciAI.Infrastructure.Handlers.Content;

public class PublishContentHandler : IRequestHandler<PublishContentCommand, PublicationDto>
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITwitterService _twitterService;

    public PublishContentHandler(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        ITwitterService twitterService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _twitterService = twitterService;
    }

    public async Task<PublicationDto> Handle(PublishContentCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var tenantId = _currentUserService.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        // Get content
        var content = await _context.Contents
            .FirstOrDefaultAsync(c =>
                c.Id == request.ContentId &&
                c.TenantId == tenantId &&
                c.UserId == userId,
                cancellationToken);

        if (content == null)
            throw new InvalidOperationException("Content not found");

        // Get social profile
        var socialProfile = await _context.SocialProfiles
            .FirstOrDefaultAsync(sp =>
                sp.Id == request.SocialProfileId &&
                sp.TenantId == tenantId &&
                sp.UserId == userId &&
                sp.IsActive,
                cancellationToken);

        if (socialProfile == null)
            throw new InvalidOperationException("Social profile not found");

        // Create publication record
        var publication = new Publication
        {
            TenantId = tenantId,
            UserId = userId,
            ContentId = content.Id,
            SocialProfileId = socialProfile.Id,
            Status = PublicationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Publications.Add(publication);
        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            // Publish to Twitter
            var result = await _twitterService.PublishTweetAsync(
                socialProfile.AccessToken,
                content.Body
            );

            // Update publication with success
            publication.ExternalId = result.TweetId;
            publication.ExternalUrl = result.TweetUrl;
            publication.Status = PublicationStatus.Published;
            publication.PublishedAt = DateTime.UtcNow;

            // Update content status
            content.Status = ContentStatus.Published;
            content.PublishedAt = DateTime.UtcNow;
            content.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Update publication with error
            publication.Status = PublicationStatus.Failed;
            publication.ErrorMessage = ex.Message;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return MapToDto(publication);
    }

    private static PublicationDto MapToDto(Publication publication)
    {
        return new PublicationDto(
            publication.Id,
            publication.ContentId,
            publication.SocialProfileId,
            publication.ExternalId,
            publication.ExternalUrl,
            publication.Status,
            publication.PublishedAt,
            publication.ErrorMessage
        );
    }
}
