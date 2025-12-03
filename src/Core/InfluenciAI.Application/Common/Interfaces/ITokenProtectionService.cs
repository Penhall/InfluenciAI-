namespace InfluenciAI.Application.Common.Interfaces;

/// <summary>
/// Service for encrypting and decrypting sensitive tokens (OAuth tokens, API keys, etc.)
/// </summary>
public interface ITokenProtectionService
{
    /// <summary>
    /// Encrypts a token for secure storage
    /// </summary>
    string Protect(string token);

    /// <summary>
    /// Decrypts a previously encrypted token
    /// </summary>
    string Unprotect(string encryptedToken);
}
