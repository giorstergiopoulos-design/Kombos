using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Kombos.Services
{
    public record SuiteApp(
        string Id, string Name, string Repo, string InstallerAsset, string? ChecksumAsset,
        string UninstallDisplayName, string[] UninstallAliases, string[] Paths,
        Dictionary<string, string> Tagline, Dictionary<string, string[]> Capabilities)
    {
        public string RepoUrl => $"https://github.com/{SuiteCatalog.Owner}/{Repo}";
        public string TaglineFor(string lang) => Tagline.TryGetValue(lang, out var t) ? t : Tagline.GetValueOrDefault("en", "");
        public string[] CapabilitiesFor(string lang) =>
            Capabilities.TryGetValue(lang, out var c) ? c : Capabilities.GetValueOrDefault("en", Array.Empty<string>());
    }

    public record SuiteCatalog(IReadOnlyList<SuiteApp> Apps)
    {
        public const string Owner = "giorstergiopoulos-design";
        public const long MaxBytes = 256 * 1024;
        public const string LiveUrl = "https://raw.githubusercontent.com/giorstergiopoulos-design/Opsis/master/suite/apps.json";

        private static readonly Regex IdRx = new("^[a-z0-9_-]{1,32}$", RegexOptions.Compiled);
        private static readonly Regex RepoRx = new("^[A-Za-z0-9._-]{1,100}$", RegexOptions.Compiled);

        /// <summary>Επιτρεπόμενοι host για ΟΛΑ τα αιτήματα καταλόγου/λήψεων: μόνο https προς GitHub.</summary>
        public static bool IsAllowedHost(Uri? u) =>
            u != null && u.Scheme == Uri.UriSchemeHttps &&
            (u.Host is "github.com" or "raw.githubusercontent.com" or "api.github.com"
             || u.Host.EndsWith(".githubusercontent.com", StringComparison.Ordinal));

        /// <summary>Αναλύει + επικυρώνει τον κατάλογο. Άγνωστα πεδία αγνοούνται· null + error αν είναι
        /// υπερβολικά μεγάλος, κακοσχηματισμένος ή δείχνει σε άλλον owner/άκυρο repo/regex.</summary>
        public static SuiteCatalog? TryParse(byte[] data, out string? error)
        {
            error = null;
            if (data.LongLength > MaxBytes) { error = "too-large"; return null; }
            try
            {
                using var doc = JsonDocument.Parse(data);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object) { error = "not-object"; return null; }
                if (!root.TryGetProperty("schema", out var sch) || sch.ValueKind != JsonValueKind.Number || sch.GetInt32() != 1) { error = "schema"; return null; }
                if (root.TryGetProperty("owner", out var ow) && ow.GetString() != Owner) { error = "owner"; return null; }
                if (!root.TryGetProperty("apps", out var arr) || arr.ValueKind != JsonValueKind.Array || arr.GetArrayLength() is 0 or > 40) { error = "apps"; return null; }

                var apps = new List<SuiteApp>();
                foreach (var a in arr.EnumerateArray())
                {
                    string Str(string n) => a.TryGetProperty(n, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : "";
                    var id = Str("id"); var name = Str("name"); var repo = Str("repo"); var rx = Str("installerAsset");
                    if (!IdRx.IsMatch(id) || name.Length is 0 or > 80 || !RepoRx.IsMatch(repo)) { error = "app-fields"; return null; }
                    try { _ = new Regex(rx, RegexOptions.None, TimeSpan.FromMilliseconds(100)); } catch { error = "regex"; return null; }
                    var checksum = Str("checksumAsset"); if (checksum.Length == 0) checksum = null;
                    if (checksum != null && (checksum.Contains('/') || checksum.Contains('\\'))) { error = "checksum-name"; return null; }

                    string[] StrArr(JsonElement el, string n) =>
                        el.TryGetProperty(n, out var v) && v.ValueKind == JsonValueKind.Array
                            ? v.EnumerateArray().Where(x => x.ValueKind == JsonValueKind.String).Select(x => x.GetString()!).Take(12).ToArray()
                            : Array.Empty<string>();

                    var tagline = new Dictionary<string, string>();
                    if (a.TryGetProperty("tagline", out var tl) && tl.ValueKind == JsonValueKind.Object)
                        foreach (var p in tl.EnumerateObject().Take(20)) if (p.Value.ValueKind == JsonValueKind.String) tagline[p.Name] = p.Value.GetString()!;
                    var caps = new Dictionary<string, string[]>();
                    if (a.TryGetProperty("capabilities", out var cp) && cp.ValueKind == JsonValueKind.Object)
                        foreach (var p in cp.EnumerateObject().Take(20)) if (p.Value.ValueKind == JsonValueKind.Array) caps[p.Name] = StrArr(cp, p.Name);
                    if (!tagline.ContainsKey("en") || !caps.ContainsKey("en")) { error = "missing-en"; return null; }

                    apps.Add(new SuiteApp(id, name, repo, rx, checksum, Str("uninstallDisplayName"), StrArr(a, "uninstallAliases"),
                        StrArr(a, "paths"), tagline, caps));
                }
                return new SuiteCatalog(apps);
            }
            catch (Exception ex) { error = "malformed: " + ex.GetType().Name; return null; }
        }
    }

    /// <summary>Φορτώνει τον κατάλογο: ενσωματωμένο αντίγραφο (fallback) → cache στο %AppData%\Kombos\suite →
    /// ζωντανό αρχείο το πολύ μία φορά το 24ωρο (timeout 5 s, σφάλματα αγνοούνται σιωπηλά).</summary>
    public static class SuiteCatalogLoader
    {
        public const string ResourceName = "Kombos.Data.suite-apps.json";

        public static string DefaultDir => Path.Combine(AppPaths.Root, "suite");

        public static SuiteCatalog LoadEmbedded()
        {
            using var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName)
                          ?? throw new InvalidOperationException("embedded suite catalogue missing");
            using var ms = new MemoryStream(); s.CopyTo(ms);
            return SuiteCatalog.TryParse(ms.ToArray(), out var err) ?? throw new InvalidOperationException("embedded suite catalogue invalid: " + err);
        }

        public static SuiteCatalog LoadCachedOrEmbedded(string? dir = null)
        {
            var file = Path.Combine(dir ?? DefaultDir, "apps.json");
            try
            {
                if (File.Exists(file))
                {
                    var cat = SuiteCatalog.TryParse(File.ReadAllBytes(file), out _);
                    if (cat != null) return cat;
                }
            }
            catch { }
            return LoadEmbedded();
        }

        /// <summary>Φέρνει το ζωντανό αρχείο αν το cache είναι παλαιότερο από 24 ώρες. Ποτέ δεν ρίχνει εξαίρεση.</summary>
        public static async Task<bool> RefreshAsync(HttpClient http, string? dir = null, string? url = null, CancellationToken ct = default)
        {
            try
            {
                dir ??= DefaultDir;
                var file = Path.Combine(dir, "apps.json");
                if (File.Exists(file) && DateTime.UtcNow - File.GetLastWriteTimeUtc(file) < TimeSpan.FromHours(24)) return false;

                var uri = new Uri(url ?? SuiteCatalog.LiveUrl);
                if (!SuiteCatalog.IsAllowedHost(uri)) return false;
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(TimeSpan.FromSeconds(5));
                using var req = new HttpRequestMessage(HttpMethod.Get, uri);
                req.Headers.UserAgent.ParseAdd("Kombos");
                using var resp = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cts.Token);
                if (!resp.IsSuccessStatusCode || !SuiteCatalog.IsAllowedHost(resp.RequestMessage?.RequestUri)) return false;
                if (resp.Content.Headers.ContentLength > SuiteCatalog.MaxBytes) return false;

                await using var stream = await resp.Content.ReadAsStreamAsync(cts.Token);
                using var ms = new MemoryStream();
                var buf = new byte[16 * 1024]; int n;
                while ((n = await stream.ReadAsync(buf, cts.Token)) > 0)
                {
                    ms.Write(buf, 0, n);
                    if (ms.Length > SuiteCatalog.MaxBytes) return false;
                }
                var data = ms.ToArray();
                if (SuiteCatalog.TryParse(data, out _) == null) return false;
                Directory.CreateDirectory(dir);
                File.WriteAllBytes(file, data);
                return true;
            }
            catch { return false; }
        }
    }
}
