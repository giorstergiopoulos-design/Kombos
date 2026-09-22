using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace Kombos.Services
{
    /// <summary>Nexus Marketplace (v4.5.0) - εγκατάσταση plugin από Git URL, ΧΩΡΙΣ να χρειάζεται git.exe
    /// εγκατεστημένο: κατεβάζει το ZIP snapshot του branch μέσω του δημόσιου GitHub codeload endpoint
    /// (δοκιμάζει "main" μετά "master"), το αποσυμπιέζει σε προσωρινό φάκελο, βρίσκει τα .dll που ήδη
    /// υπάρχουν μέσα (το Kombos ΔΕΝ μεταγλωττίζει πηγαίο κώδικα - το plugin πρέπει να έχει ήδη build output
    /// στο repo, ίδιος κανόνας με την υπάρχουσα "Λήψη &amp; Ενσωμάτωση" γκαλερί) και τα αντιγράφει στο
    /// PluginLoader.PluginsRoot - ίδιο μοντέλο εμπιστοσύνης με ένα χειροκίνητο αντίγραφο DLL, απλά πιο
    /// βολικό. Ο χρήστης δίνει ρητά το URL - το Kombos δεν κατεβάζει/εγκαθιστά ποτέ τίποτα μόνο του.</summary>
    public static class PluginMarketplace
    {
        private static readonly HttpClient Http = new();

        public static async Task<string> InstallFromGitHubAsync(string repoUrl)
        {
            var (owner, repo) = ParseGitHubUrl(repoUrl);
            byte[]? zipBytes = null;
            foreach (var branch in new[] { "main", "master" })
            {
                try
                {
                    zipBytes = await Http.GetByteArrayAsync($"https://codeload.github.com/{owner}/{repo}/zip/refs/heads/{branch}");
                    break;
                }
                catch { /* δοκίμασε το επόμενο branch */ }
            }
            if (zipBytes == null) throw new InvalidOperationException("Δεν βρέθηκε κλαδί main/master σε αυτό το repo.");

            var tempDir = Path.Combine(Path.GetTempPath(), "kombos-marketplace-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            var zipPath = Path.Combine(tempDir, "repo.zip");
            await File.WriteAllBytesAsync(zipPath, zipBytes);
            ZipFile.ExtractToDirectory(zipPath, tempDir);

            var dlls = Directory.GetFiles(tempDir, "*.dll", SearchOption.AllDirectories);
            if (dlls.Length == 0)
                throw new InvalidOperationException("Δεν βρέθηκαν .dll αρχεία στο repo - το Kombos δεν μεταγλωττίζει πηγαίο κώδικα, χρειάζεται ήδη-χτισμένο plugin.");

            PluginLoader.InstallFromDllFiles(repo, dlls);
            try { Directory.Delete(tempDir, recursive: true); } catch { /* καθαρισμός best-effort */ }
            return repo;
        }

        private static (string Owner, string Repo) ParseGitHubUrl(string url)
        {
            url = url.Trim().TrimEnd('/');
            if (url.EndsWith(".git")) url = url[..^4];
            var uri = new Uri(url);
            var segments = uri.AbsolutePath.Trim('/').Split('/');
            if (segments.Length < 2) throw new InvalidOperationException("Αναμένεται URL μορφής https://github.com/owner/repo");
            return (segments[0], segments[1]);
        }
    }
}
