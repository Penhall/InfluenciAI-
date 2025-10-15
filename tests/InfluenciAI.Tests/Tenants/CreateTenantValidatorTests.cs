using FluentAssertions;
using InfluenciAI.Application.Tenants;
using Xunit;

namespace InfluenciAI.Tests.Tenants;

public class CreateTenantValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("A")]
    public void Invalid_names_fail_validation(string? name)
    {
        var validator = new CreateTenantValidator();
        var result = validator.Validate(new CreateTenantCommand(name ?? string.Empty));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Valid_name_passes()
    {
        var validator = new CreateTenantValidator();
        var result = validator.Validate(new CreateTenantCommand("Acme"));
        result.IsValid.Should().BeTrue();
    }
}

