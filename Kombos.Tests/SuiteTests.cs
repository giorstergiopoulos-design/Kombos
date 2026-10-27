using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kombos.Services;
using Xunit;

namespace Kombos.Tests
{
    internal class FakeHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, HttpResponseMessage> Respond = _ => new HttpResponseMessage(HttpStatusCode.NotFound);
        public List<HttpRequestMessage> Requests { get; } = new();
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Requests.Add(request);
            var r = Respond(request);
            r.RequestMessage ??= request;
            return Task.FromResult(r);
        }
    }

    public class VersionComparerTests
    {
        [Theory]
        [InlineData("v4.9.0", "4.8.2", true)]
        [InlineData("4.8.10", "4.8.9", true)]       // αριθμητικά, όχι αλφαβητικά
        [InlineData("v5.0", "4.99.99", true)]
        [InlineData("4.8.2", "4.8.2", false)]
        [InlineData("4.8.1", "4.8.2", false)]
        [InlineData("v4.9.0-beta.1", "4.8.2", false)] // pre-release δεν ειδοποιεί
        [InlineData("garbage", "4.8.2", false)]
        [InlineData("4.9.0", "4.9.0-rc.1", true)]     // σταθερή > pre-release του ίδιου πυρήνα
        public void IsNewer(string latest, string current, bool expected) =>
            Assert.Equal(expected, VersionComparer.IsNewer(latest, current));

        [Fact]
        public void CompareIsSemantic()
        {
            Assert.True(VersionComparer.Compare("1.10.0", "1.9.0") > 0);
            Assert.Equal(0, VersionComparer.Compare("v1.2.3", "1.2.3"));
        }
    }

    public class UpdateNotifierTests : IDisposable
    {
        private readonly string _dir = Path.Combine(Path.GetTempPath(), "kombos-tests-" + Guid.NewGuid().ToString("N"));
        public UpdateNotifierTests() => Directory.CreateDirectory(_dir);
        public void Dispose() { try { Directory.Delete(_dir, true); } catch { } }

        private static HttpResponseMessage Release(string tag, bool pre = false, bool draft = false, string etag = "\"abc\"")
        {
            var r = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($"{{\"tag_name\":\"{tag}\",\"html_url\":\"https://github.com/o/r/releases/tag/{tag}\",\"prerelease\":{pre.ToString().ToLower()},\"draft\":{draft.ToString().ToLower()}}}")
            };
            r.Headers.TryAddWithoutValidation("ETag", etag);
            return r;
        }

        [Fact]
        public async Task NotifiesOncePerVersion_ThenRemembers()
        {
            var h = new FakeHandler { Respond = _ => Release("v4.9.0") };
            var n = new UpdateNotifier(new HttpClient(h), new UpdateNoticeStore(Path.Combine(_dir, "n.json")), "o/r", "4.8.2");
            var first = await n.CheckAsync();
            Assert.NotNull(first);
            Assert.Equal("4.9.0", first!.Version);
            n.MarkNotified(first.Version);
            Assert.Null(await n.CheckAsync());
        }

        [Fact]
        public async Task SendsIfNoneMatch_And304UsesCache()
        {
            var calls = 0;
            var h = new FakeHandler { Respond = req => ++calls == 1 ? Release("v4.9.0") : new HttpResponseMessage(HttpStatusCode.NotModified) };
            var store = new UpdateNoticeStore(Path.Combine(_dir, "n.json"));
            var n = new UpdateNotifier(new HttpClient(h), store, "o/r", "4.8.2");
            Assert.NotNull(await n.CheckAsync());
            var second = await n.CheckAsync();               // 304 → ίδια γνώση, ακόμη δεν έχει "σημανθεί"
            Assert.NotNull(second);
            Assert.Equal("\"abc\"", h.Requests[1].Headers.IfNoneMatch.First().ToString());
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public async Task IgnoresPrereleaseAndDraft(bool pre, bool draft)
        {
            var h = new FakeHandler { Respond = _ => Release("v9.0.0", pre, draft) };
            var n = new UpdateNotifier(new HttpClient(h), new UpdateNoticeStore(Path.Combine(_dir, "n.json")), "o/r", "4.8.2");
            Assert.Null(await n.CheckAsync());
        }

        [Fact]
        public async Task ErrorsAreIgnored_AndOlderIsNotAnnounced()
        {
            var bad = new UpdateNotifier(new HttpClient(new FakeHandler { Respond = _ => new HttpResponseMessage(HttpStatusCode.Forbidden) }),
                new UpdateNoticeStore(Path.Combine(_dir, "a.json")), "o/r", "4.8.2");
            Assert.Null(await bad.CheckAsync());
            var old = new UpdateNotifier(new HttpClient(new FakeHandler { Respond = _ => Release("v4.8.2") }),
                new UpdateNoticeStore(Path.Combine(_dir, "b.json")), "o/r", "4.8.2");
            Assert.Null(await old.CheckAsync());
        }

        [Fact]
        public async Task RejectsReleaseUrlOutsideGitHub()
        {
            var h = new FakeHandler
            {
                Respond = _ => new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent("{\"tag_name\":\"v9.0.0\",\"html_url\":\"https://evil.example/x\"}") }
            };
            var n = new UpdateNotifier(new HttpClient(h), new UpdateNoticeStore(Path.Combine(_dir, "n.json")), "o/r", "4.8.2");
            Assert.Null(await n.CheckAsync());
        }
    }

    public class SuiteCatalogTests : IDisposable
    {
        private readonly string _dir = Path.Combine(Path.GetTempPath(), "kombos-tests-" + Guid.NewGuid().ToString("N"));
        public SuiteCatalogTests() => Directory.CreateDirectory(_dir);
        public void Dispose() { try { Directory.Delete(_dir, true); } catch { } }

        private static string Json(string owner = "giorstergiopoulos-design", string repo = "Opsis", string rx = "^A\\.exe$", string extra = "") =>
            "{\"schema\":1,\"owner\":\"" + owner + "\",\"unknownTop\":1,\"apps\":[{\"id\":\"opsis\",\"name\":\"Opsis\",\"repo\":\"" + repo + "\",\"installerAsset\":\"" + rx.Replace("\\", "\\\\") +
            "\",\"uninstallDisplayName\":\"Opsis\",\"paths\":[\"%X%\\\\a.exe\"],\"futureField\":true," +
            "\"tagline\":{\"en\":\"t\"},\"capabilities\":{\"en\":[\"a\",\"b\"]}" + extra + "}]}";

        [Fact]
        public void EmbeddedCatalogueIsValidAndHasAllSevenApps()
        {
            var c = SuiteCatalogLoader.LoadEmbedded();
            Assert.Equal(7, c.Apps.Count);
            Assert.All(c.Apps, a => { Assert.NotEmpty(a.TaglineFor("el")); Assert.NotEmpty(a.CapabilitiesFor("el")); });
            Assert.Contains(c.Apps, a => a.Id == "kombos");
        }

        [Fact]
        public void ParsesAndIgnoresUnknownFields()
        {
            var c = SuiteCatalog.TryParse(Encoding.UTF8.GetBytes(Json()), out var err);
            Assert.NotNull(c); Assert.Null(err);
            Assert.Equal("t", c!.Apps[0].TaglineFor("fr"));          // fallback στα αγγλικά
        }

        [Theory]
        [InlineData("evil-owner", "Opsis", "^A$")]
        [InlineData("giorstergiopoulos-design", "../etc", "^A$")]
        [InlineData("giorstergiopoulos-design", "Opsis", "([")]     // μη έγκυρο regex
        public void RejectsBadData(string owner, string repo, string rx) =>
            Assert.Null(SuiteCatalog.TryParse(Encoding.UTF8.GetBytes(Json(owner, repo, rx)), out _));

        [Fact]
        public void RejectsMalformedAndOversize()
        {
            Assert.Null(SuiteCatalog.TryParse(Encoding.UTF8.GetBytes("{not json"), out var e1)); Assert.NotNull(e1);
            Assert.Null(SuiteCatalog.TryParse(new byte[SuiteCatalog.MaxBytes + 1], out var e2)); Assert.Equal("too-large", e2);
            Assert.Null(SuiteCatalog.TryParse(Encoding.UTF8.GetBytes("{\"schema\":2,\"apps\":[]}"), out _));
        }

        [Theory]
        [InlineData("https://github.com/a/b", true)]
        [InlineData("https://raw.githubusercontent.com/a/b/c", true)]
        [InlineData("https://api.github.com/x", true)]
        [InlineData("http://github.com/a", false)]
        [InlineData("https://github.com.evil.com/a", false)]
        [InlineData("https://evil.com/github.com", false)]
        public void HostAllowList(string url, bool ok) => Assert.Equal(ok, SuiteCatalog.IsAllowedHost(new Uri(url)));

        [Fact]
        public async Task Refresh_WritesValidCache_AndSkipsWhenFresh()
        {
            var h = new FakeHandler { Respond = _ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(Json()) } };
            var http = new HttpClient(h);
            Assert.True(await SuiteCatalogLoader.RefreshAsync(http, _dir));
            Assert.Single(SuiteCatalogLoader.LoadCachedOrEmbedded(_dir).Apps);
            Assert.False(await SuiteCatalogLoader.RefreshAsync(http, _dir));   // < 24 ώρες
            Assert.Single(h.Requests);
        }

        [Fact]
        public async Task Refresh_RejectsInvalidAndWrongHost_KeepsEmbedded()
        {
            var bad = new HttpClient(new FakeHandler { Respond = _ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{oops") } });
            Assert.False(await SuiteCatalogLoader.RefreshAsync(bad, _dir));
            Assert.False(File.Exists(Path.Combine(_dir, "apps.json")));
            var good = new HttpClient(new FakeHandler { Respond = _ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(Json()) } });
            Assert.False(await SuiteCatalogLoader.RefreshAsync(good, _dir, "https://evil.example/apps.json"));
            Assert.Equal(7, SuiteCatalogLoader.LoadCachedOrEmbedded(_dir).Apps.Count);
        }
    }

    public class InstallDetectorTests
    {
        private class FakeProbe : IInstallProbe
        {
            public List<UninstallEntry> Entries = new();
            public HashSet<string> Files = new(StringComparer.OrdinalIgnoreCase);
            public Dictionary<string, string> Versions = new(StringComparer.OrdinalIgnoreCase);
            public IEnumerable<UninstallEntry> UninstallEntries() => Entries;
            public bool FileExists(string p) => Files.Contains(p);
            public string? FileVersion(string p) => Versions.GetValueOrDefault(p);
            public string Expand(string p) => p.Replace("%PF%", @"C:\PF");
        }

        private static SuiteApp App(string display = "Waveframe", string[]? alias = null) =>
            new("wf", "Waveframe", "waveframe", "^x$", null, display, alias ?? Array.Empty<string>(), new[] { @"%PF%\Waveframe\Waveframe.exe" },
                new() { ["en"] = "t" }, new() { ["en"] = new[] { "a" } });

        [Fact] public void NotInstalled_WhenNothingFound() =>
            Assert.Equal(InstallState.NotInstalled, InstallDetector.Detect(App(), new FakeProbe(), "v1.0.0").State);

        [Fact]
        public void MatchesNsisStyleNameWithVersion_AndFlagsUpdate()
        {
            var p = new FakeProbe(); p.Entries.Add(new("Waveframe 1.5.0", "1.5.0", null));
            var r = InstallDetector.Detect(App(), p, "v1.6.0");
            Assert.Equal(InstallState.UpdateAvailable, r.State);
            Assert.Equal("1.5.0", r.InstalledVersion);
        }

        [Fact]
        public void DoesNotMatchSimilarlyNamedOtherApp()
        {
            var p = new FakeProbe(); p.Entries.Add(new("Waveframe Studio Pro", "9.0", null));
            Assert.Equal(InstallState.NotInstalled, InstallDetector.Detect(App(), p, null).State);
        }

        [Fact]
        public void FallsBackToFileVersion_WhenNoRegistryEntry()
        {
            var p = new FakeProbe(); p.Files.Add(@"C:\PF\Waveframe\Waveframe.exe"); p.Versions[@"C:\PF\Waveframe\Waveframe.exe"] = "1.6.0";
            var r = InstallDetector.Detect(App(), p, "v1.6.0");
            Assert.Equal(InstallState.Installed, r.State);
            Assert.Equal(@"C:\PF\Waveframe\Waveframe.exe", r.ExePath);
        }

        [Fact]
        public void AliasMatches_GreekAppName()
        {
            var p = new FakeProbe(); p.Entries.Add(new("Κόμβος", "4.8.2", null));
            Assert.Equal(InstallState.Installed, InstallDetector.Detect(App("Kombos", new[] { "Κόμβος" }), p, "v4.8.2").State);
        }

        [Fact]
        public void UsesInstallLocationToFindExe()
        {
            var p = new FakeProbe(); p.Entries.Add(new("Waveframe", "1.0", @"D:\Apps\wf")); p.Files.Add(@"D:\Apps\wf\Waveframe.exe");
            Assert.Equal(@"D:\Apps\wf\Waveframe.exe", InstallDetector.Detect(App(), p, null).ExePath);
        }
    }

    public class ReleaseDownloaderTests : IDisposable
    {
        private readonly string _dir = Path.Combine(Path.GetTempPath(), "kombos-tests-" + Guid.NewGuid().ToString("N"));
        public ReleaseDownloaderTests() => Directory.CreateDirectory(_dir);
        public void Dispose() { try { Directory.Delete(_dir, true); } catch { } }

        private static string HashOf(byte[] b) => Convert.ToHexString(SHA256.HashData(b)).ToLowerInvariant();

        [Fact]
        public void ParseChecksumText_Formats()
        {
            var h = new string('a', 64);
            Assert.Equal(h, ReleaseDownloader.ParseChecksumText(h + "\n", "x.exe"));
            Assert.Equal(h, ReleaseDownloader.ParseChecksumText(h + "  x.exe\n", "x.exe"));
            Assert.Equal(h, ReleaseDownloader.ParseChecksumText("SHA256 (x.exe) = " + h, "x.exe"));
            Assert.Null(ReleaseDownloader.ParseChecksumText("nothing here", "x.exe"));
        }

        [Fact]
        public void ParseDigest()
        {
            var h = new string('b', 64);
            Assert.Equal(h, ReleaseDownloader.ParseDigest("sha256:" + h));
            Assert.Null(ReleaseDownloader.ParseDigest("sha1:" + h));
            Assert.Null(ReleaseDownloader.ParseDigest("sha256:zz"));
            Assert.Null(ReleaseDownloader.ParseDigest(null));
        }

        [Fact]
        public void PickInstaller_ByRegex_AndRejectsForeignHost()
        {
            var rel = new LatestRelease("v1", "https://github.com/x", new[]
            {
                new ReleaseAsset("App.zip", 1, "https://github.com/a/b/releases/download/v1/App.zip", null),
                new ReleaseAsset("App-Setup-1.exe", 1, "https://github.com/a/b/releases/download/v1/App-Setup-1.exe", null),
                new ReleaseAsset("Evil-Setup.exe", 1, "https://evil.example/Evil-Setup.exe", null),
            });
            Assert.Equal("App-Setup-1.exe", ReleaseDownloader.PickInstaller(rel, @"^App-Setup-.*\.exe$")!.Name);
            Assert.Null(ReleaseDownloader.PickInstaller(rel, @"^Evil-Setup\.exe$"));
        }

        [Fact]
        public async Task Download_MatchingHash_Succeeds_AndLeavesNoPartFile()
        {
            var data = Encoding.UTF8.GetBytes("installer-bytes");
            var http = new HttpClient(new FakeHandler { Respond = _ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(data) } });
            var asset = new ReleaseAsset("S.exe", data.Length, "https://github.com/a/b/releases/download/v1/S.exe", null);
            var path = await ReleaseDownloader.DownloadVerifiedAsync(http, asset, HashOf(data), _dir);
            Assert.True(File.Exists(path));
            Assert.False(File.Exists(path + ".part"));
            Assert.True(ReleaseDownloader.Sha256Matches(path, HashOf(data)));
        }

        [Fact]
        public async Task Download_HashMismatch_RefusesAndDeletes()
        {
            var data = Encoding.UTF8.GetBytes("tampered");
            var http = new HttpClient(new FakeHandler { Respond = _ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(data) } });
            var asset = new ReleaseAsset("S.exe", data.Length, "https://github.com/a/b/releases/download/v1/S.exe", null);
            await Assert.ThrowsAsync<ChecksumMismatchException>(() =>
                ReleaseDownloader.DownloadVerifiedAsync(http, asset, new string('0', 64), _dir));
            Assert.Empty(Directory.GetFiles(_dir));
        }

        [Fact]
        public async Task Download_RefusesForeignHost()
        {
            var http = new HttpClient(new FakeHandler());
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                ReleaseDownloader.DownloadVerifiedAsync(http, new ReleaseAsset("S.exe", 1, "https://evil.example/S.exe", null), new string('0', 64), _dir));
        }

        [Fact]
        public async Task ResolveExpectedHash_PrefersSha256Asset_ThenDigest()
        {
            var data = Encoding.UTF8.GetBytes("x");
            var hash = HashOf(data);
            var inst = new ReleaseAsset("S.exe", 1, "https://github.com/a/b/releases/download/v1/S.exe", "sha256:" + new string('c', 64));
            var sum = new ReleaseAsset("S.exe.sha256", 80, "https://github.com/a/b/releases/download/v1/S.exe.sha256", null);
            var http = new HttpClient(new FakeHandler { Respond = _ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(hash + "  S.exe") } });
            Assert.Equal(hash, await ReleaseDownloader.ResolveExpectedHashAsync(http, new LatestRelease("v1", "", new[] { inst, sum }), inst, null));
            Assert.Equal(new string('c', 64), await ReleaseDownloader.ResolveExpectedHashAsync(http, new LatestRelease("v1", "", new[] { inst }), inst, null));
            var noHash = inst with { Digest = null };
            Assert.Null(await ReleaseDownloader.ResolveExpectedHashAsync(http, new LatestRelease("v1", "", new[] { noHash }), noHash, null));
        }

        [Fact]
        public void ParseRelease_IgnoresDraftAndPrerelease()
        {
            Assert.Null(ReleaseDownloader.ParseRelease("{\"tag_name\":\"v1\",\"draft\":true,\"assets\":[]}"));
            Assert.Null(ReleaseDownloader.ParseRelease("{\"tag_name\":\"v1\",\"prerelease\":true,\"assets\":[]}"));
            var r = ReleaseDownloader.ParseRelease("{\"tag_name\":\"v1\",\"html_url\":\"https://github.com/x\",\"assets\":[{\"name\":\"a\",\"size\":2,\"browser_download_url\":\"https://github.com/a\",\"digest\":\"sha256:" + new string('d', 64) + "\"}]}");
            Assert.Equal("v1", r!.Tag);
            Assert.Equal(2, r.Assets[0].Size);
        }
    }
}
