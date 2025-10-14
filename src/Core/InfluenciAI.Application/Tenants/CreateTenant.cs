using InfluenciAI.Domain.Entities;
using MediatR;

namespace InfluenciAI.Application.Tenants;

public sealed record CreateTenantCommand(string Name) : IRequest<TenantDto>;

public sealed class CreateTenantHandler(ITenantRepository repo) : IRequestHandler<CreateTenantCommand, TenantDto>
{
    public async Task<TenantDto> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required", nameof(request.Name));

        var entity = new Tenant
        {
            Name = request.Name.Trim()
        };

        await repo.AddAsync(entity, cancellationToken);
        await repo.SaveChangesAsync(cancellationToken);

        return new TenantDto
        {
            Id = entity.Id,
            Name = entity.Name,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }
}

