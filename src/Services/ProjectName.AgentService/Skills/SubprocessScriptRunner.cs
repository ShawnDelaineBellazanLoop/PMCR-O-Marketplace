// ═══════════════════════════════════════════════════════════════════════════════
// PMCR-O — AgentService
// File       : Skills/SubprocessScriptRunner.cs
// Identity   : External interpreter dispatcher for file-based agent skills
// ThoughtLock: 2026-05-30
// ═══════════════════════════════════════════════════════════════════════════════

#pragma warning disable MAESKILLS001

using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Agents.AI;

namespace ProjectName.AgentService.Skills;

/// <summary>
/// Runs file-based skill scripts (.py, .js, .sh, .ps1) in an external interpreter subprocess.
/// Pass <see cref="RunAsync"/> wherever an <see cref="AgentFileSkillScriptRunner"/> delegate is required.
/// </summary>
internal static class SubprocessScriptRunner
{
    private static readonly Dictionary<string, (string Exe, string Flags)> s_interpreters =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [".py"]  = ("python3", ""),
            [".js"]  = ("node",    ""),
            [".sh"]  = ("bash",    ""),
            [".ps1"] = ("pwsh",    "-NonInteractive -File"),
        };

    public static async Task<object?> RunAsync(
        AgentFileSkill skill,
        AgentFileSkillScript script,
        JsonElement? arguments,
        IServiceProvider? serviceProvider,
        CancellationToken cancellationToken)
    {
        var ext = Path.GetExtension(script.FullPath);

        if (!s_interpreters.TryGetValue(ext, out var interpreter))
            throw new InvalidOperationException(
                $"SubprocessScriptRunner: unsupported extension '{ext}' for '{script.Name}'. " +
                $"Supported: {string.Join(", ", s_interpreters.Keys)}.");

        string[] cliArgs = arguments.HasValue && arguments.Value.ValueKind == JsonValueKind.Array
            ? JsonSerializer.Deserialize<string[]>(arguments.Value) ?? []
            : [];

        var argsBuilder = new StringBuilder();
        if (!string.IsNullOrEmpty(interpreter.Flags))
            argsBuilder.Append(interpreter.Flags).Append(' ');
        argsBuilder.Append(Quote(script.FullPath));
        foreach (var arg in cliArgs)
            argsBuilder.Append(' ').Append(Quote(arg));

        var psi = new ProcessStartInfo
        {
            FileName               = interpreter.Exe,
            Arguments              = argsBuilder.ToString(),
            WorkingDirectory       = skill.Path,
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false,
            CreateNoWindow         = true,
        };

        using var process = new Process { StartInfo = psi, EnableRaisingEvents = true };
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        process.OutputDataReceived += (_, e) => { if (e.Data is not null) stdout.AppendLine(e.Data); };
        process.ErrorDataReceived  += (_, e) => { if (e.Data is not null) stderr.AppendLine(e.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            try { process.Kill(entireProcessTree: true); } catch { /* already exited */ }
            throw;
        }

        if (process.ExitCode != 0)
            throw new InvalidOperationException(
                $"Script '{script.Name}' exited with code {process.ExitCode}. Stderr: {stderr.ToString().TrimEnd()}");

        return stdout.ToString().TrimEnd();
    }

    private static string Quote(string value)
        => $"\"{value.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
}
