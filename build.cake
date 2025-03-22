#nullable enable
#addin nuget:?package=Cake.Git&version=5.0.0

using System.Text.RegularExpressions;

// `target` indicates where to run through
// - e.g. dotnet dotnet-cake --target=Clean
var target        = Argument("target", "Test");
var configuration = Argument("configuration", "Release");
var slnPath       = "./src/DataImportUtility/DataImportUtility.sln";

var nugetApi      = "https://api.nuget.org/v3/index.json";
var nugetApiKey   = EnvironmentVariable("NUGET_API_KEY");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

record BuildData(
        string? Version
    );

Setup(ctx =>
    {
        var tip = GitLogTip(".");
        var tags = GitTags(".", true);
        var tipTag = tags
            .FirstOrDefault(tag => tag.Target.Sha == tip.Sha)
            ;
        string? version = null;

        if (tipTag is not null)
        {
            var tagName = tipTag.FriendlyName;
            var match   = Regex.Match(tagName, @"^v(?<version>\d+\.\d+\.\d+)$");
            if (match.Success)
            {
                version = match.Groups["version"].Value;
                Information($"Tip is tagged with version: {version}");
            }
            else
            {
                Warning($"Tip is tagged, but the tag doesn't match the version schema: {tagName}");
            }
        }
        else
        {
            Information("Tip is not tagged with version");
        }

        return new BuildData(version);
    });

// Use `--rebuild` to clean the solution
Task("Clean")
    .WithCriteria(c => HasArgument("rebuild"))
    .Does(() =>
{
    DotNetClean(slnPath, new()
    {
        Configuration = configuration,
    });
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetBuild(slnPath, new()
    {
        Configuration = configuration,
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetTest(slnPath, new()
    {
        Configuration = configuration,
        NoBuild       = true,
        NoRestore     = true,
    });
});

Task("Pack")
    .IsDependentOn("Test")
    .Does<BuildData>((ctx, bd) =>
{
    var bs = new DotNetMSBuildSettings()
        .SetVersion(bd.Version??"0.0.1")
        ;

    DotNetPack(slnPath, new()
    {
        Configuration   = configuration,
        NoBuild         = true,
        NoRestore       = true,
        MSBuildSettings = bs
    });
});

Task("PublishCoreLibraryToNuGet")
    .WithCriteria<BuildData>((ctx, bd) => bd.Version is not null)
    .IsDependentOn("Pack")
    .Does<BuildData>((ctx, bd) =>
{
    var packPath = $"./src/nupkg/DataImportUtility.{bd.Version}.nupkg";
    Information($"Publishing package: {packPath}");
    DotNetNuGetPush(packPath, new()
    {
            ApiKey = nugetApiKey,
            Source = nugetApi
    });
});

Task("PublishComponentLibraryToNuGet")
    .WithCriteria<BuildData>((ctx, bd) => bd.Version is not null)
    .IsDependentOn("Pack")
    .Does<BuildData>((ctx, bd) =>
{
    var packPath = $"./src/nupkg/DataImportUtility.Components.{bd.Version}.nupkg";
    Information($"Publishing package: {packPath}");
    DotNetNuGetPush(packPath, new()
    {
            ApiKey = nugetApiKey,
            Source = nugetApi
    });
});

Task("GithubAction")
    .IsDependentOn("PublishCoreLibraryToNuGet")
    .IsDependentOn("PublishComponentLibraryToNuGet")
    ;


//Task("GenerateReferenceAssets")
//    .Does<BuildData>((ctx, bd) =>
//{
//    (string,string)[] refs = [
//        ("dotnet-bot"     , "-i ../../assets/dotnet-bot_branded.png -s 100 -o ../../assets/dotnet-bot_branded.txt -oo")
//    ,   ("tool-icon"      , "-i ../../assets/icon.png -s 100 -o ../../assets/icon.txt -oo")
//    ,   ("cube-1s"        , "-i ../../assets/cube1s.gif -s 100 -o ../../assets/cube1s.txt -oo")
//    ,   ("cube-1s(IFRAME)", "-ei -i ../../assets/cube1s.gif -s 100 -o ../../assets/cube1s_.txt -oo")
//    ];
//    foreach (var (name, command) in refs)
//    {
//        Information($"Generating reference asset: {name}");
//        DotNetRun("./DataImportUtility.fsproj", command, new()
//        {
//            Configuration   = "Release"
//        ,   WorkingDirectory= "./src/DataImportUtility/"
//        });
//    }
//});


//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);