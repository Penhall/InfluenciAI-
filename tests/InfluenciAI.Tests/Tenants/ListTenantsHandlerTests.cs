using FluentAssertions;
using InfluenciAI.Application.Tenants;
using InfluenciAI.Domain.Entities;
using Moq;
using Xunit;

namespace InfluenciAI.Tests.Tenants;

public class ListTenantsHandlerTests
{
    [Fact]
    public async Task Returns_sorted_by_name()
    {
        var repo = new Mock<ITenantRepository>();
        repo.Setup(r => r.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tenant>
            {
                new() { Name = "Charlie" },
                new() { Name = "Alice" },
                new() { Name = "Bob" }
            });

        var handler = new ListTenantsHandler(repo.Object);
        var list = await handler.Handle(new ListTenantsQuery(), CancellationToken.None);

        list.Select(x => x.Name).Should().ContainInOrder("Alice", "Bob", "Charlie");
    }
}

