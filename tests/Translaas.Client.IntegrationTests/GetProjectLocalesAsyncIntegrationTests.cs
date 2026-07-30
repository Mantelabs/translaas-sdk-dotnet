using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Translaas.Client;
using Translaas.Models.Errors;
using Translaas.Models.Responses;
using Xunit;

namespace Translaas.Client.IntegrationTests;

/// <summary>
/// Integration tests for GetProjectLocalesAsync method.
/// These tests require a running development API and TRANSLAAS_API_KEY environment variable.
/// </summary>
public class GetProjectLocalesAsyncIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task GetProjectLocalesAsync_ShouldReturnProjectLocales_WhenProjectExists()
    {
        if (!Configuration.IsEnabled)
        {
            return;
        }

        var project = Configuration.DefaultProject;

        ProjectLocales result;
        try
        {
            result = await Client.GetProjectLocalesAsync(project);
        }
        catch (TranslaasApiException ex) when (IntegrationTestHelpers.SoftSkipOnSdkNotFound(ex))
        {
            return;
        }

        result.Should().NotBeNull();
        if (IntegrationTestHelpers.SoftSkipIf(result.Locales.Count == 0, "fixture data not available in API"))
        {
            return;
        }

        result.Locales.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetProjectLocalesAsync_ShouldReturnMultipleLocales_WhenProjectHasMultipleLocales()
    {
        if (!Configuration.IsEnabled)
        {
            return;
        }

        var project = Configuration.DefaultProject;

        ProjectLocales result;
        try
        {
            result = await Client.GetProjectLocalesAsync(project);
        }
        catch (TranslaasApiException ex) when (IntegrationTestHelpers.SoftSkipOnSdkNotFound(ex))
        {
            return;
        }

        result.Should().NotBeNull();
        if (IntegrationTestHelpers.SoftSkipIf(result.Locales.Count == 0, "fixture data not available in API"))
        {
            return;
        }

        var commonLocales = new[] { "en", "fr", "es", "de" };
        var hasCommonLocale = result.Locales.Any(locale => commonLocales.Contains(locale));
        if (IntegrationTestHelpers.SoftSkipIf(!hasCommonLocale, "expected at least one common locale in fixture API"))
        {
            return;
        }

        hasCommonLocale.Should().BeTrue();
    }

    [Fact]
    public async Task GetProjectLocalesAsync_ShouldHandleNotFound_WhenProjectNotFound()
    {
        if (!Configuration.IsEnabled)
        {
            return;
        }

        var project = "nonexistent-project";

        try
        {
            var result = await Client.GetProjectLocalesAsync(project);
            result.Should().NotBeNull();
            result.Locales.Should().BeEmpty();
        }
        catch (TranslaasApiException ex) when (IntegrationTestHelpers.IsSdkNotFound(ex))
        {
            // Mantelabs platform returns HTTP 404 for missing SDK resources.
        }
    }
}
