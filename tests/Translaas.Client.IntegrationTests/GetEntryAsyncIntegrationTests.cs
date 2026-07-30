using System.Threading.Tasks;
using FluentAssertions;
using Translaas.Client;
using Translaas.Models.Errors;
using Xunit;

namespace Translaas.Client.IntegrationTests;

/// <summary>
/// Integration tests for GetEntryAsync method.
/// These tests require a running development API and TRANSLAAS_API_KEY environment variable.
/// </summary>
public class GetEntryAsyncIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task GetEntryAsync_ShouldReturnTranslation_WhenEntryExists()
    {
        // Skip if integration tests are not enabled
        if (!Configuration.IsEnabled)
        {
            return;
        }

        // Arrange
        var group = IntegrationTestFixtures.SimpleGroup;
        var entry = IntegrationTestFixtures.SimpleEntry;
        var lang = IntegrationTestFixtures.DefaultLanguage;

        // Act
        var result = await Client.GetEntryAsync(group, entry, lang);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetEntryAsync_ShouldReturnTranslation_WithPluralization()
    {
        // Skip if integration tests are not enabled
        if (!Configuration.IsEnabled)
        {
            return;
        }

        // Arrange
        var group = IntegrationTestFixtures.PluralGroup;
        var entry = IntegrationTestFixtures.PluralEntry;
        var lang = IntegrationTestFixtures.DefaultLanguage;
        var number = 5;

        // Act
        var result = await Client.GetEntryAsync(group, entry, lang, number);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetEntryAsync_ShouldHandleNotFound_WhenEntryNotFound()
    {
        // Skip if integration tests are not enabled
        if (!Configuration.IsEnabled)
        {
            return;
        }

        // Arrange
        var group = "nonexistent";
        var entry = "nonexistent.entry";
        var lang = IntegrationTestFixtures.DefaultLanguage;

        // Act
        // Note: API returns 204 No Content for non-existent entries, which returns the entry key as fallback
        var result = await Client.GetEntryAsync(group, entry, lang);

        // Assert
        // When entry is not found, API returns 204 and client returns the entry key as fallback
        result.Should().NotBeNull();
        result.Should().Be(entry); // Client returns entry key when 204 No Content is received
    }

    [Fact]
    public async Task GetEntryAsync_ShouldThrowTranslaasApiException_WhenInvalidApiKey()
    {
        // Skip if integration tests are not enabled
        if (!Configuration.IsEnabled)
        {
            return;
        }

        // Arrange
        var invalidOptions = new TranslaasClientOptions
        {
            ApiKey = "invalid-api-key",
            BaseUrl = Configuration.BaseUrl,
            DefaultProjectId = Configuration.DefaultProject
        };
        var invalidClient = new TranslaasClient(HttpClient, invalidOptions);

        // Act & Assert
        await Assert.ThrowsAsync<TranslaasApiException>(
            () => invalidClient.GetEntryAsync(
                IntegrationTestFixtures.SimpleGroup,
                IntegrationTestFixtures.SimpleEntry,
                IntegrationTestFixtures.DefaultLanguage));
    }
}
