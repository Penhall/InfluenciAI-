using InfluenciAI.Application.Content.DTOs;
using InfluenciAI.Application.Metrics.DTOs;
using InfluenciAI.Application.SocialProfiles.DTOs;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using InfluenciAI.Application.Common.Interfaces;
using InfluenciAI.Application.Content.Commands;
using InfluenciAI.Application.SocialProfiles.Commands;
using InfluenciAI.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace InfluenciAI.Tests.E2E;

public class TwitterEndToEndTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public TwitterEndToEndTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CompleteFlow_ShouldSucceed()
    {
        // Arrange
        var twitterServiceMock = new Mock<ITwitterService>();
        twitterServiceMock.Setup(x => x.GetUserProfileAsync(It.IsAny<string>()))
            .ReturnsAsync(new TwitterUserProfile("123", "testuser", "Test User", ""));
        twitterServiceMock.Setup(x => x.PublishTweetAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new TweetResult("456", "https://twitter.com/testuser/status/456"));
        twitterServiceMock.Setup(x => x.GetTweetMetricsAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new TweetMetrics(100, 10, 5, 2));

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => twitterServiceMock.Object);
            });
        }).CreateClient();

        var tenantId = Guid.NewGuid();
        var registerRequest = new
        {
            TenantId = tenantId,
            Email = "testuser@test.com",
            Password = "password"
        };

        // Act
        // 1. Register and Login (omitted for brevity, assuming a logged in user with a valid token)
        // For a real test, you would register a user, log in, and get a token.
        // For this example, we'll skip to connecting the social profile.

        // 2. Connect Social Profile
        var connectRequest = new ConnectSocialProfileCommand(SocialNetwork.Twitter, "test_access_token", "test_refresh_token", DateTime.UtcNow.AddHours(1));
        var connectResponse = await client.PostAsJsonAsync("/api/social-profiles", connectRequest);
        var connectedProfile = await connectResponse.Content.ReadFromJsonAsync<SocialProfileDto>();

        // 3. Create Content
        var createContentRequest = new CreateContentCommand("Test Title", "Test tweet body", ContentType.Text);
        var createContentResponse = await client.PostAsJsonAsync("/api/content", createContentRequest);
        var createdContent = await createContentResponse.Content.ReadFromJsonAsync<ContentDto>();

        // 4. Publish Content
        var publishRequest = new PublishContentRequest(connectedProfile.Id);
        var publishResponse = await client.PostAsJsonAsync($"/api/content/{createdContent.Id}/publish", publishRequest);
        var publishedContent = await publishResponse.Content.ReadFromJsonAsync<PublicationDto>();

        // 5. Get Metrics
        var metricsResponse = await client.GetAsync($"/api/content/{createdContent.Id}/metrics");
        var metrics = await metricsResponse.Content.ReadFromJsonAsync<ContentMetricsDto>();

        // Assert
        connectResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        connectedProfile.Should().NotBeNull();
        connectedProfile.ProfileId.Should().Be("123");

        createContentResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        createdContent.Should().NotBeNull();
        createdContent.Body.Should().Be("Test tweet body");

        publishResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        publishedContent.Should().NotBeNull();
        publishedContent.ExternalId.Should().Be("456");

        metricsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        metrics.Should().NotBeNull();
        metrics.Latest.Should().NotBeNull();
        metrics.Latest.Likes.Should().Be(10);
    }
}
