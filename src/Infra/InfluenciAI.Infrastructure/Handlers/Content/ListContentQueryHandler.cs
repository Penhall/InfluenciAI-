using InfluenciAI.Application.Common.Interfaces;
using InfluenciAI.Application.Content.DTOs;
using InfluenciAI.Application.Content.Queries;
using InfluenciAI.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InfluenciAI.Infrastructure.Handlers.Content;

public class ListContentQueryHandler : IRequestHandler<ListContentQuery, List<ContentDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ListContentQueryHandler(
        ApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<ContentDto>> Handle(ListContentQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var tenantId = _currentUserService.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        var contents = await _context.Contents
            .Where(c => c.TenantId == tenantId && c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ContentDto(
                c.Id,
                c.TenantId,
                c.UserId,
                c.Title,
                c.Body,
                c.Type,
                c.Status,
                c.ScheduledFor,
                c.PublishedAt,
                c.CreatedAt,
                c.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        return contents;
    }
}
