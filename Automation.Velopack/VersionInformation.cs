using System.Reflection;

namespace Automation.Velopack;

public static class VersionInformation
{
    public static string FullVersion =>
        Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? "Unknown";

    public static string Version
    {
        get
        {
            var full = FullVersion;
            var plusIndex = full.IndexOf('+');
            return plusIndex > 0 ? full.Substring(0, plusIndex) : full;
        }
    }
}
