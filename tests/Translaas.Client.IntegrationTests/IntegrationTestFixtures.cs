namespace Translaas.Client.IntegrationTests;

/// <summary>
/// Canonical fixture ids for live API integration tests.
/// Aligned with <a href="https://github.com/Mantelabs/translaas-sdk-examples">translaas-sdk-examples</a>
/// (<c>dotnet/docs/translaas_sdk_samples_strings.csv</c>) and local Mantelabs Docker seed data.
/// </summary>
public static class IntegrationTestFixtures
{
    /// <summary>Default project id for scoped reads (local Docker dogfoods this project).</summary>
    public const string DefaultProject = "translaas-sdk-samples";

    /// <summary>Group for simple entry reads.</summary>
    public const string SimpleGroup = "common";

    /// <summary>Simple entry key.</summary>
    public const string SimpleEntry = "welcome.message";

    /// <summary>Group for plural entry reads.</summary>
    public const string PluralGroup = "messages";

    /// <summary>Plural entry key.</summary>
    public const string PluralEntry = "item";

    /// <summary>Default language code.</summary>
    public const string DefaultLanguage = "en";
}
