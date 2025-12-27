using System.Runtime.CompilerServices;
using Velopack;
using Velopack.Sources;

[assembly: InternalsVisibleTo("Automation.Velopack.UnitTests")]

namespace Automation.Velopack;

public static class VelopackBootstrapper
{
    internal static readonly string SharedDeptFolder = "Automation";

    internal static string ResolveChannel(string appName)
    {
        // 1) Env var override (CI/testing friendly)
        var env = Environment.GetEnvironmentVariable("VELOPACK_CHANNEL");
        if (!string.IsNullOrWhiteSpace(env)) return env;

        // 2) Command line (allow a shortcut like: MyApp.exe --channel=Prerelease)
        var args = Environment.GetCommandLineArgs().Skip(1);
        foreach (var arg in args)
        {
            if (arg.StartsWith("--channel=", StringComparison.OrdinalIgnoreCase))
            {
                return arg.Split('=')[1];
            }
        }

        // 3) Marker file (user opt-in): %LOCALAPPDATA%\YourApp\.channel (contents: Stable/Prerelease)
        var globalChannelFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            SharedDeptFolder, ".channel");
        if (File.Exists(globalChannelFile))
        {
            var ch = File.ReadAllText(globalChannelFile).Trim();
            if (!string.IsNullOrEmpty(ch)) return ch;
        }

        // 4) Marker file (user opt-in): %LOCALAPPDATA%\YourApp\.channel (contents: Stable/Prerelease)
        var applicationChannelFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            appName, ".channel");
        if (File.Exists(applicationChannelFile))
        {
            var ch = File.ReadAllText(applicationChannelFile).Trim();
            if (!string.IsNullOrEmpty(ch)) return ch;
        }

        // default
        return "Stable";
    }

    public static void Startup(string applicationName, string[]? args = null)
    {
        string logStep = "Application.Startup() begin";
        // This does not work from here. must be called directly from startup.
        //VelopackApp.Build().Run();
        try
        {
            // Check for --skip-update flag
            args ??= Environment.GetCommandLineArgs().Skip(1).ToArray();
            if (args.Any(a => a.Equals("--skip-update", StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            var channel = ResolveChannel(applicationName); // "Stable" or "Prerelease"

            logStep = "Create Instance UpdateManager()";
            var sourceUrl =
                "https://staftrinstallers.blob.core.windows.net/installers/?sp=rl&st=2025-11-15T07:03:42Z&se=2125-11-15T15:18:42Z&spr=https&sv=2024-11-04&sr=c&sig=HC1aJk%2B%2FiebSo61QYQ4zMEAyezFGmUqW9ajOSs31MWI%3D";
            var source = new SimpleWebSource(sourceUrl);
            var checkOptions = new UpdateOptions
            {
                ExplicitChannel = $"{applicationName}-{channel}-win"
            };
            var mgr = new UpdateManager(source, checkOptions);

            // check for new version
            logStep = "Check for new version";
            var newVersion = mgr.CheckForUpdatesAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            logStep = $"newVersion = {newVersion}";
            if (newVersion != null)
            {
                // download new version
                logStep = "Download Updates...";
                mgr.DownloadUpdatesAsync(newVersion).ConfigureAwait(false).GetAwaiter().GetResult();

                // install new version and restart app
                logStep = "Apply Updates and Restart...";
                mgr.ApplyUpdatesAndRestart(newVersion);
            }
        }
        catch (Exception ex)
        {
            try
            {
                if (!Directory.Exists("C:\\Temp"))
                {
                    Directory.CreateDirectory("C:\\Temp");
                }

                File.AppendAllText($"C:\\Temp\\{applicationName}-Log.txt",
                    $"Step: {logStep} Exception: {ex.Message} {Environment.NewLine} StackTrace: {ex.StackTrace}");
            }
            catch
            {
                // Intentionally left blank
            }

            Console.WriteLine(ex.Message);
        }
    }

    public static bool CreateChannelFile(ChannelScope scope, string appName, string channel = "Prerelease")
    {
        if (channel != "Stable" && channel != "Prerelease")
            throw new ArgumentException("Channel must be either 'Stable' or 'Prerelease'.", nameof(channel));

        var pathScope = scope == ChannelScope.Global ? SharedDeptFolder : appName;
        var channelFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            pathScope, ".channel");
        var containingFolder = Path.GetDirectoryName(channelFile);
        if (containingFolder == null)
            return false;

        if (!Directory.Exists(containingFolder))
            Directory.CreateDirectory(containingFolder);
        if (File.Exists(channelFile)) File.Delete(channelFile);
        File.WriteAllText(channelFile, channel);
        return true;
    }

    public static string? ReadChannelFile(ChannelScope scope, string appName)
    {
        var pathScope = scope == ChannelScope.Global ? SharedDeptFolder : appName;
        var channelFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            pathScope, ".channel");

        if (File.Exists(channelFile))
        {
            var ch = File.ReadAllText(channelFile).Trim();
            if (!string.IsNullOrEmpty(ch)) return ch;
        }

        return null;
    }
}