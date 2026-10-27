using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Kombos.Services
{
    public enum InstallState { NotInstalled, Installed, UpdateAvailable }
    public record InstallInfo(InstallState State, string? InstalledVersion, string? ExePath);
    public record UninstallEntry(string DisplayName, string? DisplayVersion, string? InstallLocation);

    /// <summary>Αφαίρεση του I/O ώστε η ανίχνευση εγκατάστασης να δοκιμάζεται με ψεύτικα δεδομένα.</summary>
    public interface IInstallProbe
    {
        IEnumerable<UninstallEntry> UninstallEntries();
        bool FileExists(string path);
        string? FileVersion(string path);
        string Expand(string path);
    }

    /// <summary>Μόνο ανάγνωση: κλειδιά Uninstall του μητρώου (HKCU/HKLM, 64+32 bit) και αρχεία.</summary>
    public class WindowsInstallProbe : IInstallProbe
    {
        public IEnumerable<UninstallEntry> UninstallEntries()
        {
            var results = new List<UninstallEntry>();
            foreach (var (hive, view) in new[] { (RegistryHive.CurrentUser, RegistryView.Default), (RegistryHive.LocalMachine, RegistryView.Registry64), (RegistryHive.LocalMachine, RegistryView.Registry32) })
            {
                try
                {
                    using var baseKey = RegistryKey.OpenBaseKey(hive, view);
                    using var uninstall = baseKey.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
                    if (uninstall == null) continue;
                    foreach (var sub in uninstall.GetSubKeyNames())
                    {
                        try
                        {
                            using var k = uninstall.OpenSubKey(sub);
                            var name = k?.GetValue("DisplayName") as string;
                            if (string.IsNullOrWhiteSpace(name)) continue;
                            results.Add(new UninstallEntry(name!, k!.GetValue("DisplayVersion") as string, k.GetValue("InstallLocation") as string));
                        }
                        catch { }
                    }
                }
                catch { }
            }
            return results;
        }

        public bool FileExists(string path) { try { return File.Exists(path); } catch { return false; } }
        public string? FileVersion(string path)
        {
            try { return FileVersionInfo.GetVersionInfo(path).ProductVersion?.Split('+')[0]; } catch { return null; }
        }
        public string Expand(string path) => Environment.ExpandEnvironmentVariables(path);
    }

    public static class InstallDetector
    {
        public static InstallInfo Detect(SuiteApp app, IInstallProbe probe, string? latestTag)
        {
            var names = new[] { app.UninstallDisplayName }.Concat(app.UninstallAliases).Where(n => n.Length > 0).ToArray();
            UninstallEntry? entry = probe.UninstallEntries().FirstOrDefault(e => names.Any(n =>
                e.DisplayName.Equals(n, StringComparison.OrdinalIgnoreCase) ||
                e.DisplayName.StartsWith(n + " ", StringComparison.OrdinalIgnoreCase) && Regex.IsMatch(e.DisplayName[(n.Length + 1)..], @"^v?\d")));

            string? exe = app.Paths.Select(probe.Expand).FirstOrDefault(probe.FileExists);
            if (exe == null && entry?.InstallLocation is { Length: > 0 } loc && app.Paths.Length > 0)
            {
                var candidate = Path.Combine(loc, Path.GetFileName(app.Paths[0]));
                if (probe.FileExists(candidate)) exe = candidate;
            }
            if (entry == null && exe == null) return new InstallInfo(InstallState.NotInstalled, null, null);

            var version = entry?.DisplayVersion;
            if (string.IsNullOrWhiteSpace(version) && exe != null) version = probe.FileVersion(exe);
            bool update = latestTag != null && !string.IsNullOrWhiteSpace(version) && VersionComparer.IsNewer(latestTag, version);
            return new InstallInfo(update ? InstallState.UpdateAvailable : InstallState.Installed, version, exe);
        }
    }

    public record ReleaseAsset(string Name, long Size, string Url, string? Digest);
    public record LatestRelease(string Tag, string HtmlUrl, IReadOnlyList<ReleaseAsset> Assets);

    public class ChecksumMismatchException : Exception
    {
        public ChecksumMismatchException(string expected, string actual) : base($"SHA-256 mismatch: expected {expected}, got {actual}") { }
    }

    public static class ReleaseDownloader
    {
        public static LatestRelease? ParseRelease(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var r = doc.RootElement;
                if ((r.TryGetProperty("draft", out var d) && d.ValueKind == JsonValueKind.True) ||
                    (r.TryGetProperty("prerelease", out var p) && p.ValueKind == JsonValueKind.True)) return null;
                var tag = r.GetProperty("tag_name").GetString() ?? "";
                var url = r.TryGetProperty("html_url", out var u) ? u.GetString() ?? "" : "";
                var assets = new List<ReleaseAsset>();
                if (r.TryGetProperty("assets", out var arr))
                    foreach (var a in arr.EnumerateArray())
                        assets.Add(new ReleaseAsset(a.GetProperty("name").GetString() ?? "",
                            a.TryGetProperty("size", out var s) ? s.GetInt64() : 0,
                            a.GetProperty("browser_download_url").GetString() ?? "",
                            a.TryGetProperty("digest", out var dg) && dg.ValueKind == JsonValueKind.String ? dg.GetString() : null));
                return new LatestRelease(tag, url, assets);
            }
            catch { return null; }
        }

        public static async Task<LatestRelease?> GetLatestAsync(HttpClient http, string repo, CancellationToken ct = default)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(TimeSpan.FromSeconds(8));
                using var req = new HttpRequestMessage(HttpMethod.Get, $"https://api.github.com/repos/{SuiteCatalog.Owner}/{repo}/releases/latest");
                req.Headers.UserAgent.ParseAdd("Kombos");
                req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
                using var resp = await http.SendAsync(req, cts.Token);
                if (!resp.IsSuccessStatusCode) return null;
                return ParseRelease(await resp.Content.ReadAsStringAsync(cts.Token));
            }
            catch { return null; }
        }

        public static ReleaseAsset? PickInstaller(LatestRelease r, string installerRegex)
        {
            Regex rx;
            try { rx = new Regex(installerRegex, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)); } catch { return null; }
            return r.Assets.FirstOrDefault(a => rx.IsMatch(a.Name) && Uri.TryCreate(a.Url, UriKind.Absolute, out var u) && SuiteCatalog.IsAllowedHost(u));
        }

        /// <summary>Δέχεται "hash", "hash  file" ή "SHA256 (file) = hash"· επιστρέφει τον hex hash ή null.</summary>
        public static string? ParseChecksumText(string text, string fileName)
        {
            foreach (var line in text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var m = Regex.Match(line, @"\b([0-9a-fA-F]{64})\b");
                if (!m.Success) continue;
                if (line.Contains(fileName, StringComparison.OrdinalIgnoreCase) || text.Trim().Split('\n').Length == 1)
                    return m.Groups[1].Value.ToLowerInvariant();
            }
            return null;
        }

        public static string? ParseDigest(string? digest) =>
            digest != null && digest.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase) && Regex.IsMatch(digest[7..], "^[0-9a-fA-F]{64}$")
                ? digest[7..].ToLowerInvariant() : null;

        public static string Sha256Of(string file)
        {
            using var fs = File.OpenRead(file);
            return Convert.ToHexString(SHA256.HashData(fs)).ToLowerInvariant();
        }

        public static bool Sha256Matches(string file, string expectedHex) =>
            string.Equals(Sha256Of(file), expectedHex, StringComparison.OrdinalIgnoreCase);

        /// <summary>Κατεβάζει στον φάκελο προορισμού ως .part, επαληθεύει SHA-256 και μετονομάζει· σε
        /// ασυμφωνία διαγράφει το αρχείο και ρίχνει <see cref="ChecksumMismatchException"/>. ΠΟΤΕ δεν εκτελεί τον installer.</summary>
        public static async Task<string> DownloadVerifiedAsync(HttpClient http, ReleaseAsset asset, string expectedHex, string destDir,
            IProgress<double>? progress = null, CancellationToken ct = default)
        {
            if (!Uri.TryCreate(asset.Url, UriKind.Absolute, out var uri) || !SuiteCatalog.IsAllowedHost(uri))
                throw new InvalidOperationException("download host not allowed");
            var safeName = Path.GetFileName(asset.Name);
            if (string.IsNullOrEmpty(safeName)) throw new InvalidOperationException("bad asset name");
            Directory.CreateDirectory(destDir);
            var final = Path.Combine(destDir, safeName);
            var part = final + ".part";

            using var req = new HttpRequestMessage(HttpMethod.Get, uri);
            req.Headers.UserAgent.ParseAdd("Kombos");
            using var resp = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
            resp.EnsureSuccessStatusCode();
            if (!SuiteCatalog.IsAllowedHost(resp.RequestMessage?.RequestUri)) throw new InvalidOperationException("redirect host not allowed");

            long total = resp.Content.Headers.ContentLength ?? asset.Size;
            await using (var src = await resp.Content.ReadAsStreamAsync(ct))
            await using (var dst = File.Create(part))
            {
                var buf = new byte[81920]; int n; long done = 0;
                while ((n = await src.ReadAsync(buf, ct)) > 0)
                {
                    await dst.WriteAsync(buf.AsMemory(0, n), ct);
                    done += n;
                    if (total > 0) progress?.Report((double)done / total);
                }
            }
            var actual = Sha256Of(part);
            if (!string.Equals(actual, expectedHex, StringComparison.OrdinalIgnoreCase))
            {
                try { File.Delete(part); } catch { }
                throw new ChecksumMismatchException(expectedHex, actual);
            }
            if (File.Exists(final)) File.Delete(final);
            File.Move(part, final);
            return final;
        }

        /// <summary>Αναμενόμενος hash: πρώτα το αρχείο .sha256 του release, αλλιώς το πεδίο digest του asset. null = άγνωστος.</summary>
        public static async Task<string?> ResolveExpectedHashAsync(HttpClient http, LatestRelease rel, ReleaseAsset installer, string? checksumAssetName, CancellationToken ct = default)
        {
            var sumAsset = rel.Assets.FirstOrDefault(a => a.Name.Equals(checksumAssetName ?? installer.Name + ".sha256", StringComparison.OrdinalIgnoreCase));
            if (sumAsset != null && Uri.TryCreate(sumAsset.Url, UriKind.Absolute, out var u) && SuiteCatalog.IsAllowedHost(u) && sumAsset.Size < 2048)
            {
                try
                {
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    cts.CancelAfter(TimeSpan.FromSeconds(8));
                    var text = await http.GetStringAsync(u, cts.Token);
                    var h = ParseChecksumText(text, installer.Name);
                    if (h != null) return h;
                }
                catch { }
            }
            return ParseDigest(installer.Digest);
        }
    }
}
