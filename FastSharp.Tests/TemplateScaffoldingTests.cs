using System.Diagnostics;
using System.IO.Compression;
using System.Security;
using Xunit;

namespace FastSharp.Tests;

public class TemplateTestFixture : IDisposable
{
    public string TempRootDir { get; }
    public string TempArtifactsDir { get; }
    public string TempSourceDir { get; }
    public string DotnetCliHomeDir { get; }
    public string NugetPackagesDir { get; }
    public string RepoRootDir { get; }
    public string Version { get; } = "1.0.0-test-source";
    public string PackageVersion { get; } = "1.0.0-test-package";
    public IReadOnlyDictionary<string, string> EnvironmentVariables { get; }

    public TemplateTestFixture()
    {
        RepoRootDir = FindRepoRootDir();
        TempRootDir = Path.Combine(Path.GetTempPath(), "FastSharpTemplateTests_" + Guid.NewGuid().ToString("N"));
        TempArtifactsDir = Path.Combine(TempRootDir, "artifacts");
        TempSourceDir = Path.Combine(TempRootDir, "source");
        DotnetCliHomeDir = Path.Combine(TempRootDir, "dotnet-cli-home");
        NugetPackagesDir = Path.Combine(TempRootDir, "nuget-packages");
        Directory.CreateDirectory(TempRootDir);
        Directory.CreateDirectory(TempArtifactsDir);
        Directory.CreateDirectory(TempSourceDir);
        Directory.CreateDirectory(DotnetCliHomeDir);
        Directory.CreateDirectory(NugetPackagesDir);
        EnvironmentVariables = new Dictionary<string, string>
        {
            ["DOTNET_CLI_HOME"] = DotnetCliHomeDir,
            ["NUGET_PACKAGES"] = NugetPackagesDir,
            ["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1"
        };

        CopyPackSources();

        // Pack private source copies so tests never modify tracked files or shared build output.
        var versionProperties = $"/p:Version={Version} /p:PackageVersion={PackageVersion}";
        RunCommand("dotnet", $"pack FastSharp.Models/FastSharp.Models.csproj -c Release -o \"{TempArtifactsDir}\" {versionProperties}", TempSourceDir, EnvironmentVariables);
        RunCommand("dotnet", $"pack FastSharp.Modules/FastSharp.Modules.csproj -c Release -o \"{TempArtifactsDir}\" {versionProperties}", TempSourceDir, EnvironmentVariables);

        RunCommand("dotnet", $"pack FastSharp.Templates/FastSharp.Templates.csproj -c Release -o \"{TempArtifactsDir}\" {versionProperties}", TempSourceDir, EnvironmentVariables);
        VerifyPackedPackageVersions();

        // The fixture-specific DOTNET_CLI_HOME isolates template installation from other tests and users.
        var templateNupkg = Path.Combine(TempArtifactsDir, $"FastSharp.Templates.{PackageVersion}.nupkg");
        RunCommand("dotnet", $"new install \"{templateNupkg}\"", TempSourceDir, EnvironmentVariables);
    }

    public void Dispose()
    {
        // Template state and all generated packages live under the fixture's private root.
        try
        {
            if (Directory.Exists(TempRootDir))
            {
                Directory.Delete(TempRootDir, true);
            }
        }
        catch { }
    }

    private string FindRepoRootDir()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "FastSharp.slnx")))
        {
            dir = dir.Parent;
        }
        return dir?.FullName ?? throw new InvalidOperationException("Could not find repository root containing FastSharp.slnx");
    }

    private void CopyPackSources()
    {
        foreach (var directoryName in new[] { "FastSharp.Models", "FastSharp.Generators", "FastSharp.Modules", "FastSharp.Templates" })
        {
            CopyDirectory(Path.Combine(RepoRootDir, directoryName), Path.Combine(TempSourceDir, directoryName));
        }

        foreach (var fileName in new[] { "README.md", "LICENSE", "Gemini_Generated_Image_52oeva52oeva52oe.png" })
        {
            File.Copy(Path.Combine(RepoRootDir, fileName), Path.Combine(TempSourceDir, fileName));
        }
    }

    private void VerifyPackedPackageVersions()
    {
        var modulesPackagePath = Path.Combine(TempArtifactsDir, $"FastSharp.Modules.{PackageVersion}.nupkg");
        using var modulesPackage = ZipFile.OpenRead(modulesPackagePath);
        var nuspec = modulesPackage.GetEntry("FastSharp.Modules.nuspec")
            ?? throw new InvalidOperationException("The packed FastSharp.Modules nuspec is missing.");
        using var reader = new StreamReader(nuspec.Open());
        var nuspecContent = reader.ReadToEnd();

        if (!nuspecContent.Contains("FastSharp.Models", StringComparison.Ordinal) ||
            !nuspecContent.Contains(PackageVersion, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The packed FastSharp.Modules package does not depend on the fixture's FastSharp.Models version.");
        }

        var templatesPackagePath = Path.Combine(TempArtifactsDir, $"FastSharp.Templates.{PackageVersion}.nupkg");
        var sourceVersionPackagePath = Path.Combine(TempArtifactsDir, $"FastSharp.Templates.{Version}.nupkg");
        if (!File.Exists(templatesPackagePath) || File.Exists(sourceVersionPackagePath))
        {
            throw new InvalidOperationException("The packed template package does not use the fixture's PackageVersion.");
        }

        using var templatesPackage = ZipFile.OpenRead(templatesPackagePath);
        var templateNuspec = templatesPackage.GetEntry("FastSharp.Templates.nuspec")
            ?? throw new InvalidOperationException("The packed template nuspec is missing.");
        using var templateNuspecReader = new StreamReader(templateNuspec.Open());
        var templateNuspecContent = templateNuspecReader.ReadToEnd();
        if (!templateNuspecContent.Contains($"<version>{PackageVersion}</version>", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The packed template nuspec does not use the fixture's PackageVersion.");
        }

        var templateProject = templatesPackage.GetEntry("content/content/FastSharp.Template.Api/FastSharpApi.csproj")
            ?? throw new InvalidOperationException("The packed template project is missing.");
        using var templateReader = new StreamReader(templateProject.Open());
        var templateProjectContent = templateReader.ReadToEnd();

        if (!templateProjectContent.Contains($"<PackageReference Include=\"FastSharp.Modules\" Version=\"{PackageVersion}\" />", StringComparison.Ordinal) ||
            templateProjectContent.Contains("__FASTSHARP_MODULES_VERSION__", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The packed template does not contain the fixture's FastSharp.Modules version.");
        }
    }

    private static void CopyDirectory(string sourceDirectory, string destinationDirectory)
    {
        Directory.CreateDirectory(destinationDirectory);
        foreach (var file in Directory.EnumerateFiles(sourceDirectory))
        {
            File.Copy(file, Path.Combine(destinationDirectory, Path.GetFileName(file)));
        }

        foreach (var directory in Directory.EnumerateDirectories(sourceDirectory))
        {
            var directoryName = Path.GetFileName(directory);
            if (directoryName is "bin" or "obj")
            {
                continue;
            }

            CopyDirectory(directory, Path.Combine(destinationDirectory, directoryName));
        }
    }

    public static void RunCommand(string fileName, string arguments, string workingDirectory, IReadOnlyDictionary<string, string>? environmentVariables = null)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        if (environmentVariables is not null)
        {
            foreach (var environmentVariable in environmentVariables)
            {
                process.StartInfo.Environment[environmentVariable.Key] = environmentVariable.Value;
            }
        }

        process.Start();
        
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();

        if (!process.WaitForExit(TimeSpan.FromMinutes(2)))
        {
            process.Kill();
            throw new TimeoutException($"Command '{fileName} {arguments}' timed out.");
        }

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Command '{fileName} {arguments}' failed with exit code {process.ExitCode}.\nOutput: {outputTask.Result}\nError: {errorTask.Result}");
        }
    }
}

public class TemplateScaffoldingTests : IClassFixture<TemplateTestFixture>
{
    private readonly TemplateTestFixture _fixture;

    public TemplateScaffoldingTests(TemplateTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData("InMemory", true)]
    [InlineData("InMemory", false)]
    [InlineData("SqlServer", true)]
    [InlineData("Postgres", true)]
    [InlineData("MySql", true)]
    public void ScaffoldedProject_BuildsSuccessfully(string databaseProvider, bool enableSwagger)
    {
        var testOutputDir = Path.Combine(Path.GetTempPath(), $"FastSharpTestProject_{databaseProvider}_Sw{enableSwagger}_{Guid.NewGuid().ToString("N")}");
        Directory.CreateDirectory(testOutputDir);

        try
        {
            var projectName = $"TestApp_{databaseProvider}";
            
            // 1. Scaffold project
            TemplateTestFixture.RunCommand(
                "dotnet", 
                $"new fastsharp-api -n \"{projectName}\" --Database \"{databaseProvider}\" --EnableSwagger {enableSwagger.ToString().ToLower()} -o \"{testOutputDir}\"", 
                _fixture.TempSourceDir,
                _fixture.EnvironmentVariables
            );

            // Restore against the private packages directory. A unique version guarantees FastSharp packages
            // must come from this fixture's artifacts rather than a published or global-cache package.
            var csprojFile = Path.Combine(testOutputDir, $"{projectName}.csproj");
            var nugetConfigPath = Path.Combine(testOutputDir, "NuGet.Config");
            File.WriteAllText(nugetConfigPath, $"""
                <?xml version="1.0" encoding="utf-8"?>
                <configuration>
                  <packageSources>
                    <clear />
                    <add key="fixture-artifacts" value="{SecurityElement.Escape(_fixture.TempArtifactsDir)}" />
                    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
                  </packageSources>
                </configuration>
                """);
            TemplateTestFixture.RunCommand(
                "dotnet", 
                $"restore \"{csprojFile}\" --configfile \"{nugetConfigPath}\"",
                _fixture.TempSourceDir,
                _fixture.EnvironmentVariables
            );
            TemplateTestFixture.RunCommand(
                "dotnet",
                $"build \"{csprojFile}\" --no-restore",
                _fixture.TempSourceDir,
                _fixture.EnvironmentVariables
            );

            var assetsFile = Path.Combine(testOutputDir, "obj", "project.assets.json");
            var assetsContent = File.ReadAllText(assetsFile);
            Assert.Contains($"FastSharp.Modules/{_fixture.PackageVersion}", assetsContent, StringComparison.Ordinal);
        }
        finally
        {
            // Cleanup generated project folder
            try
            {
                if (Directory.Exists(testOutputDir))
                {
                    Directory.Delete(testOutputDir, true);
                }
            }
            catch { }
        }
    }
}
