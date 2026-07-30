using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Translaas.Models.Responses;

/// <summary>
/// Represents a translation project containing multiple translation groups.
/// </summary>
public class TranslationProject
{
    /// <summary>
    /// Project identifier from the delivery API root payload (not a translation group).
    /// </summary>
    [JsonPropertyName("Project")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Project { get; set; }

    /// <summary>
    /// Language code from the delivery API root payload (not a translation group).
    /// </summary>
    [JsonPropertyName("Lang")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Lang { get; set; }

    /// <summary>
    /// Content version from the delivery API root payload (not a translation group).
    /// </summary>
    [JsonPropertyName("Version")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? Version { get; set; }

    /// <summary>
    /// Generation timestamp from the delivery API root payload (not a translation group).
    /// </summary>
    [JsonPropertyName("GeneratedAt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? GeneratedAt { get; set; }

    /// <summary>
    /// Release channel from the delivery API root payload (not a translation group).
    /// </summary>
    [JsonPropertyName("Channel")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Channel { get; set; }

    /// <summary>
    /// Optional per-group entry context when <c>includeContext</c> is enabled.
    /// </summary>
    [JsonPropertyName("groupEntryContext")]
    public Dictionary<string, JsonElement>? GroupEntryContext { get; set; }

    /// <summary>
    /// Gets or sets the dictionary of translation groups, where the key is the group name
    /// and the value is the translation group.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement> Groups { get; set; } = [];

    /// <summary>
    /// Gets a translation group by name.
    /// </summary>
    /// <param name="groupName">The group name.</param>
    /// <returns>The translation group, or null if not found or not an object.</returns>
    /// <example>
    /// <code>
    /// TranslationProject project = await client.GetProjectAsync("my-project", "en");
    /// TranslationGroup? uiGroup = project.GetGroup("ui");
    /// if (uiGroup != null)
    /// {
    ///     string welcome = uiGroup.GetValue("welcome");
    /// }
    /// </code>
    /// </example>
    public TranslationGroup? GetGroup(string groupName)
    {
        if (!Groups.TryGetValue(groupName, out var element) || element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        // Check if this is a full TranslationGroup JSON (from API) or just entries dictionary (from cache file)
        // Cache files store groups as flat entry dictionaries: { "app.name": "...", "welcome": "..." }
        // API returns full TranslationGroup: { "Project": "...", "Lang": "...", "Entries": { ... } }
        if (element.TryGetProperty("Entries", out _))
        {
            // Full TranslationGroup structure - deserialize normally
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<TranslationGroup>(element.GetRawText(), options);
        }

        // Flat entries dictionary from cache file - wrap it in a TranslationGroup
        var group = new TranslationGroup();
        var deserializeOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        group.Entries = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(element.GetRawText(), deserializeOptions)
            ?? new Dictionary<string, JsonElement>();
        return group;
    }
}
