using Bimlab.Nuke;
using Bimlab.Nuke.Components;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[GitHubActions(
    "WIP",
    GitHubActionsImage.UbuntuLatest,
    FetchDepth = 0,
    OnPushBranches = ["feature/**", "bugfix/**"],
    InvokedTargets = [nameof(Test), nameof(IPublish.Publish)],
    ImportSecrets = ["NUGET_API_KEY", "ALL_PACKAGES"])]
[GitHubActions(
    "Develop",
    GitHubActionsImage.UbuntuLatest,
    FetchDepth = 0,
    OnPushBranches = ["develop"],
    InvokedTargets = [nameof(Test)])]
[GitHubActions(
    "PreRelease",
    GitHubActionsImage.UbuntuLatest,
    FetchDepth = 0,
    OnPushBranches = ["release/**", "hotfix/**"],
    InvokedTargets = [nameof(Test), nameof(IPublish.Publish)],
    ImportSecrets = ["NUGET_API_KEY", "ALL_PACKAGES"])]
[GitHubActions(
    "Release",
    GitHubActionsImage.UbuntuLatest,
    FetchDepth = 0,
    OnPushBranches = ["master"],
    InvokedTargets = [nameof(Test), nameof(IPublish.Publish)],
    ImportSecrets = ["NUGET_API_KEY", "ALL_PACKAGES"])]
class Build : BimLabBuild, IPublish
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.From<ICompile>().Compile);

    public Target Test => definition => definition
        .Before<IRestore>()
        .Executes(() =>
        {
            DotNetTest(settings => settings
                .SetProjectFile(From<IHasSolution>().Solution.Path)
                .SetConfiguration(From<IHasConfiguration>().Configuration));
        });
}