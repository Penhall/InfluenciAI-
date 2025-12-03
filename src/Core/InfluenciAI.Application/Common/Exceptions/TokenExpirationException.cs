using System;

namespace InfluenciAI.Application.Common.Exceptions;

public class TokenExpirationException : Exception
{
    public TokenExpirationException(string message) : base(message)
    {
    }

    public TokenExpirationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
