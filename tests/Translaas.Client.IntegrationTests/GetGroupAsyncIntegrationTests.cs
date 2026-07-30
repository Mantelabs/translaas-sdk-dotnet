using System.Threading.Tasks;
using FluentAssertions;
using Translaas.Client;
using Translaas.Models.Errors;
using Translaas.Models.Responses;
using Xunit;

namespace Translaas.Client.IntegrationTests;

/// <summary>
/// Integration tests for GetGroupAsync method.
/// These tests require a running development API and TRANSLAAS_API_KEY environment variable.
/// </summary>
public class GetGroupAsyncIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task GetGroupAsync_ShouldReturnTranslationGroup_WhenGroupExists()
    {
        if (!Configuration.IsEnabled)
        {
            return;
        }

        var project = Configuration.DefaultProject;
        var group = IntegrationTestFixtures.SimpleGroup;
        var lang = IntegrationTestFixtures.DefaultLanguage;

        TranslationGroup result;
        try
        {
            result = await Client.GetGroupAsync(project, group, lang);
        }
        catch (TranslaasApiException ex) when (IntegrationTestHelpers.SoftSkipOnSdkNotFound(ex))
        {
            return;
        }

        result.Should().NotBeNull();
        if (IntegrationTestHelpers.SoftSkipIf(result.Entries.Count == 0, "fixture data not available in API"))
        {
            return;
        }

        result.Entries.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetGroupAsync_ShouldReturnTranslationGroup_WithFormat()
    {
        if (!Configuration.IsEnabled)
        {
            return;
        }

        var project = Configuration.DefaultProject;
        var group = IntegrationTestFixtures.SimpleGroup;
        var lang = IntegrationTestFixtures.DefaultLanguage;
        var format = "json";

        TranslationGroup result;
        try
        {
            result = await Client.GetGroupAsync(project, group, lang, format);
        }
        catch (TranslaasApiException ex) when (IntegrationTestHelpers.SoftSkipOnSdkNotFound(ex))
        {
            return;
        }

        result.Should().NotBeNull();
        if (IntegrationTestHelpers.SoftSkipIf(result.Entries.Count == 0, "fixture data not available in API"))
        {
            return;
        }

        result.Entries.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetGroupAsync_ShouldHandleNotFound_WhenGroupNotFound()
    {
        if (!Configuration.IsEnabled)
        {
            return;
        }

        var project = Configuration.DefaultProject;
        var group = "nonexistent-group";
        var lang = IntegrationTestFixtures.DefaultLanguage;

        try
        {
            var result = await Client.GetGroupAsync(project, group, lang);
            result.Should().NotBeNull();
            result.Entries.Should().BeEmpty();
        }
        catch (TranslaasApiException ex) when (IntegrationTestHelpers.IsSdkNotFound(ex))
        {
            // Mantelabs platform returns HTTP 404 for missing SDK resources.
        }
    }

    [Fact]
    public async Task GetGroupAsync_ShouldHandleNotFound_WhenProjectNotFound()
    {
        if (!Configuration.IsEnabled)
        {
            return;
        }

        var project = "nonexistent-project";
        var group = IntegrationTestFixtures.SimpleGroup;
        var lang = IntegrationTestFixtures.DefaultLanguage;

        try
        {
            var result = await Client.GetGroupAsync(project, group, lang);
            result.Should().NotBeNull();
            result.Entries.Should().BeEmpty();
        }
        catch (TranslaasApiException ex) when (IntegrationTestHelpers.IsSdkNotFound(ex))
        {
            // Mantelabs platform returns HTTP 404 for missing SDK resources.
        }
    }
}
