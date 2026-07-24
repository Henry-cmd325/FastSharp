using System.Diagnostics;
using Xunit;

namespace FastSharp.Tests;

public class TemplateTestFixture : IDisposable
{
    public string TempArtifactsDir { get; }
    public string RepoRootDir { get; }
    public string Version { get; } = $"1.0.0-test-{Guid.NewGuid():N}";

    public TemplateTestFixture()
    {
        RepoRootDir = FindRepoRootDir();
        TempArtifactsDir = Path.Combine(Path.GetTempPath(), "FastSharpTempArtifacts_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(TempArtifactsDir);

        // 1. Pack Models and Modules with test version
        RunCommand("dotnet", $"pack FastSharp.Models/FastSharp.Models.csproj -c Release -o \"{TempArtifactsDir}\" /p:Version={Version}", RepoRootDir);
        RunCommand("dotnet", $"pack FastSharp.Modules/FastSharp.Modules.csproj -c Release -o \"{TempArtifactsDir}\" /p:Version={Version}", RepoRootDir);

        // 2. Temporarily rewrite FastSharpApi.csproj to reference the test version, then pack the templates
        var templateCsprojPath = Path.Combine(RepoRootDir, "FastSharp.Templates", "content", "FastSharp.Template.Api", "FastSharpApi.csproj");
        var originalCsprojContent = File.ReadAllText(templateCsprojPath);
        var testCsprojContent = originalCsprojContent.Replace("1.0.0-beta.11", Version);
        File.WriteAllText(templateCsprojPath, testCsprojContent);
        
        try
        {
            RunCommand("dotnet", $"pack FastSharp.Templates/FastSharp.Templates.csproj -c Release -o \"{TempArtifactsDir}\" /p:Version={Version}", RepoRootDir);
        }
        finally
        {
            File.WriteAllText(templateCsprojPath, originalCsprojContent);
        }

        // 3. Uninstall any pre-existing templates
        try
        {
            RunCommand("dotnet", "new uninstall FastSharp.Templates", RepoRootDir);
        }
        catch
        {
            // Ignore error if it was not installed
        }

        // 4. Install the generated test template
        var templateNupkg = Path.Combine(TempArtifactsDir, $"FastSharp.Templates.{Version}.nupkg");
        RunCommand("dotnet", $"new install \"{templateNupkg}\"", RepoRootDir);
    }

    public void Dispose()
    {
        // 1. Uninstall test template
        try
        {
            RunCommand("dotnet", "new uninstall FastSharp.Templates", RepoRootDir);
        }
        catch { }

        // 2. Clean up temporary artifacts directory
        try
        {
            if (Directory.Exists(TempArtifactsDir))
            {
                Directory.Delete(TempArtifactsDir, true);
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

    public static void RunCommand(string fileName, string arguments, string workingDirectory)
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
                _fixture.RepoRootDir
            );

            // 2. Build the project using the temporary artifacts directory as an additional NuGet source
            var csprojFile = Path.Combine(testOutputDir, $"{projectName}.csproj");
            TemplateTestFixture.RunCommand(
                "dotnet", 
                $"build \"{csprojFile}\" /p:RestoreAdditionalProjectSources=\"{_fixture.TempArtifactsDir}\"", 
                _fixture.RepoRootDir
            );
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
