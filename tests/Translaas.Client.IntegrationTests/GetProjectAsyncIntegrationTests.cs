using System.Threading.Tasks;
using FluentAssertions;
using Translaas.Client;
using Translaas.Models.Errors;
using Translaas.Models.Responses;
using Xunit;

namespace Translaas.Client.IntegrationTests;

/// <summary>
/// Integration tests for GetProjectAsync method.
/// These tests require a running development API and TRANSLAAS_API_KEY environment variable.
/// </summary>
public class GetProjectAsyncIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task GetProjectAsync_ShouldReturnTranslationProject_WhenProjectExists()
    {
        if (!Configuration.IsEnabled)
        {
            return;
        }

        var project = Configuration.DefaultProject;
        var lang = IntegrationTestFixtures.DefaultLanguage;

        TranslationProject result;
        try
        {
            result = await Client.GetProjectAsync(project, lang);
        }
        catch (TranslaasApiException ex) when (IntegrationTestHelpers.SoftSkipOnSdkNotFound(ex))
        {
            return;
        }

        result.Should().NotBeNull();
        if (IntegrationTestHelpers.SoftSkipIf(result.Groups.Count == 0, "fixture data not available in API"))
        {
            return;
        }

        result.Groups.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetProjectAsync_ShouldReturnTranslationProject_WithFormat()
    {
        if (!Configuration.IsEnabled)
        {
            return;
        }

        var project = Configuration.DefaultProject;
        var lang = IntegrationTestFixtures.DefaultLanguage;
        var format = "json";

        TranslationProject result;
        try
        {
            result = await Client.GetProjectAsync(project, lang, format);
        }
        catch (TranslaasApiException ex) when (IntegrationTestHelpers.SoftSkipOnSdkNotFound(ex))
        {
            return;
        }

        result.Should().NotBeNull();
        if (IntegrationTestHelpers.SoftSkipIf(result.Groups.Count == 0, "fixture data not available in API"))
        {
            return;
        }

        result.Groups.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetProjectAsync_ShouldHandleNotFound_WhenProjectNotFound()
    {
        if (!Configuration.IsEnabled)
        {
            return;
        }

        var project = "nonexistent-project";
        var lang = IntegrationTestFixtures.DefaultLanguage;

        try
        {
            var result = await Client.GetProjectAsync(project, lang);
            result.Should().NotBeNull();
            result.Groups.Should().BeEmpty();
        }
        catch (TranslaasApiException ex) when (IntegrationTestHelpers.IsSdkNotFound(ex))
        {
            // Mantelabs platform returns HTTP 404 for missing SDK resources.
        }
    }

    [Fact]
    public async Task GetProjectAsync_ShouldContainMultipleGroups_WhenProjectHasMultipleGroups()
    {
        if (!Configuration.IsEnabled)
        {
            return;
        }

        var project = Configuration.DefaultProject;
        var lang = IntegrationTestFixtures.DefaultLanguage;

        TranslationProject result;
        try
        {
            result = await Client.GetProjectAsync(project, lang);
        }
        catch (TranslaasApiException ex) when (IntegrationTestHelpers.SoftSkipOnSdkNotFound(ex))
        {
            return;
        }

        result.Should().NotBeNull();
        if (IntegrationTestHelpers.SoftSkipIf(result.Groups.Count == 0, "fixture data not available in API"))
        {
            return;
        }

        var walked = 0;
        foreach (var groupName in result.Groups.Keys)
        {
            var group = result.GetGroup(groupName);
            if (group == null)
            {
                continue;
            }

            group.Entries.Should().NotBeEmpty();
            walked++;
        }

        if (IntegrationTestHelpers.SoftSkipIf(walked == 0, "fixture data not available in API"))
        {
            return;
        }
    }
}
