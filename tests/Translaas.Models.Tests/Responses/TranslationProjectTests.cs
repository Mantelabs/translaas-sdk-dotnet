using System.Text.Json;
using FluentAssertions;
using Translaas.Models.Responses;

namespace Translaas.Models.Tests.Responses;

public class TranslationProjectTests
{
    [Fact]
    public void TranslationProject_ShouldDeserializeFromJson()
    {
        // Arrange
        var json = """{"ui":{"button.save":"Save","button.cancel":"Cancel"},"common":{"welcome":"Welcome"}}""";

        // Act
        var project = JsonSerializer.Deserialize<TranslationProject>(json);

        // Assert
        project.Should().NotBeNull();
        project!.Groups.Should().NotBeNull();
        project.Groups.Should().HaveCount(2);
        project.Groups.Should().ContainKey("ui");
        project.Groups.Should().ContainKey("common");
    }

    [Fact]
    public void TranslationProject_GetGroup_ShouldReturnCorrectGroup()
    {
        // Arrange
        var json = """{"ui":{"button.save":"Save","button.cancel":"Cancel"},"common":{"welcome":"Welcome"}}""";
        var project = JsonSerializer.Deserialize<TranslationProject>(json);

        // Act
        var uiGroup = project!.GetGroup("ui");
        var commonGroup = project.GetGroup("common");
        var missingGroup = project.GetGroup("missing");

        // Assert
        uiGroup.Should().NotBeNull();
        uiGroup!.GetValue("button.save").Should().Be("Save");
        uiGroup.GetValue("button.cancel").Should().Be("Cancel");
        
        commonGroup.Should().NotBeNull();
        commonGroup!.GetValue("welcome").Should().Be("Welcome");
        
        missingGroup.Should().BeNull();
    }

    [Fact]
    public void TranslationProject_ShouldSerializeToJson()
    {
        // Arrange
        var json = """{"ui":{"button.save":"Save"},"common":{"welcome":"Welcome"}}""";
        var project = JsonSerializer.Deserialize<TranslationProject>(json);

        // Act
        var serialized = JsonSerializer.Serialize(project);

        // Assert
        serialized.Should().Contain("\"ui\"");
        serialized.Should().Contain("\"button.save\"");
        serialized.Should().Contain("\"common\"");
        serialized.Should().Contain("\"welcome\"");
    }

    [Fact]
    public void TranslationProject_ShouldHandleEmptyGroups()
    {
        // Arrange
        var json = """{}""";

        // Act
        var project = JsonSerializer.Deserialize<TranslationProject>(json);

        // Assert
        project.Should().NotBeNull();
        project!.Groups.Should().NotBeNull();
        project.Groups.Should().BeEmpty();
    }

    [Fact]
    public void TranslationProject_ShouldExcludeApiMetadataFromGroups()
    {
        // Mantelabs delivery API includes root metadata alongside group objects.
        var json = """
            {
              "Project": "translaas-sdk-samples",
              "Lang": "en",
              "Version": 245734752,
              "GeneratedAt": "2026-01-15T12:00:00Z",
              "groupEntryContext": { "common": { "welcome.message": { "note": "ctx" } } },
              "common": { "welcome.message": "Welcome" },
              "messages": { "item": "1 item" }
            }
            """;

        var project = JsonSerializer.Deserialize<TranslationProject>(json);

        project.Should().NotBeNull();
        project!.Project.Should().Be("translaas-sdk-samples");
        project.Lang.Should().Be("en");
        project.Version.Should().Be(245734752);
        project.GeneratedAt.Should().NotBeNull();
        project.Groups.Should().HaveCount(2);
        project.Groups.Should().ContainKey("common");
        project.Groups.Should().ContainKey("messages");
        project.Groups.Should().NotContainKey("Project");
        project.Groups.Should().NotContainKey("Lang");
        project.Groups.Should().NotContainKey("Version");
        project.Groups.Should().NotContainKey("GeneratedAt");
        project.GroupEntryContext.Should().NotBeNull();
    }

    [Fact]
    public void TranslationProject_GetGroup_ShouldIgnoreNonObjectValues()
    {
        var json = """
            {
              "Version": 123,
              "common": { "welcome.message": "Welcome" }
            }
            """;

        var project = JsonSerializer.Deserialize<TranslationProject>(json);

        project.Should().NotBeNull();
        project!.Version.Should().Be(123);
        project.Groups.Should().NotContainKey("Version");
        project.GetGroup("Version").Should().BeNull();
        project.GetGroup("common")!.GetValue("welcome.message").Should().Be("Welcome");
    }

    [Fact]
    public void TranslationProject_ShouldSerializeFlatCacheShapeWithoutMetadata()
    {
        var json = """{"ui":{"button.save":"Save"},"common":{"welcome":"Welcome"}}""";
        var project = JsonSerializer.Deserialize<TranslationProject>(json);

        var serialized = JsonSerializer.Serialize(project);

        serialized.Should().Contain("\"ui\"");
        serialized.Should().Contain("\"common\"");
        serialized.Should().NotContain("\"Project\"");
        serialized.Should().NotContain("\"Version\"");
    }

    [Fact]
    public void TranslationProject_GetGroup_ShouldReturnNull_WhenAllGroupsAreAccessible()
    {
        var json = """
            {
              "Project": "translaas-sdk-samples",
              "Lang": "en",
              "Version": 1,
              "common": { "welcome.message": "Welcome" },
              "messages": { "item.one": "1 item", "item.other": "{count} items" }
            }
            """;

        var project = JsonSerializer.Deserialize<TranslationProject>(json);

        project.Should().NotBeNull();
        foreach (var groupName in project!.Groups.Keys)
        {
            var group = project.GetGroup(groupName);
            group.Should().NotBeNull($"group '{groupName}' should deserialize");
            group!.Entries.Should().NotBeEmpty($"group '{groupName}' should have entries");
        }
    }
}
