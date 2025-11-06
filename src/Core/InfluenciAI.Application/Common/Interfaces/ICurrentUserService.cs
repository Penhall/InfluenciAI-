namespace InfluenciAI.Application.Common.Interfaces;

/// <summary>
/// Service to get the current authenticated user information
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user ID from the JWT token
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Gets the current tenant ID from the JWT token
    /// </summary>
    Guid? TenantId { get; }

    /// <summary>
    /// Gets the current username
    /// </summary>
    string? Username { get; }
}
