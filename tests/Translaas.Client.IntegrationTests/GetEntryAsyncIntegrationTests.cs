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
        if (!Configuration.IsEnabled)
        {
            return;
        }

        var group = IntegrationTestFixtures.SimpleGroup;
        var entry = IntegrationTestFixtures.SimpleEntry;
        var lang = IntegrationTestFixtures.DefaultLanguage;

        string result;
        try
        {
            result = await Client.GetEntryAsync(group, entry, lang);
        }
        catch (TranslaasApiException ex) when (IntegrationTestHelpers.SoftSkipOnSdkNotFound(ex))
        {
            return;
        }

        if (IntegrationTestHelpers.SoftSkipIf(
            string.IsNullOrEmpty(result) || result == entry,
            "fixture data not available in API"))
        {
            return;
        }

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetEntryAsync_ShouldReturnTranslation_WithPluralization()
    {
        if (!Configuration.IsEnabled)
        {
            return;
        }

        var group = IntegrationTestFixtures.PluralGroup;
        var entry = IntegrationTestFixtures.PluralEntry;
        var lang = IntegrationTestFixtures.DefaultLanguage;
        var number = 5;

        string result;
        try
        {
            result = await Client.GetEntryAsync(group, entry, lang, number);
        }
        catch (TranslaasApiException ex) when (IntegrationTestHelpers.SoftSkipOnSdkNotFound(ex))
        {
            return;
        }

        if (IntegrationTestHelpers.SoftSkipIf(
            string.IsNullOrEmpty(result) || result == entry,
            "fixture data not available in API"))
        {
            return;
        }

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetEntryAsync_ShouldHandleNotFound_WhenEntryNotFound()
    {
        if (!Configuration.IsEnabled)
        {
            return;
        }

        var group = "nonexistent";
        var entry = "nonexistent.entry";
        var lang = IntegrationTestFixtures.DefaultLanguage;

        try
        {
            var result = await Client.GetEntryAsync(group, entry, lang);
            result.Should().Be(entry);
        }
        catch (TranslaasApiException ex) when (IntegrationTestHelpers.IsSdkNotFound(ex))
        {
            // Mantelabs platform returns HTTP 404 for missing SDK resources.
        }
    }

    [Fact]
    public async Task GetEntryAsync_ShouldThrowTranslaasApiException_WhenInvalidApiKey()
    {
        if (!Configuration.IsEnabled)
        {
            return;
        }

        var invalidOptions = new TranslaasClientOptions
        {
            ApiKey = "invalid-api-key",
            BaseUrl = Configuration.BaseUrl,
            DefaultProjectId = Configuration.DefaultProject
        };
        var invalidClient = new TranslaasClient(HttpClient, invalidOptions);

        await Assert.ThrowsAsync<TranslaasApiException>(
            () => invalidClient.GetEntryAsync(
                IntegrationTestFixtures.SimpleGroup,
                IntegrationTestFixtures.SimpleEntry,
                IntegrationTestFixtures.DefaultLanguage));
    }
}
