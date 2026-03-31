using System.Diagnostics;

namespace DotnetPyRunner;

internal static class Program
{
    // PUBLIC_INTERFACE
    /// <summary>
    /// Runs the repository's hello_world.py using the system Python executable and prints stdout/stderr.
    /// Optional args:
    ///   --python <exe>   Python executable name/path (default: python3, then python fallback)
    ///   --script <path>  Path to python script (default: hello_world.py at repo root)
    /// </summary>
    public static int Main(string[] args)
    {
        string? pythonOverride = null;
        string? scriptOverride = null;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--python" && i + 1 < args.Length)
            {
                pythonOverride = args[++i];
            }
            else if (args[i] == "--script" && i + 1 < args.Length)
            {
                scriptOverride = args[++i];
            }
        }

        var repoRoot = FindRepoRoot(AppContext.BaseDirectory);
        if (repoRoot is null)
        {
            Console.Error.WriteLine("ERROR: Could not locate repository root (directory containing hello_world.py or .git).");
            return 2;
        }

        var scriptPath = scriptOverride ?? Path.Combine(repoRoot, "hello_world.py");
        if (!File.Exists(scriptPath))
        {
            Console.Error.WriteLine($"ERROR: Python script not found: {scriptPath}");
            return 3;
        }

        var pythonCandidates = pythonOverride is not null
            ? new[] { pythonOverride }
            : new[] { "python3", "python" };

        foreach (var py in pythonCandidates)
        {
            var (started, exitCode) = TryRunPython(py, scriptPath, repoRoot);
            if (started)
            {
                return exitCode;
            }
        }

        Console.Error.WriteLine("ERROR: Unable to start Python. Tried: " + string.Join(", ", pythonCandidates));
        Console.Error.WriteLine("Ensure Python is installed and available on PATH, or pass --python <path>.");
        return 4;
    }

    private static (bool started, int exitCode) TryRunPython(string pythonExe, string scriptPath, string workingDirectory)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = pythonExe,
                Arguments = Quote(scriptPath),
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var proc = new Process { StartInfo = psi };

            if (!proc.Start())
            {
                return (false, 1);
            }

            string stdout = proc.StandardOutput.ReadToEnd();
            string stderr = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            if (!string.IsNullOrEmpty(stdout))
            {
                Console.Write(stdout);
                if (!stdout.EndsWith(Environment.NewLine))
                {
                    Console.WriteLine();
                }
            }

            if (!string.IsNullOrEmpty(stderr))
            {
                Console.Error.Write(stderr);
                if (!stderr.EndsWith(Environment.NewLine))
                {
                    Console.Error.WriteLine();
                }
            }

            Console.WriteLine($"[dotnet_py_runner] Python executable: {pythonExe}");
            Console.WriteLine($"[dotnet_py_runner] Script: {scriptPath}");
            Console.WriteLine($"[dotnet_py_runner] Exit code: {proc.ExitCode}");

            return (true, proc.ExitCode);
        }
        catch
        {
            // Typically thrown when the executable is not found or can't be started.
            return (false, 1);
        }
    }

    private static string Quote(string value)
        => value.Contains(' ') ? $"\"{value}\"" : value;

    private static string? FindRepoRoot(string startDirectory)
    {
        var dir = new DirectoryInfo(startDirectory);

        // Walk up until we find either:
        // - hello_world.py (preferred) or
        // - a .git directory (fallback indicator of repo root)
        while (dir is not null)
        {
            var hello = Path.Combine(dir.FullName, "hello_world.py");
            var git = Path.Combine(dir.FullName, ".git");

            if (File.Exists(hello) || Directory.Exists(git))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        return null;
    }
}
