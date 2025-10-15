using FluentAssertions;
using InfluenciAI.Application.Tenants;
using InfluenciAI.Domain.Entities;
using Moq;
using Xunit;

namespace InfluenciAI.Tests.Tenants;

public class CreateTenantHandlerTests
{
    [Fact]
    public async Task Creates_tenant_and_persists()
    {
        var repo = new Mock<ITenantRepository>();
        repo.Setup(r => r.AddAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant t, CancellationToken _) => t);
        repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreateTenantHandler(repo.Object);
        var dto = await handler.Handle(new CreateTenantCommand("Acme"), CancellationToken.None);

        dto.Id.Should().NotBeEmpty();
        dto.Name.Should().Be("Acme");
        repo.Verify(r => r.AddAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

