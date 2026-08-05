using Fallout.Common;
using Fallout.Solutions;
using Automation.Fallout.Components;
using Automation.Fallout.Components.Components;
using Automation.Fallout.Components.DefaultBuilds;
using Automation.Fallout.Components.Parameters;

/// <summary>
/// Build configuration for PackageBuild
/// </summary>
/// Support plugins are available for:
///   - JetBrains ReSharper        https://nuke.build/resharper
///   - JetBrains Rider            https://nuke.build/rider
///   - Microsoft VisualStudio     https://nuke.build/visualstudio
///   - Microsoft VSCode           https://nuke.build/vscode

public class Build : GitHubActionsBuild, IShowVersion, IClean, ICompile, IRestore, IScanForSecrets, IRunUnitTests, IRunIntegrationTests, IGenerateCoverageReport, ITest, IUpdateChangelog, IPackageGitHub, ITagRelease, IAnnounceRelease
{

    public static int Main() => Execute<Build>(
        x => ((IPackageGitHub)x).ReleasePackage);

    int IHasTests.MinCoverageThreshold => 40;
    string IHasGitHubPackages.GitHubOwner => "meddlingidiot";
}
