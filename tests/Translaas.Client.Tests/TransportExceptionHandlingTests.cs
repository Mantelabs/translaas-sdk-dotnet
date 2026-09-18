using System.Net.Http;

using FluentAssertions;
using Moq;
using Moq.Protected;

using Translaas.Models.Errors;
using Translaas.Models.Requests;

namespace Translaas.Client.Tests;

public class TransportExceptionHandlingTests
{
    private readonly TranslaasClientOptions _defaultOptions = new()
    {
        ApiKey = "test-api-key",
        BaseUrl = "https://api.test.com"
    };

    private static Mock<HttpMessageHandler> CreateHandlerThatThrows(HttpRequestException exception)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(exception);

        return handlerMock;
    }

    private TranslaasClient CreateClient(HttpRequestException exception)
    {
        var httpClient = new HttpClient(CreateHandlerThatThrows(exception).Object);
        return new TranslaasClient(httpClient, _defaultOptions);
    }

    private static void AssertTransportException(TranslaasTransportException exception, string expectedPrefix, HttpRequestException inner)
    {
        exception.Should().NotBeAssignableTo<TranslaasApiException>();
        exception.InnerException.Should().BeSameAs(inner);
        exception.Message.Should().Be($"{expectedPrefix}{inner.Message}");
    }

    [Fact]
    public async Task GetEntryAsync_ShouldThrowTranslaasTransportException_WhenHttpRequestExceptionOccurs()
    {
        var inner = new HttpRequestException("connection refused");
        var client = CreateClient(inner);

        var exception = await Assert.ThrowsAsync<TranslaasTransportException>(
            () => client.GetEntryAsync("ui", "greeting", "en"));

        AssertTransportException(exception, "Failed to retrieve translation: ", inner);
    }

    [Fact]
    public async Task GetEntryAsync_ShouldNotThrowTranslaasApiException_WhenTransportFails()
    {
        var client = CreateClient(new HttpRequestException("connection refused"));

        var exception = await Record.ExceptionAsync(
            () => client.GetEntryAsync("ui", "greeting", "en"));

        exception.Should().BeOfType<TranslaasTransportException>();
        exception.Should().NotBeAssignableTo<TranslaasApiException>();
    }

    [Theory]
    [InlineData("connection refused")]
    [InlineData("No such host is known")]
    [InlineData("The SSL connection could not be established")]
    public async Task GetEntryAsync_ShouldThrowTranslaasTransportException_ForConnectTlsAndDnsFailures(string message)
    {
        var inner = new HttpRequestException(message);
        var client = CreateClient(inner);

        var exception = await Assert.ThrowsAsync<TranslaasTransportException>(
            () => client.GetEntryAsync("ui", "greeting", "en"));

        AssertTransportException(exception, "Failed to retrieve translation: ", inner);
    }

    [Fact]
    public async Task GetGroupAsync_ShouldThrowTranslaasTransportException_WhenHttpRequestExceptionOccurs()
    {
        var inner = new HttpRequestException("connection refused");
        var client = CreateClient(inner);

        var exception = await Assert.ThrowsAsync<TranslaasTransportException>(
            () => client.GetGroupAsync("project", "ui", "en"));

        AssertTransportException(exception, "Failed to retrieve translation group: ", inner);
    }

    [Fact]
    public async Task GetProjectAsync_ShouldThrowTranslaasTransportException_WhenHttpRequestExceptionOccurs()
    {
        var inner = new HttpRequestException("connection refused");
        var client = CreateClient(inner);

        var exception = await Assert.ThrowsAsync<TranslaasTransportException>(
            () => client.GetProjectAsync("project", "en"));

        AssertTransportException(exception, "Failed to retrieve translation project: ", inner);
    }

    [Fact]
    public async Task GetProjectLocalesAsync_ShouldThrowTranslaasTransportException_WhenHttpRequestExceptionOccurs()
    {
        var inner = new HttpRequestException("connection refused");
        var client = CreateClient(inner);

        var exception = await Assert.ThrowsAsync<TranslaasTransportException>(
            () => client.GetProjectLocalesAsync("project"));

        AssertTransportException(exception, "Failed to retrieve project locales: ", inner);
    }

    [Fact]
    public async Task GetOfflineCacheAsync_ShouldThrowTranslaasTransportException_WhenHttpRequestExceptionOccurs()
    {
        var inner = new HttpRequestException("connection refused");
        var client = CreateClient(inner);

        var exception = await Assert.ThrowsAsync<TranslaasTransportException>(
            () => client.GetOfflineCacheAsync("project"));

        AssertTransportException(exception, "Failed to download offline cache: ", inner);
    }

    [Fact]
    public async Task ReportMissingKeysAsync_ShouldThrowTranslaasTransportException_WhenHttpRequestExceptionOccurs()
    {
        var inner = new HttpRequestException("connection refused");
        var client = CreateClient(inner);
        var keys = new[]
        {
            new ReportMissingKeyItemRequest
            {
                GroupKey = "ui",
                EntryKey = "greeting",
                LanguageIsoCode = "en"
            }
        };

        var exception = await Assert.ThrowsAsync<TranslaasTransportException>(
            () => client.ReportMissingKeysAsync(keys));

        AssertTransportException(exception, "Failed to report missing keys: ", inner);
    }

    [Fact]
    public async Task ValidateApiKeyAsync_ShouldThrowTranslaasTransportException_WhenHttpRequestExceptionOccurs()
    {
        var inner = new HttpRequestException("connection refused");
        var client = CreateClient(inner);

        var exception = await Assert.ThrowsAsync<TranslaasTransportException>(
            () => client.ValidateApiKeyAsync());

        AssertTransportException(exception, "Failed to validate API key: ", inner);
    }
}
