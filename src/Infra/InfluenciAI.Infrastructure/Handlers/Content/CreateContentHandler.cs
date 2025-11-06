using InfluenciAI.Application.Common.Interfaces;
using InfluenciAI.Application.Content.Commands;
using InfluenciAI.Application.Content.DTOs;
using InfluenciAI.Domain.Entities;
using InfluenciAI.Infrastructure.Data;
using MediatR;

namespace InfluenciAI.Infrastructure.Handlers.Content;

public class CreateContentHandler : IRequestHandler<CreateContentCommand, ContentDto>
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateContentHandler(
        ApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ContentDto> Handle(CreateContentCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var tenantId = _currentUserService.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var content = new Domain.Entities.Content
        {
            TenantId = tenantId,
            UserId = userId,
            Title = request.Title,
            Body = request.Body,
            Type = request.Type,
            Status = ContentStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Contents.Add(content);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(content);
    }

    private static ContentDto MapToDto(Domain.Entities.Content content)
    {
        return new ContentDto(
            content.Id,
            content.TenantId,
            content.UserId,
            content.Title,
            content.Body,
            content.Type,
            content.Status,
            content.ScheduledFor,
            content.PublishedAt,
            content.CreatedAt,
            content.UpdatedAt
        );
    }
}
