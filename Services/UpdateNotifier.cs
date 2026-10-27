using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Kombos.Services
{
    public record ReleaseInfo(string Tag, string Version, string HtmlUrl);

    public class UpdateNoticeState
    {
        public string ETag { get; set; } = "";
        public string LatestTag { get; set; } = "";
        public string LatestUrl { get; set; } = "";
        /// <summary>Η τελευταία έκδοση για την οποία ο χρήστης ειδοποιήθηκε (μία φορά ανά έκδοση).</summary>
        public string NotifiedVersion { get; set; } = "";
        public DateTime LastCheckUtc { get; set; }
    }

    /// <summary>Αποθηκεύει ETag + "ποια έκδοση είπαμε ήδη στον χρήστη" σε ένα μικρό JSON (%AppData%\Kombos).</summary>
    public class UpdateNoticeStore
    {
        private readonly string _path;
        public UpdateNoticeStore(string? path = null) =>
            _path = path ?? Path.Combine(AppPaths.Root, "update-notice.json");

        public UpdateNoticeState Load()
        {
            try { if (File.Exists(_path)) return JsonSerializer.Deserialize<UpdateNoticeState>(File.ReadAllText(_path)) ?? new(); }
            catch { }
            return new();
        }

        public void Save(UpdateNoticeState s)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
                File.WriteAllText(_path, JsonSerializer.Serialize(s));
            }
            catch { }
        }
    }

    /// <summary>Αυτόματος, αθόρυβος έλεγχος νέας έκδοσης: ένα μόνο GET στο GitHub Releases API (ETag/If-None-Match,
    /// timeout 8 s, σφάλματα αγνοούνται, drafts/pre-releases αγνοούνται). Καμία τηλεμετρία.</summary>
    public class UpdateNotifier
    {
        private readonly HttpClient _http;
        private readonly UpdateNoticeStore _store;
        private readonly string _repo;
        private readonly string _current;

        public UpdateNotifier(HttpClient http, UpdateNoticeStore store, string repo, string currentVersion)
        {
            _http = http; _store = store; _repo = repo; _current = currentVersion;
        }

        /// <summary>Επιστρέφει την έκδοση ΜΟΝΟ αν είναι νεότερη ΚΑΙ δεν έχει ήδη ανακοινωθεί· αλλιώς null.</summary>
        public async Task<ReleaseInfo?> CheckAsync(CancellationToken ct = default)
        {
            try
            {
                var state = _store.Load();
                using var req = new HttpRequestMessage(HttpMethod.Get, $"https://api.github.com/repos/{_repo}/releases/latest");
                req.Headers.UserAgent.Add(new ProductInfoHeaderValue("Kombos", _current));
                req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
                if (!string.IsNullOrEmpty(state.ETag) && EntityTagHeaderValue.TryParse(state.ETag, out var etag))
                    req.Headers.IfNoneMatch.Add(etag);

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(TimeSpan.FromSeconds(8));
                using var resp = await _http.SendAsync(req, cts.Token);

                if (resp.StatusCode == HttpStatusCode.NotModified)
                {
                    // τίποτα νέο από την τελευταία φορά - χρησιμοποίησε ό,τι ξέρουμε ήδη
                }
                else if (resp.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(cts.Token));
                    var root = doc.RootElement;
                    bool draft = root.TryGetProperty("draft", out var d) && d.ValueKind == JsonValueKind.True;
                    bool pre = root.TryGetProperty("prerelease", out var p) && p.ValueKind == JsonValueKind.True;
                    var tag = root.TryGetProperty("tag_name", out var t) ? t.GetString() ?? "" : "";
                    var url = root.TryGetProperty("html_url", out var u) ? u.GetString() ?? "" : "";
                    if (draft || pre) return null;
                    state.LatestTag = tag; state.LatestUrl = url;
                    state.ETag = resp.Headers.ETag?.ToString() ?? "";
                }
                else return null;

                state.LastCheckUtc = DateTime.UtcNow;
                _store.Save(state);

                if (!VersionComparer.IsNewer(state.LatestTag, _current)) return null;
                var version = state.LatestTag.TrimStart('v', 'V');
                if (string.Equals(state.NotifiedVersion, version, StringComparison.OrdinalIgnoreCase)) return null;
                if (!IsAllowedReleaseUrl(state.LatestUrl)) return null;
                return new ReleaseInfo(state.LatestTag, version, state.LatestUrl);
            }
            catch { return null; }
        }

        /// <summary>Ο χρήστης ενημερώθηκε (ή απέρριψε) για αυτή την έκδοση - δεν ξαναεμφανίζεται.</summary>
        public void MarkNotified(string version)
        {
            var s = _store.Load();
            s.NotifiedVersion = version;
            _store.Save(s);
        }

        public static bool IsAllowedReleaseUrl(string url) =>
            Uri.TryCreate(url, UriKind.Absolute, out var u) && u.Scheme == Uri.UriSchemeHttps &&
            (u.Host == "github.com" || u.Host.EndsWith(".github.com", StringComparison.Ordinal));
    }
}
