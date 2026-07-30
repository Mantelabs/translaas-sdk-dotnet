using System;
using System.Net;
using Translaas.Models.Errors;

namespace Translaas.Client.IntegrationTests;

/// <summary>
/// Shared helpers for live API integration tests (Mantelabs 404 vs legacy 204 semantics).
/// </summary>
internal static class IntegrationTestHelpers
{
    /// <summary>
    /// Hint printed when the configured project or resource is missing on the Mantelabs platform.
    /// </summary>
    public const string SdkNotFoundSkipMessage =
        "SDK resource not found (HTTP 404) — set TRANSLAAS_DEFAULT_PROJECT to an existing project id (default: translaas-sdk-samples)";

    /// <summary>
    /// Returns true when the delivery API reports a missing SDK resource (Mantelabs platform uses HTTP 404).
    /// </summary>
    public static bool IsSdkNotFound(TranslaasApiException exception) =>
        exception.StatusCode == HttpStatusCode.NotFound;

    /// <summary>
    /// Logs and returns true when a test should soft-skip due to missing fixture data or API behavior.
    /// </summary>
    public static bool SoftSkipIf(bool condition, string message)
    {
        if (condition)
        {
            Console.Error.WriteLine($"skipping: {message}");
        }

        return condition;
    }

    /// <summary>
    /// Soft-skips when the configured project (or resource) is missing on the API.
    /// </summary>
    public static bool SoftSkipOnSdkNotFound(TranslaasApiException exception)
    {
        if (!IsSdkNotFound(exception))
        {
            return false;
        }

        SoftSkipIf(true, SdkNotFoundSkipMessage);
        return true;
    }
}
