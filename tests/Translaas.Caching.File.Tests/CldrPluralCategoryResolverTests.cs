using FluentAssertions;

using Translaas.Models;

namespace Translaas.Caching.File.Tests;

public class CldrPluralCategoryResolverTests
{
    public static TheoryData<string, decimal, PluralCategory> GoldenPluralRows =>
        new()
        {
            { "ar", 0m, PluralCategory.Zero },
            { "ar", 2m, PluralCategory.Two },
            { "pl", 2m, PluralCategory.Few },
            { "fr", 0m, PluralCategory.One },
            { "en", 0m, PluralCategory.Other },
            { "en", 1m, PluralCategory.One },
        };

    public static TheoryData<string, decimal, PluralCategory> AntiBucketPluralRows =>
        new()
        {
            { "he", 2m, PluralCategory.Two },
            { "ja", 1m, PluralCategory.Other },
            { "pt", 0m, PluralCategory.One },
            { "pt-PT", 0m, PluralCategory.Other },
            { "bg", 2m, PluralCategory.Other },
            { "es", 0m, PluralCategory.Other },
            { "fr-CA", 0m, PluralCategory.One },
            { "ar_EG", 0m, PluralCategory.Zero },
        };

    [Theory]
    [MemberData(nameof(GoldenPluralRows))]
    public void ResolveCategory_WhenGoldenTableRow_ReturnsExpectedCategory(
        string lang,
        decimal number,
        PluralCategory expected)
    {
        CldrPluralCategoryResolver.ResolveCategory(number, lang).Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(AntiBucketPluralRows))]
    public void ResolveCategory_WhenAntiBucketRow_ReturnsExpectedCategory(
        string lang,
        decimal number,
        PluralCategory expected)
    {
        CldrPluralCategoryResolver.ResolveCategory(number, lang).Should().Be(expected);
    }

    [Fact]
    public void ResolveCategory_WhenNullNumber_ReturnsOther()
    {
        CldrPluralCategoryResolver.ResolveCategory(null, "ar").Should().Be(PluralCategory.Other);
        CldrPluralCategoryResolver.ResolveCategory(null, "en").Should().Be(PluralCategory.Other);
    }

    [Fact]
    public void ResolveCategory_WhenLocaleUsesUnderscore_NormalizesToHyphen()
    {
        CldrPluralCategoryResolver.ResolveCategory(1m, "en_US").Should().Be(PluralCategory.One);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ResolveCategory_WhenLangMissing_FallsBackToEnglish(string? lang)
    {
        CldrPluralCategoryResolver.ResolveCategory(1m, lang).Should().Be(PluralCategory.One);
        CldrPluralCategoryResolver.ResolveCategory(0m, lang).Should().Be(PluralCategory.Other);
    }

    [Fact]
    public void ResolveCategory_WhenInvalidLocale_FallsBackToEnglish()
    {
        CldrPluralCategoryResolver.ResolveCategory(0m, "not a locale!!").Should().Be(PluralCategory.Other);
        CldrPluralCategoryResolver.ResolveCategory(1m, "not a locale!!").Should().Be(PluralCategory.One);
    }
}
