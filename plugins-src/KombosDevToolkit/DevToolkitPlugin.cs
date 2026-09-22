using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Kombos.Plugins;

namespace KombosDevToolkit
{
    /// <summary>Επίσημο, πρώτου-μέρους πακέτο εργαλείων για developers - όλα τρέχουν 100% τοπικά, καμία
    /// κλήση σε AI/δίκτυο, άρα μηδενικό κόστος tokens. Μέρος της "Λήψη &amp; Ενσωμάτωση" γκαλερί plugins.</summary>
    public class DevToolkitPlugin : IKombosPlugin
    {
        public string Name => "Εργαλειοθήκη Developer";
        public string Version => "1.0.0";

        public void Initialize(IKombosHost host)
        {
            host.RegisterSlashCommand("/json", JsonAsync);
            host.RegisterSlashCommand("/base64", Base64Async);
            host.RegisterSlashCommand("/hash", HashAsync);
            host.RegisterSlashCommand("/uuid", UuidAsync);
            host.RegisterSlashCommand("/color", ColorAsync);
            host.RegisterSlashCommand("/timestamp", TimestampAsync);
            host.RegisterSlashCommand("/regex", RegexAsync);
            host.RegisterSlashCommand("/git", GitAsync);
            host.RegisterSlashCommand("/http", HttpAsync);
            host.RegisterSlashCommand("/cron", CronAsync);
            host.Log("Dev Toolkit plugin initialized (10 commands).");
        }

        private static Task<string> JsonAsync(string arg)
        {
            if (string.IsNullOrWhiteSpace(arg)) return Task.FromResult("Χρήση: /json {\"a\":1} (μορφοποιεί/επικυρώνει JSON)");
            try
            {
                using var doc = JsonDocument.Parse(arg);
                var pretty = JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
                return Task.FromResult($"✅ Έγκυρο JSON:\n```json\n{pretty}\n```");
            }
            catch (JsonException ex)
            {
                return Task.FromResult($"❌ Μη έγκυρο JSON: {ex.Message}");
            }
        }

        private static Task<string> Base64Async(string arg)
        {
            var parts = arg.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) return Task.FromResult("Χρήση: /base64 encode <κείμενο>  ή  /base64 decode <base64>");
            var mode = parts[0].ToLowerInvariant();
            var value = parts[1];
            try
            {
                if (mode == "encode") return Task.FromResult(Convert.ToBase64String(Encoding.UTF8.GetBytes(value)));
                if (mode == "decode") return Task.FromResult(Encoding.UTF8.GetString(Convert.FromBase64String(value)));
                return Task.FromResult("Χρήση: /base64 encode|decode <κείμενο>");
            }
            catch (Exception ex) { return Task.FromResult($"❌ Σφάλμα: {ex.Message}"); }
        }

        private static Task<string> HashAsync(string arg)
        {
            if (string.IsNullOrEmpty(arg)) return Task.FromResult("Χρήση: /hash <κείμενο>");
            var bytes = Encoding.UTF8.GetBytes(arg);
            string ToHex(byte[] b) => Convert.ToHexString(b).ToLowerInvariant();
            var md5 = ToHex(MD5.HashData(bytes));
            var sha1 = ToHex(SHA1.HashData(bytes));
            var sha256 = ToHex(SHA256.HashData(bytes));
            return Task.FromResult($"MD5: {md5}\nSHA1: {sha1}\nSHA256: {sha256}");
        }

        private static Task<string> UuidAsync(string arg) => Task.FromResult(Guid.NewGuid().ToString());

        private static Task<string> ColorAsync(string arg)
        {
            arg = arg.Trim();
            if (arg.Length == 0) return Task.FromResult("Χρήση: /color #RRGGBB  ή  /color r,g,b");
            try
            {
                byte r, g, b;
                if (arg.StartsWith('#'))
                {
                    var hex = arg[1..];
                    if (hex.Length == 3) hex = string.Concat(hex[0], hex[0], hex[1], hex[1], hex[2], hex[2]);
                    r = Convert.ToByte(hex[..2], 16);
                    g = Convert.ToByte(hex[2..4], 16);
                    b = Convert.ToByte(hex[4..6], 16);
                }
                else
                {
                    var vals = arg.Split(',');
                    if (vals.Length != 3) return Task.FromResult("Χρήση: /color #RRGGBB  ή  /color r,g,b");
                    r = byte.Parse(vals[0].Trim()); g = byte.Parse(vals[1].Trim()); b = byte.Parse(vals[2].Trim());
                }
                double rf = r / 255.0, gf = g / 255.0, bf = b / 255.0;
                double max = Math.Max(rf, Math.Max(gf, bf)), min = Math.Min(rf, Math.Min(gf, bf));
                double h = 0, s, l = (max + min) / 2;
                double d = max - min;
                s = d == 0 ? 0 : d / (1 - Math.Abs(2 * l - 1));
                if (d != 0)
                {
                    if (max == rf) h = 60 * (((gf - bf) / d) % 6);
                    else if (max == gf) h = 60 * ((bf - rf) / d + 2);
                    else h = 60 * ((rf - gf) / d + 4);
                }
                if (h < 0) h += 360;
                return Task.FromResult($"HEX: #{r:X2}{g:X2}{b:X2}\nRGB: {r}, {g}, {b}\nHSL: {h:F0}°, {s * 100:F0}%, {l * 100:F0}%");
            }
            catch { return Task.FromResult("❌ Μη έγκυρη μορφή χρώματος. Χρήση: /color #RRGGBB  ή  /color r,g,b"); }
        }

        private static Task<string> TimestampAsync(string arg)
        {
            arg = arg.Trim();
            if (arg.Length == 0 || arg.Equals("now", StringComparison.OrdinalIgnoreCase))
            {
                var now = DateTimeOffset.UtcNow;
                return Task.FromResult($"Τώρα: {now.ToUnixTimeSeconds()} (Unix) · {now:yyyy-MM-dd HH:mm:ss} UTC");
            }
            if (long.TryParse(arg, out var unix))
            {
                var dt = DateTimeOffset.FromUnixTimeSeconds(unix);
                return Task.FromResult($"{unix} → {dt:yyyy-MM-dd HH:mm:ss} UTC ({dt.ToLocalTime():yyyy-MM-dd HH:mm:ss} τοπική ώρα)");
            }
            if (DateTimeOffset.TryParse(arg, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
                return Task.FromResult($"{arg} → {parsed.ToUnixTimeSeconds()} (Unix)");
            return Task.FromResult("Χρήση: /timestamp now  ή  /timestamp <unix>  ή  /timestamp <ημερομηνία ISO>");
        }

        private static Task<string> RegexAsync(string arg)
        {
            var parts = arg.Split('|', 2);
            if (parts.Length < 2) return Task.FromResult("Χρήση: /regex <pattern>|<κείμενο δοκιμής>");
            try
            {
                var matches = Regex.Matches(parts[1], parts[0]);
                if (matches.Count == 0) return Task.FromResult("Καμία αντιστοίχιση.");
                var sb = new StringBuilder($"{matches.Count} αντιστοιχίσεις:\n");
                foreach (Match m in matches) sb.AppendLine($"• \"{m.Value}\" (θέση {m.Index})");
                return Task.FromResult(sb.ToString().TrimEnd());
            }
            catch (Exception ex) { return Task.FromResult($"❌ Μη έγκυρο regex: {ex.Message}"); }
        }

        private static readonly System.Collections.Generic.Dictionary<string, string> GitCheatsheet = new(StringComparer.OrdinalIgnoreCase)
        {
            ["undo commit"] = "git reset --soft HEAD~1   (κρατά τις αλλαγές staged)",
            ["undo last commit"] = "git reset --soft HEAD~1   (κρατά τις αλλαγές staged)",
            ["discard changes"] = "git checkout -- <αρχείο>   ή   git restore <αρχείο>",
            ["stash"] = "git stash        (αποθήκευση)\ngit stash pop    (επαναφορά)",
            ["rename branch"] = "git branch -m <νέο-όνομα>",
            ["delete branch"] = "git branch -d <όνομα>     (ασφαλές)\ngit branch -D <όνομα>     (αναγκαστικό)",
            ["squash"] = "git rebase -i HEAD~<Ν>   → άλλαξε 'pick' σε 'squash' στα commits που θες να ενώσεις",
            ["amend"] = "git commit --amend        (αλλάζει το τελευταίο commit)",
            ["revert"] = "git revert <hash>         (νέο commit που αναιρεί ένα παλιό, ασφαλές σε shared branches)",
            ["tag"] = "git tag v1.0.0 && git push origin v1.0.0",
            ["remote"] = "git remote -v             (λίστα)\ngit remote set-url origin <url>",
            ["ignore"] = "πρόσθεσε το pattern στο .gitignore - αν το αρχείο ήδη tracked: git rm --cached <αρχείο>",
            ["conflict"] = "διόρθωσε τα <<<<<<< μέσα στα αρχεία, μετά: git add <αρχείο> && git commit",
        };

        private static Task<string> GitAsync(string arg)
        {
            arg = arg.Trim();
            if (arg.Length == 0)
                return Task.FromResult("Χρήση: /git <τι θες να κάνεις>, π.χ. /git undo commit, /git stash, /git squash\nΓνωστά θέματα: " + string.Join(", ", new System.Collections.Generic.HashSet<string>(GitCheatsheet.Keys)));
            foreach (var kv in GitCheatsheet)
                if (arg.Contains(kv.Key, StringComparison.OrdinalIgnoreCase))
                    return Task.FromResult($"{kv.Value}");
            return Task.FromResult($"Δεν βρέθηκε έτοιμη συνταγή για «{arg}». Γνωστά θέματα: " + string.Join(", ", new System.Collections.Generic.HashSet<string>(GitCheatsheet.Keys)));
        }

        private static readonly System.Collections.Generic.Dictionary<int, string> HttpStatusCodes = new()
        {
            [200] = "OK - επιτυχής αίτηση",
            [201] = "Created - δημιουργήθηκε νέος πόρος",
            [204] = "No Content - επιτυχία, χωρίς σώμα απάντησης",
            [301] = "Moved Permanently - μόνιμη ανακατεύθυνση",
            [302] = "Found - προσωρινή ανακατεύθυνση",
            [304] = "Not Modified - χρήση cache",
            [400] = "Bad Request - λανθασμένη αίτηση",
            [401] = "Unauthorized - λείπει/άκυρη αυθεντικοποίηση",
            [403] = "Forbidden - δεν επιτρέπεται πρόσβαση",
            [404] = "Not Found - δεν βρέθηκε ο πόρος",
            [405] = "Method Not Allowed",
            [408] = "Request Timeout",
            [409] = "Conflict",
            [410] = "Gone",
            [418] = "I'm a teapot (RFC 2324, αστείο)",
            [422] = "Unprocessable Entity - έγκυρο JSON αλλά άκυρα δεδομένα",
            [429] = "Too Many Requests - rate limit",
            [500] = "Internal Server Error",
            [502] = "Bad Gateway",
            [503] = "Service Unavailable",
            [504] = "Gateway Timeout",
        };

        private static Task<string> HttpAsync(string arg)
        {
            arg = arg.Trim();
            if (int.TryParse(arg, out var code))
                return Task.FromResult(HttpStatusCodes.TryGetValue(code, out var meaning) ? $"{code}: {meaning}" : $"{code}: άγνωστος/μη τυποποιημένος κωδικός.");
            return Task.FromResult("Χρήση: /http <κωδικός>, π.χ. /http 404");
        }

        private static Task<string> CronAsync(string arg)
        {
            arg = arg.Trim();
            if (arg.Length == 0) return Task.FromResult("Χρήση: /cron <5 πεδία>, π.χ. /cron 0 9 * * 1-5");
            var fields = arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length != 5) return Task.FromResult("Χρειάζονται ακριβώς 5 πεδία: λεπτό ώρα ημέρα-μήνα μήνας ημέρα-εβδομάδας");
            string Describe(string field, string unit) => field == "*" ? $"κάθε {unit}" : $"{unit} = {field}";
            var minute = Describe(fields[0], "λεπτό");
            var hour = Describe(fields[1], "ώρα");
            var dom = Describe(fields[2], "ημέρα του μήνα");
            var month = Describe(fields[3], "μήνας");
            var dow = Describe(fields[4], "ημέρα της εβδομάδας (0=Κυριακή)");
            return Task.FromResult($"{minute}, {hour}, {dom}, {month}, {dow}");
        }
    }
}
