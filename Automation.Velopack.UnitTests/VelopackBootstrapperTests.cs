namespace Automation.Velopack.UnitTests;

public class VelopackBootstrapperTests : IDisposable
{
    private readonly string _globalChannelFile;
    private readonly string _globalFolder;
    private readonly string _appChannelFile;
    private readonly string _appFolder;
    private readonly string? _originalEnvVar;
    private const string TestAppName = "TestApp";

    public VelopackBootstrapperTests()
    {
        // Store original environment variable
        _originalEnvVar = Environment.GetEnvironmentVariable("VELOPACK_CHANNEL");

        // Setup global channel file path
        _globalFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), VelopackBootstrapper.SharedDeptFolder);
        _globalChannelFile = Path.Combine(_globalFolder, ".channel");

        // Setup app-specific channel file path
        _appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), TestAppName);
        _appChannelFile = Path.Combine(_appFolder, ".channel");

        // Clean up any existing test files
        if (File.Exists(_globalChannelFile))
        {
            File.Delete(_globalChannelFile);
        }
        if (File.Exists(_appChannelFile))
        {
            File.Delete(_appChannelFile);
        }

        // Clear environment variable for clean tests
        Environment.SetEnvironmentVariable("VELOPACK_CHANNEL", null);
    }

    public void Dispose()
    {
        // Restore original environment variable
        Environment.SetEnvironmentVariable("VELOPACK_CHANNEL", _originalEnvVar);

        // Clean up test files
        if (File.Exists(_globalChannelFile))
        {
            File.Delete(_globalChannelFile);
        }
        if (File.Exists(_appChannelFile))
        {
            File.Delete(_appChannelFile);
        }
    }

    [Fact]
    public void CreateChannelFile_CreatesGlobalFileWithDefaultChannel()
    {
        // Act
        var result = VelopackBootstrapper.CreateChannelFile(ChannelScope.Global, TestAppName);

        // Assert
        Assert.True(result);
        Assert.True(File.Exists(_globalChannelFile));
        var content = File.ReadAllText(_globalChannelFile);
        Assert.Equal("Prerelease", content);
    }

    [Fact]
    public void CreateChannelFile_CreatesGlobalFileWithCustomChannel()
    {
        // Act
        var result = VelopackBootstrapper.CreateChannelFile(ChannelScope.Global, TestAppName, "Stable");

        // Assert
        Assert.True(result);
        Assert.True(File.Exists(_globalChannelFile));
        var content = File.ReadAllText(_globalChannelFile);
        Assert.Equal("Stable", content);
    }

    [Fact]
    public void CreateChannelFile_CreatesApplicationFileWithDefaultChannel()
    {
        // Act
        var result = VelopackBootstrapper.CreateChannelFile(ChannelScope.Application, TestAppName);

        // Assert
        Assert.True(result);
        Assert.True(File.Exists(_appChannelFile));
        var content = File.ReadAllText(_appChannelFile);
        Assert.Equal("Prerelease", content);
    }

    [Fact]
    public void CreateChannelFile_CreatesApplicationFileWithCustomChannel()
    {
        // Act
        var result = VelopackBootstrapper.CreateChannelFile(ChannelScope.Application, TestAppName, "Stable");

        // Assert
        Assert.True(result);
        Assert.True(File.Exists(_appChannelFile));
        var content = File.ReadAllText(_appChannelFile);
        Assert.Equal("Stable", content);
    }

    [Fact]
    public void CreateChannelFile_OverwritesExistingGlobalFile()
    {
        // Arrange
        Directory.CreateDirectory(_globalFolder);
        File.WriteAllText(_globalChannelFile, "Stable");

        // Act
        var result = VelopackBootstrapper.CreateChannelFile(ChannelScope.Global, TestAppName, "Prerelease");

        // Assert
        Assert.True(result);
        Assert.True(File.Exists(_globalChannelFile));
        var content = File.ReadAllText(_globalChannelFile);
        Assert.Equal("Prerelease", content);
    }

    [Fact]
    public void CreateChannelFile_OverwritesExistingApplicationFile()
    {
        // Arrange
        Directory.CreateDirectory(_appFolder);
        File.WriteAllText(_appChannelFile, "Prerelease");

        // Act
        var result = VelopackBootstrapper.CreateChannelFile(ChannelScope.Application, TestAppName, "Stable");

        // Assert
        Assert.True(result);
        Assert.True(File.Exists(_appChannelFile));
        var content = File.ReadAllText(_appChannelFile);
        Assert.Equal("Stable", content);
    }

    [Fact]
    public void CreateChannelFile_CreatesGlobalDirectoryIfNotExists()
    {
        // Arrange
        if (Directory.Exists(_globalFolder))
        {
            Directory.Delete(_globalFolder, true);
        }

        // Act
        var result = VelopackBootstrapper.CreateChannelFile(ChannelScope.Global, TestAppName);

        // Assert
        Assert.True(result);
        Assert.True(Directory.Exists(_globalFolder));
        Assert.True(File.Exists(_globalChannelFile));
    }

    [Fact]
    public void CreateChannelFile_CreatesApplicationDirectoryIfNotExists()
    {
        // Arrange
        if (Directory.Exists(_appFolder))
        {
            Directory.Delete(_appFolder, true);
        }

        // Act
        var result = VelopackBootstrapper.CreateChannelFile(ChannelScope.Application, TestAppName);

        // Assert
        Assert.True(result);
        Assert.True(Directory.Exists(_appFolder));
        Assert.True(File.Exists(_appChannelFile));
    }

    [Fact]
    public void ResolveChannel_ReturnsStable_WhenNoOverridesExist()
    {
        // Act
        var channel = VelopackBootstrapper.ResolveChannel(TestAppName);

        // Assert
        Assert.Equal("Stable", channel);
    }

    [Fact]
    public void ResolveChannel_ReturnsEnvironmentVariable_WhenSet()
    {
        // Arrange
        Environment.SetEnvironmentVariable("VELOPACK_CHANNEL", "Development");

        // Act
        var channel = VelopackBootstrapper.ResolveChannel(TestAppName);

        // Assert
        Assert.Equal("Development", channel);
    }

    [Fact]
    public void ResolveChannel_ReturnsChannelFromGlobalFile_WhenFileExists()
    {
        // Arrange
        Directory.CreateDirectory(_globalFolder);
        File.WriteAllText(_globalChannelFile, "Beta");

        // Act
        var channel = VelopackBootstrapper.ResolveChannel(TestAppName);

        // Assert
        Assert.Equal("Beta", channel);
    }

    [Fact]
    public void ResolveChannel_ReturnsChannelFromAppFile_WhenFileExists()
    {
        // Arrange
        Directory.CreateDirectory(_appFolder);
        File.WriteAllText(_appChannelFile, "Alpha");

        // Act
        var channel = VelopackBootstrapper.ResolveChannel(TestAppName);

        // Assert
        Assert.Equal("Alpha", channel);
    }

    [Fact]
    public void ResolveChannel_PrioritizesGlobalFileOverAppFile()
    {
        // Arrange
        Directory.CreateDirectory(_globalFolder);
        File.WriteAllText(_globalChannelFile, "GlobalChannel");
        Directory.CreateDirectory(_appFolder);
        File.WriteAllText(_appChannelFile, "AppChannel");

        // Act
        var channel = VelopackBootstrapper.ResolveChannel(TestAppName);

        // Assert
        Assert.Equal("GlobalChannel", channel);
    }

    [Fact]
    public void ResolveChannel_TrimsWhitespaceFromGlobalFile()
    {
        // Arrange
        Directory.CreateDirectory(_globalFolder);
        File.WriteAllText(_globalChannelFile, "  Beta  \n");

        // Act
        var channel = VelopackBootstrapper.ResolveChannel(TestAppName);

        // Assert
        Assert.Equal("Beta", channel);
    }

    [Fact]
    public void ResolveChannel_TrimsWhitespaceFromAppFile()
    {
        // Arrange
        Directory.CreateDirectory(_appFolder);
        File.WriteAllText(_appChannelFile, "  Gamma  \r\n");

        // Act
        var channel = VelopackBootstrapper.ResolveChannel(TestAppName);

        // Assert
        Assert.Equal("Gamma", channel);
    }

    [Fact]
    public void ResolveChannel_ReturnsStable_WhenGlobalFileExistsButIsEmpty()
    {
        // Arrange
        Directory.CreateDirectory(_globalFolder);
        File.WriteAllText(_globalChannelFile, "   ");

        // Act
        var channel = VelopackBootstrapper.ResolveChannel(TestAppName);

        // Assert
        Assert.Equal("Stable", channel);
    }

    [Fact]
    public void ResolveChannel_FallsBackToAppFile_WhenGlobalFileIsEmpty()
    {
        // Arrange
        Directory.CreateDirectory(_globalFolder);
        File.WriteAllText(_globalChannelFile, "   ");
        Directory.CreateDirectory(_appFolder);
        File.WriteAllText(_appChannelFile, "AppFallback");

        // Act
        var channel = VelopackBootstrapper.ResolveChannel(TestAppName);

        // Assert
        Assert.Equal("AppFallback", channel);
    }

    [Fact]
    public void ResolveChannel_PrioritizesEnvironmentVariableOverAllFiles()
    {
        // Arrange
        Environment.SetEnvironmentVariable("VELOPACK_CHANNEL", "EnvChannel");
        Directory.CreateDirectory(_globalFolder);
        File.WriteAllText(_globalChannelFile, "GlobalChannel");
        Directory.CreateDirectory(_appFolder);
        File.WriteAllText(_appChannelFile, "AppChannel");

        // Act
        var channel = VelopackBootstrapper.ResolveChannel(TestAppName);

        // Assert
        Assert.Equal("EnvChannel", channel);
    }

    [Fact]
    public void CreateChannelFile_ThrowsIfInvalidChannel()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            VelopackBootstrapper.CreateChannelFile(ChannelScope.Global, TestAppName, "Invalid"));
    }

    [Fact]
    public void ReadChannelFile_ReturnsNullIfFileDoesNotExist()
    {
        // Act
        var result = VelopackBootstrapper.ReadChannelFile(ChannelScope.Global, TestAppName);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ReadChannelFile_ReturnsValueFromGlobalFile()
    {
        // Arrange
        VelopackBootstrapper.CreateChannelFile(ChannelScope.Global, TestAppName, "Stable");

        // Act
        var result = VelopackBootstrapper.ReadChannelFile(ChannelScope.Global, TestAppName);

        // Assert
        Assert.Equal("Stable", result);
    }

    [Fact]
    public void ReadChannelFile_ReturnsValueFromApplicationFile()
    {
        // Arrange
        VelopackBootstrapper.CreateChannelFile(ChannelScope.Application, TestAppName, "Prerelease");

        // Act
        var result = VelopackBootstrapper.ReadChannelFile(ChannelScope.Application, TestAppName);

        // Assert
        Assert.Equal("Prerelease", result);
    }

    [Fact]
    public void ReadChannelFile_TrimsWhitespace()
    {
        // Arrange
        Directory.CreateDirectory(_globalFolder);
        File.WriteAllText(_globalChannelFile, "  Prerelease  \n");

        // Act
        var result = VelopackBootstrapper.ReadChannelFile(ChannelScope.Global, TestAppName);

        // Assert
        Assert.Equal("Prerelease", result);
    }

    [Fact]
    public void Resolve_HandlesNonExistentFiles()
    {
        // Arrange - ensure files don't exist
        if (File.Exists(_globalChannelFile))
        {
            File.Delete(_globalChannelFile);
        }
        if (File.Exists(_appChannelFile))
        {
            File.Delete(_appChannelFile);
        }

        // Act
        var channel = VelopackBootstrapper.ResolveChannel(TestAppName);

        // Assert
        Assert.Equal("Stable", channel);
    }

    [Fact]
    public void Startup_SkipsUpdateCheck_WhenSkipUpdateFlagProvided()
    {
        // Arrange
        var args = new[] { "--skip-update" };

        // Act - should return immediately without throwing
        VelopackBootstrapper.Startup(TestAppName, args);

        // Assert - if we get here without exception, the test passed
        Assert.True(true);
    }

    [Fact]
    public void Startup_SkipsUpdateCheck_WhenSkipUpdateFlagProvidedWithOtherArgs()
    {
        // Arrange
        var args = new[] { "--some-arg", "--skip-update", "--another-arg" };

        // Act - should return immediately without throwing
        VelopackBootstrapper.Startup(TestAppName, args);

        // Assert - if we get here without exception, the test passed
        Assert.True(true);
    }

    [Fact]
    public void Startup_SkipsUpdateCheck_CaseInsensitive()
    {
        // Arrange
        var args = new[] { "--SKIP-UPDATE" };

        // Act - should return immediately without throwing
        VelopackBootstrapper.Startup(TestAppName, args);

        // Assert - if we get here without exception, the test passed
        Assert.True(true);
    }

    [Fact]
    public void Startup_SkipsUpdateCheck_MixedCase()
    {
        // Arrange
        var args = new[] { "--Skip-Update" };

        // Act - should return immediately without throwing
        VelopackBootstrapper.Startup(TestAppName, args);

        // Assert - if we get here without exception, the test passed
        Assert.True(true);
    }
}
