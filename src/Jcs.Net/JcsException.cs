using System.Text.Json;

namespace Jcs.Net;

/// <summary>
/// Thrown when input cannot be canonicalized under RFC 8785: invalid JSON,
/// numbers outside the IEEE 754 double range (NaN, Infinity), duplicate
/// object member names, or unpaired UTF-16 surrogates in string data.
/// </summary>
public sealed class JcsException : JsonException
{
    /// <summary>Initializes the exception with a message describing the violation.</summary>
    /// <param name="message">Description of the RFC 8785 constraint that was violated.</param>
    public JcsException(string message) : base(message)
    {
    }

    /// <summary>Initializes the exception with a message and the underlying failure.</summary>
    /// <param name="message">Description of the RFC 8785 constraint that was violated.</param>
    /// <param name="innerException">The original exception raised by the JSON reader.</param>
    public JcsException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
