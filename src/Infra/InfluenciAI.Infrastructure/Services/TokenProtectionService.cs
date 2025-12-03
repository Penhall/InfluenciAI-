using InfluenciAI.Application.Common.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace InfluenciAI.Infrastructure.Services;

/// <summary>
/// Implementation of token encryption/decryption using ASP.NET Core Data Protection API
/// </summary>
public class TokenProtectionService : ITokenProtectionService
{
    private readonly IDataProtector _protector;

    public TokenProtectionService(IDataProtectionProvider dataProtectionProvider)
    {
        // Create a protector with a specific purpose string
        // This ensures tokens are only decryptable by this specific purpose
        _protector = dataProtectionProvider.CreateProtector("InfluenciAI.SocialProfileTokens.v1");
    }

    public string Protect(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return token;

        return _protector.Protect(token);
    }

    public string Unprotect(string encryptedToken)
    {
        if (string.IsNullOrWhiteSpace(encryptedToken))
            return encryptedToken;

        return _protector.Unprotect(encryptedToken);
    }
}
