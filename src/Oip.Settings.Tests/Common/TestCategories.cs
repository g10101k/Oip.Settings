namespace Oip.Settings.Tests.Common;

/// <summary>
/// Well-known NUnit category names used across the test suite
/// </summary>
public static class TestCategories
{
    /// <summary>
    /// Tests that require an external service to be up and running (PostgreSQL, MS SQL Server, etc.).
    /// Exclude them with: dotnet test --filter "TestCategory!=Integration"
    /// </summary>
    public const string Integration = "Integration";
}
