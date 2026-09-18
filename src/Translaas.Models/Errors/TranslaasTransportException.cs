using System;

namespace Translaas.Models.Errors;

/// <summary>
/// Exception thrown when an HTTP transport failure occurs (connect, TLS, DNS, or name resolution).
/// This is not an HTTP status from the Translaas API — do not treat it as a 4xx/5xx response.
/// </summary>
public sealed class TranslaasTransportException : TranslaasException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TranslaasTransportException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public TranslaasTransportException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslaasTransportException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception (typically <c>HttpRequestException</c>).</param>
    public TranslaasTransportException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
