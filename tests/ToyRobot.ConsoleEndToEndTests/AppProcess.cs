using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace ToyRobot.ConsoleEndToEndTests;

public sealed record AppResult(string Stdout, string Stderr, int ExitCode);

/// <summary>
/// Starts the built application as a separate process, feeds it console input bytes and collects
/// console output (stdout), console messages (stderr) and the exit code (NFR-06, DEC-04, DEC-31 level 3).
/// </summary>
public static class AppProcess
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

    public static AppResult Run(byte[] stdin, params string[] args)
    {
        var startInfo = new ProcessStartInfo(FindExecutable())
        {
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = new UTF8Encoding(false),
            StandardErrorEncoding = new UTF8Encoding(false),
        };
        foreach (var arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();

        try
        {
            process.StandardInput.BaseStream.Write(stdin, 0, stdin.Length);
            process.StandardInput.BaseStream.Flush();
            process.StandardInput.Close();
        }
        catch (IOException)
        {
            // The application exits without reading when it is given an argument (E2E-08).
        }

        if (!process.WaitForExit((int)Timeout.TotalMilliseconds))
        {
            process.Kill(entireProcessTree: true);
            Assert.Fail($"The application did not finish within {Timeout.TotalSeconds} seconds and was stopped.");
        }

        // WaitForExit with no argument makes sure the asynchronous reads are complete.
        process.WaitForExit();
        return new AppResult(stdoutTask.GetAwaiter().GetResult(), stderrTask.GetAwaiter().GetResult(), process.ExitCode);
    }

    public static byte[] Utf8(params string[] lines)
    {
        var text = lines.Length == 0 ? string.Empty : string.Join("\n", lines) + "\n";
        return new UTF8Encoding(false).GetBytes(text);
    }

    private static string FindExecutable()
    {
        var root = FindRepositoryRoot();
        var configuration = typeof(AppProcess).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration ?? "Debug";
        var fileName = OperatingSystem.IsWindows() ? "ToyRobot.exe" : "ToyRobot";
        var path = Path.Combine(root, "src", "ToyRobot", "bin", configuration, "net8.0", fileName);
        if (!File.Exists(path))
        {
            Assert.Fail($"The built application was not found at '{path}'. Build the solution first.");
        }

        return path;
    }

    public static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "ToyRobot.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        Assert.Fail("The repository root (the folder holding ToyRobot.sln) was not found.");
        return string.Empty;
    }
}
