using System;
using System.Collections.Concurrent;

using ICU4N.Text;

using Translaas.Models;

namespace Translaas.Caching.File;

/// <summary>
/// Resolves CLDR cardinal plural categories for offline / file-cache entry selection.
/// </summary>
/// <remarks>
/// Uses ICU4N <see cref="PluralRules"/> so locales such as Arabic, Polish, and French
/// select <c>zero</c> / <c>two</c> / <c>few</c> / <c>one</c> instead of an English-like
/// <c>n == 1</c> heuristic. Pass a full BCP-47 tag (<c>pt</c> vs <c>pt-PT</c>).
/// Invalid or empty language tags fall back to the base language, then <c>en</c>.
/// Live HTTP <c>GetEntry</c> is unchanged — the server still selects from <c>n</c>.
/// </remarks>
public static class CldrPluralCategoryResolver
{
    private const string EnglishLocale = "en";
    private static readonly ConcurrentDictionary<string, PluralRules> RulesCache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Resolves the CLDR cardinal category for <paramref name="number"/> and <paramref name="lang"/>.
    /// </summary>
    /// <param name="number">The count used for plural selection. When <see langword="null"/>, returns <see cref="PluralCategory.Other"/>.</param>
    /// <param name="lang">BCP-47 language tag (hyphens or underscores). Empty or invalid tags fall back to <c>en</c>.</param>
    /// <returns>The CLDR plural category.</returns>
    public static PluralCategory ResolveCategory(decimal? number, string? lang)
    {
        if (!number.HasValue)
        {
            return PluralCategory.Other;
        }

        return ResolveCategory(number.Value, lang);
    }

    /// <summary>
    /// Resolves the CLDR cardinal category for <paramref name="number"/> and <paramref name="lang"/>.
    /// </summary>
    /// <param name="number">The count used for plural selection.</param>
    /// <param name="lang">BCP-47 language tag (hyphens or underscores). Empty or invalid tags fall back to <c>en</c>.</param>
    /// <returns>The CLDR plural category.</returns>
    public static PluralCategory ResolveCategory(decimal number, string? lang)
    {
        var rules = GetPluralRules(lang);
        var keyword = rules.Select((double)number);
        return MapKeyword(keyword);
    }

    private static PluralRules GetPluralRules(string? lang)
    {
        var locale = NormalizeLocaleTag(lang);
        return RulesCache.GetOrAdd(locale, CreatePluralRules);
    }

    private static string NormalizeLocaleTag(string? lang)
    {
        if (string.IsNullOrWhiteSpace(lang))
        {
            return EnglishLocale;
        }

        var trimmed = lang.Trim().Replace('_', '-');
        return trimmed.Length == 0 ? EnglishLocale : trimmed;
    }

    private static PluralRules CreatePluralRules(string locale)
    {
        var rules = TryGetInstance(locale);
        if (rules is not null)
        {
            return rules;
        }

        var baseLanguage = GetBaseLanguage(locale);
        if (!string.Equals(baseLanguage, locale, StringComparison.OrdinalIgnoreCase))
        {
            rules = TryGetInstance(baseLanguage);
            if (rules is not null)
            {
                return rules;
            }
        }

        return PluralRules.GetInstance(EnglishLocale);
    }

    private static string GetBaseLanguage(string locale)
    {
        var separator = locale.IndexOf('-');
        if (separator <= 0)
        {
            return locale;
        }

        return locale.Substring(0, separator);
    }

    private static PluralRules? TryGetInstance(string locale)
    {
        if (!LooksLikeBcp47(locale))
        {
            return null;
        }

        try
        {
            return PluralRules.GetInstance(locale);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    /// <summary>
    /// Accepts language tags such as <c>en</c>, <c>pt-PT</c>, <c>zh-Hans-CN</c>.
    /// Rejects free text so ICU does not silently use the root locale (always <c>other</c>).
    /// </summary>
    private static bool LooksLikeBcp47(string locale)
    {
        if (locale.Length < 2)
        {
            return false;
        }

        var firstSegment = true;
        var segmentLength = 0;
        foreach (var c in locale)
        {
            if (c == '-')
            {
                if (segmentLength == 0 || (firstSegment && segmentLength < 2))
                {
                    return false;
                }

                firstSegment = false;
                segmentLength = 0;
                continue;
            }

            var isLetter = (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
            var isDigit = c >= '0' && c <= '9';
            if (firstSegment)
            {
                if (!isLetter)
                {
                    return false;
                }
            }
            else if (!isLetter && !isDigit)
            {
                return false;
            }

            segmentLength++;
            if (segmentLength > 8)
            {
                return false;
            }
        }

        return firstSegment ? segmentLength >= 2 : segmentLength >= 1;
    }

    private static PluralCategory MapKeyword(string keyword)
    {
        return keyword switch
        {
            "zero" => PluralCategory.Zero,
            "one" => PluralCategory.One,
            "two" => PluralCategory.Two,
            "few" => PluralCategory.Few,
            "many" => PluralCategory.Many,
            _ => PluralCategory.Other,
        };
    }
}
