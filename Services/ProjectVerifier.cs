using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Kombos.Services
{
    public class VerifyResult
    {
        public bool Ran { get; set; }
        public bool Success { get; set; }
        public string Command { get; set; } = "";
        public string Output { get; set; } = "";
    }

    /// <summary>Τρέχει ΤΟΠΙΚΑ, με μηδενικό κόστος tokens, το δικό του build/lint toolchain ενός project
    /// φακέλου (αυτόματη ανίχνευση από τα αρχεία που υπάρχουν) - ένας deterministic compiler/linter έλεγχος
    /// πιάνει ασυμβατότητες πολύ πιο αξιόπιστα από το να ζητηθεί από ένα μοντέλο να "ελέγξει" τον δικό του
    /// κώδικα. Σκοπίμως ΔΕΝ τρέχει σε αυθαίρετο φάκελο - μόνο στο ρητά εισηγμένο project root.</summary>
    public static class ProjectVerifier
    {
        public static async Task<VerifyResult> VerifyAsync(string projectRoot)
        {
            string? fileName = null, args = null, command = null;

            if (Directory.GetFiles(projectRoot, "*.sln", SearchOption.TopDirectoryOnly).Length > 0 ||
                Directory.GetFiles(projectRoot, "*.csproj", SearchOption.AllDirectories).Length > 0)
            {
                fileName = "dotnet"; args = "build -v quiet"; command = "dotnet build -v quiet";
            }
            else if (File.Exists(Path.Combine(projectRoot, "package.json")))
            {
                fileName = "npm"; args = "run build --if-present"; command = "npm run build";
            }
            else if (File.Exists(Path.Combine(projectRoot, "tsconfig.json")))
            {
                fileName = "npx"; args = "tsc --noEmit"; command = "npx tsc --noEmit";
            }

            if (fileName == null)
                return new VerifyResult { Ran = false, Output = "Δεν αναγνωρίστηκε γνωστό toolchain (.csproj/.sln, package.json, tsconfig.json) σε αυτό το project." };

            try
            {
                var psi = new ProcessStartInfo(fileName, args!)
                {
                    WorkingDirectory = projectRoot,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi)!;
                var stdout = await proc.StandardOutput.ReadToEndAsync();
                var stderr = await proc.StandardError.ReadToEndAsync();
                await proc.WaitForExitAsync();

                var combined = (stdout + "\n" + stderr).Trim();
                const int maxChars = 3000;
                if (combined.Length > maxChars) combined = combined[..maxChars] + "\n…";

                return new VerifyResult { Ran = true, Success = proc.ExitCode == 0, Command = command!, Output = combined };
            }
            catch (Exception ex)
            {
                return new VerifyResult { Ran = false, Command = command!, Output = $"Δεν ήταν δυνατή η εκτέλεση '{command}': {ex.Message}" };
            }
        }
    }
}
