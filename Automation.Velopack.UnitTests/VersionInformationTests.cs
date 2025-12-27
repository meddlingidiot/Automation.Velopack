namespace Automation.Velopack.UnitTests;

public class VersionInformationTests
{
    [Fact]
    public void FullVersion_ReturnsNonEmptyString()
    {
        // Act
        var fullVersion = VersionInformation.FullVersion;

        // Assert
        Assert.NotNull(fullVersion);
        Assert.NotEmpty(fullVersion);
    }

    [Fact]
    public void Version_ReturnsVersionWithoutCommitHash()
    {
        // Act
        var version = VersionInformation.Version;

        // Assert
        Assert.NotNull(version);
        Assert.NotEmpty(version);
        Assert.DoesNotContain("+", version);
    }

    [Fact]
    public void Version_IsShorterThanOrEqualToFullVersion()
    {
        // Act
        var fullVersion = VersionInformation.FullVersion;
        var version = VersionInformation.Version;

        // Assert
        Assert.True(version.Length <= fullVersion.Length);
    }

    [Fact]
    public void FullVersion_ContainsVersion()
    {
        // Act
        var fullVersion = VersionInformation.FullVersion;
        var version = VersionInformation.Version;

        // Assert
        Assert.StartsWith(version, fullVersion);
    }
}
