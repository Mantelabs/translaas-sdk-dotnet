using System.Net.Http;

using FluentAssertions;

using Translaas.Models.Errors;

namespace Translaas.Models.Tests.Errors;

public class TranslaasTransportExceptionTests
{
    [Fact]
    public void TranslaasTransportException_ShouldInheritFromTranslaasException()
    {
        var exception = new TranslaasTransportException("transport error");

        exception.Should().BeAssignableTo<TranslaasException>();
    }

    [Fact]
    public void TranslaasTransportException_ShouldNotBeAssignableToTranslaasApiException()
    {
        var exception = new TranslaasTransportException("transport error");

        exception.Should().NotBeAssignableTo<TranslaasApiException>();
    }

    [Fact]
    public void TranslaasTransportException_ShouldWrapHttpRequestExceptionAsInnerException()
    {
        var innerException = new HttpRequestException("connection refused");

        var exception = new TranslaasTransportException(
            "Failed to retrieve translation: connection refused",
            innerException);

        exception.Message.Should().Be("Failed to retrieve translation: connection refused");
        exception.InnerException.Should().Be(innerException);
        exception.InnerException.Should().BeOfType<HttpRequestException>();
    }

    [Fact]
    public void TranslaasTransportException_ShouldPreserveInnerHttpRequestException()
    {
        var innerException = new HttpRequestException("no such host");

        var exception = new TranslaasTransportException("transport error", innerException);

        exception.InnerException.Should().BeSameAs(innerException);
    }
}
