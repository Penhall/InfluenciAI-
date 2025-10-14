using MediatR;

namespace InfluenciAI.Application.Tenants;

public sealed record GetTenantByIdQuery(Guid Id) : IRequest<TenantDto?>;
public sealed record ListTenantsQuery() : IRequest<List<TenantDto>>;

public sealed class GetTenantByIdHandler(ITenantRepository repo) : IRequestHandler<GetTenantByIdQuery, TenantDto?>
{
    public async Task<TenantDto?> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await repo.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null) return null;
        return new TenantDto { Id = entity.Id, Name = entity.Name, CreatedAtUtc = entity.CreatedAtUtc };
    }
}

public sealed class ListTenantsHandler(ITenantRepository repo) : IRequestHandler<ListTenantsQuery, List<TenantDto>>
{
    public async Task<List<TenantDto>> Handle(ListTenantsQuery request, CancellationToken cancellationToken)
    {
        var list = await repo.ListAsync(cancellationToken);
        return list
            .OrderBy(x => x.Name)
            .Select(x => new TenantDto { Id = x.Id, Name = x.Name, CreatedAtUtc = x.CreatedAtUtc })
            .ToList();
    }
}

