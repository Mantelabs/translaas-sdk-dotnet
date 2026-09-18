using System.Text.Json;

using FluentAssertions;

using Moq;
using Translaas.Client;
using Translaas.Models;
using Translaas.Models.Responses;

namespace Translaas.Caching.File.Tests;

public class CachingTranslaasClientPluralTests
{
    private const string DefaultProjectId = "test-project";
    private readonly Mock<ITranslaasClient> _mockInnerClient;
    private readonly Mock<IOfflineCacheProvider> _mockCacheProvider;
    private readonly OfflineCacheOptions _options;

    public CachingTranslaasClientPluralTests()
    {
        _mockInnerClient = new Mock<ITranslaasClient>();
        _mockCacheProvider = new Mock<IOfflineCacheProvider>();
        _options = new OfflineCacheOptions
        {
            Enabled = true,
            FallbackMode = OfflineFallbackMode.CacheOnly,
            DefaultProjectId = DefaultProjectId
        };
    }

    public static TheoryData<string, decimal, string> GoldenGetEntryRows =>
        new()
        {
            { "ar", 0m, "FORM:zero" },
            { "ar", 2m, "FORM:two" },
            { "pl", 2m, "FORM:few" },
            { "fr", 0m, "FORM:one" },
            { "en", 0m, "FORM:other" },
            { "en", 1m, "FORM:one" },
            { "pt", 0m, "FORM:one" },
            { "pt-PT", 0m, "FORM:other" },
        };

    [Theory]
    [MemberData(nameof(GoldenGetEntryRows))]
    public async Task GetEntryAsync_CacheOnly_ReturnsFormForCldrCategory(
        string lang,
        decimal number,
        string expected)
    {
        var client = CreateClient(OfflineFallbackMode.CacheOnly);
        SetupGroup(lang, CreateTranslationGroupWithPluralForms("items"));

        var result = await client.GetEntryAsync("messages", "items", lang, number);

        result.Should().Be(expected);
        _mockInnerClient.Verify(
            c => c.GetEntryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal?>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<Translaas.Models.TranslaasRequestContext?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetEntryAsync_CacheOnly_FallsBackToOtherForm_WhenSelectedCategoryMissing()
    {
        var client = CreateClient(OfflineFallbackMode.CacheOnly);
        SetupGroup("ar", CreateTranslationGroupWithPluralForms(
            "items",
            new Dictionary<string, string> { ["other"] = "FORM:other-only" }));

        var result = await client.GetEntryAsync("messages", "items", "ar", 2m);

        result.Should().Be("FORM:other-only");
    }

    [Fact]
    public async Task GetEntryAsync_CacheOnly_ReturnsOtherForm_WhenNumberIsNull()
    {
        var client = CreateClient(OfflineFallbackMode.CacheOnly);
        SetupGroup("en", CreateTranslationGroupWithPluralForms("items"));

        var result = await client.GetEntryAsync("messages", "items", "en", number: null);

        result.Should().Be("FORM:other");
    }

    [Fact]
    public async Task GetEntryAsync_CacheOnly_StillSubstitutesN_WhenPluralFormSelected()
    {
        var client = CreateClient(OfflineFallbackMode.CacheOnly);
        SetupGroup("en", CreateTranslationGroupWithPluralForms(
            "items",
            new Dictionary<string, string> { ["other"] = "Count {N}" }));

        var result = await client.GetEntryAsync("messages", "items", "en", 0m);

        result.Should().Be("Count 0");
    }

    [Fact]
    public async Task GetEntryAsync_CacheOnly_ReturnsSimpleString_WhenEntryNotPlural()
    {
        var client = CreateClient(OfflineFallbackMode.CacheOnly);
        var group = new TranslationGroup();
        group.Entries["hello"] = JsonDocument.Parse(JsonSerializer.Serialize("Hello World")).RootElement;
        SetupGroup("en", group);

        var result = await client.GetEntryAsync("common", "hello", "en", 5m);

        result.Should().Be("Hello World");
    }

    [Fact]
    public async Task GetEntryAsync_CacheFirst_ReturnsCldrPlural_WhenCacheHit()
    {
        var client = CreateClient(OfflineFallbackMode.CacheFirst);
        SetupGroup("pl", CreateTranslationGroupWithPluralForms("items"));

        var result = await client.GetEntryAsync("messages", "items", "pl", 2m);

        result.Should().Be("FORM:few");
        _mockInnerClient.Verify(
            c => c.GetEntryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal?>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<Translaas.Models.TranslaasRequestContext?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetEntryAsync_CacheFirst_UsesPtPtRules_WhenLocaleIsPtPt()
    {
        var client = CreateClient(OfflineFallbackMode.CacheFirst);
        SetupGroup("pt-PT", CreateTranslationGroupWithPluralForms("items"));

        var result = await client.GetEntryAsync("messages", "items", "pt-PT", 0m);

        result.Should().Be("FORM:other");
    }

    private CachingTranslaasClient CreateClient(OfflineFallbackMode mode)
    {
        _options.FallbackMode = mode;
        return new CachingTranslaasClient(_mockInnerClient.Object, _mockCacheProvider.Object, _options, DefaultProjectId);
    }

    private void SetupGroup(string lang, TranslationGroup group)
    {
        _mockCacheProvider
            .Setup(c => c.GetGroupAsync(DefaultProjectId, It.IsAny<string>(), lang, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
    }

    private static TranslationGroup CreateTranslationGroupWithPluralForms(
        string entryKey,
        IReadOnlyDictionary<string, string>? forms = null)
    {
        forms ??= new Dictionary<string, string>
        {
            ["zero"] = "FORM:zero",
            ["one"] = "FORM:one",
            ["two"] = "FORM:two",
            ["few"] = "FORM:few",
            ["many"] = "FORM:many",
            ["other"] = "FORM:other",
        };

        var group = new TranslationGroup();
        var json = JsonSerializer.Serialize(forms);
        using var document = JsonDocument.Parse(json);
        group.Entries[entryKey] = document.RootElement.Clone();
        return group;
    }
}
