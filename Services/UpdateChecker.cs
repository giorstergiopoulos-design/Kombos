using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kombos.Services
{
    public record UpdateCheckResult(bool Success, bool UpdateAvailable, string? LatestVersion, string? ReleaseUrl, string? Error);

    /// <summary>Έλεγχος νέας έκδοσης (v4.5.0) - ΜΟΝΟ ένα HTTP GET στο δημόσιο GitHub Releases API όταν ο
    /// χρήστης το ζητήσει ρητά, ΠΟΤΕ αυτόματα στο παρασκήνιο και ΠΟΤΕ σιωπηλή λήψη/εγκατάσταση - απλή
    /// ειδοποίηση + link προς τη σελίδα του release, ο χρήστης κατεβάζει μόνος του το setup.exe αν θέλει.
    /// Χρειάζεται το repo να έχει ήδη δημόσια GitHub releases (owner/repo, π.χ. "log9/kombos") - αν το πεδίο
    /// είναι κενό, ο έλεγχος απλά δεν τρέχει.</summary>
    public static class UpdateChecker
    {
        private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(10) };

        public static async Task<UpdateCheckResult> CheckAsync(string repo, string currentVersion)
        {
            if (string.IsNullOrWhiteSpace(repo)) repo = SuiteCatalog.Owner + "/Kombos"; // προεπιλογή: το ίδιο το Kombos
            if (!repo.Contains('/'))
                return new UpdateCheckResult(false, false, null, null, "no-repo");

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.github.com/repos/{repo.Trim()}/releases/latest");
                request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Kombos", currentVersion));
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
                using var response = await Http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return new UpdateCheckResult(false, false, null, null, $"http-{(int)response.StatusCode}");

                using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                var tag = doc.RootElement.TryGetProperty("tag_name", out var t) ? t.GetString() ?? "" : "";
                var url = doc.RootElement.TryGetProperty("html_url", out var u) ? u.GetString() : null;
                var latest = tag.TrimStart('v', 'V');

                bool isNewer = VersionComparer.IsNewer(latest, currentVersion);
                return new UpdateCheckResult(true, isNewer, latest, url, null);
            }
            catch (Exception ex) { return new UpdateCheckResult(false, false, null, null, ex.Message); }
        }
    }
}
