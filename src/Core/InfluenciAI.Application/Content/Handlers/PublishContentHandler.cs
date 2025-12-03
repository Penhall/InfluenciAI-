using System;
using System.Threading;
using System.Threading.Tasks;
using InfluenciAI.Application.Common.Interfaces;
using InfluenciAI.Application.Content.Commands;
using InfluenciAI.Application.Content.DTOs;
using InfluenciAI.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InfluenciAI.Application.Content.Handlers;

public class PublishContentHandler : IRequestHandler<PublishContentCommand, PublicationDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITwitterService _twitterService;
    private readonly ITokenProtectionService _tokenProtection;

    public PublishContentHandler(
        IApplicationDbContext context,
        ITwitterService twitterService,
        ITokenProtectionService tokenProtection)
    {
        _context = context;
        _twitterService = twitterService;
        _tokenProtection = tokenProtection;
    }

    public async Task<PublicationDto> Handle(PublishContentCommand request, CancellationToken ct)
    {
        // 1. Obter conteúdo e perfil
        var content = await _context.Contents.FindAsync(request.ContentId);
        var profile = await _context.SocialProfiles.FindAsync(request.SocialProfileId);

        if (content == null || profile == null)
            throw new Exception("Content or Social Profile not found.");

        // 2. Criar registro de publicação
        var publication = new Publication
        {
            ContentId = content.Id,
            SocialProfileId = profile.Id,
            Status = PublicationStatus.Publishing
        };
        _context.Publications.Add(publication);
        await _context.SaveChangesAsync(ct);

        try
        {
            // 3. Publicar no Twitter
            var accessToken = DecryptToken(profile.AccessToken);
            var tweetResult = await _twitterService.PublishTweetAsync(accessToken, content.Body);

            // 4. Atualizar com sucesso
            publication.Status = PublicationStatus.Published;
            publication.ExternalId = tweetResult.Id;
            publication.ExternalUrl = tweetResult.Url;
            publication.PublishedAt = DateTime.UtcNow;

            content.Status = ContentStatus.Published;
            content.PublishedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            // 5. Atualizar com erro
            publication.Status = PublicationStatus.Failed;
            publication.ErrorMessage = ex.Message;
            content.Status = ContentStatus.Failed;
        }

        await _context.SaveChangesAsync(ct);
        return MapToDto(publication);
    }

    private string DecryptToken(string encryptedToken)
    {
        return _tokenProtection.Unprotect(encryptedToken);
    }

    private PublicationDto MapToDto(Publication publication)
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
