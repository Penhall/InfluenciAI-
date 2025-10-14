using MediatR;

namespace InfluenciAI.Application.Tenants;

public sealed record UpdateTenantCommand(Guid Id, string Name) : IRequest<TenantDto>;
public sealed record DeleteTenantCommand(Guid Id) : IRequest<bool>;

public sealed class UpdateTenantHandler(ITenantRepository repo) : IRequestHandler<UpdateTenantCommand, TenantDto>
{
    public async Task<TenantDto> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var entity = await repo.GetByIdForUpdateAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Tenant not found");
        entity.Name = request.Name.Trim();
        await repo.SaveChangesAsync(cancellationToken);
        return new TenantDto { Id = entity.Id, Name = entity.Name, CreatedAtUtc = entity.CreatedAtUtc };
    }
}

public sealed class DeleteTenantHandler(ITenantRepository repo) : IRequestHandler<DeleteTenantCommand, bool>
{
    public async Task<bool> Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        var entity = await repo.GetByIdForUpdateAsync(request.Id, cancellationToken);
        if (entity is null) return false;
        repo.Remove(entity);
        await repo.SaveChangesAsync(cancellationToken);
        return true;
    }
}

