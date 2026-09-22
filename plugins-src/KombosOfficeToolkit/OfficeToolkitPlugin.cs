using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Kombos.Plugins;

namespace KombosOfficeToolkit
{
    /// <summary>Επίσημο πακέτο εργαλείων για ΚΑΘΕ είδος εργασίας (όχι μόνο developers) - καθαρό κείμενο,
    /// καμία κλήση σε AI/δίκτυο, μηδενικό κόστος tokens. Μέρος της "Λήψη &amp; Ενσωμάτωση" γκαλερί plugins.</summary>
    public class OfficeToolkitPlugin : IKombosPlugin
    {
        public string Name => "Εργαλειοθήκη Γραφείου";
        public string Version => "1.0.0";

        public void Initialize(IKombosHost host)
        {
            host.RegisterSlashCommand("/wordcount", WordCountAsync);
            host.RegisterSlashCommand("/lorem", LoremAsync);
            host.RegisterSlashCommand("/password", PasswordAsync);
            host.RegisterSlashCommand("/diff", DiffAsync);
            host.RegisterSlashCommand("/case", CaseAsync);
            host.Log("Office Toolkit plugin initialized (5 commands).");
        }

        private static Task<string> WordCountAsync(string arg)
        {
            if (string.IsNullOrEmpty(arg)) return Task.FromResult("Χρήση: /wordcount <κείμενο>");
            var words = arg.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            var chars = arg.Length;
            var charsNoSpaces = arg.Count(c => !char.IsWhiteSpace(c));
            var lines = arg.Split('\n').Length;
            return Task.FromResult($"Λέξεις: {words} · Χαρακτήρες: {chars} (χωρίς κενά: {charsNoSpaces}) · Γραμμές: {lines}");
        }

        private static readonly string[] LoremWords =
        {
            "lorem","ipsum","dolor","sit","amet","consectetur","adipiscing","elit","sed","do",
            "eiusmod","tempor","incididunt","ut","labore","et","dolore","magna","aliqua","enim",
            "ad","minim","veniam","quis","nostrud","exercitation","ullamco","laboris","nisi","aliquip"
        };

        private static Task<string> LoremAsync(string arg)
        {
            int count = 30;
            if (!string.IsNullOrWhiteSpace(arg) && int.TryParse(arg.Trim(), out var n)) count = Math.Clamp(n, 1, 500);
            var rnd = new Random();
            var words = Enumerable.Range(0, count).Select(_ => LoremWords[rnd.Next(LoremWords.Length)]);
            var text = string.Join(' ', words);
            return Task.FromResult(char.ToUpperInvariant(text[0]) + text[1..] + ".");
        }

        private static Task<string> PasswordAsync(string arg)
        {
            int length = 16;
            if (!string.IsNullOrWhiteSpace(arg) && int.TryParse(arg.Trim(), out var n)) length = Math.Clamp(n, 8, 128);
            const string chars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789!@#$%^&*-_=+";
            var bytes = RandomNumberGenerator.GetBytes(length);
            var sb = new StringBuilder(length);
            foreach (var b in bytes) sb.Append(chars[b % chars.Length]);
            return Task.FromResult($"`{sb}`\n({length} χαρακτήρες)");
        }

        private static Task<string> DiffAsync(string arg)
        {
            var parts = arg.Split('|', 2);
            if (parts.Length < 2) return Task.FromResult("Χρήση: /diff <κείμενο Α>|<κείμενο Β>");
            var linesA = parts[0].Split('\n');
            var linesB = parts[1].Split('\n');
            var sb = new StringBuilder();
            int max = Math.Max(linesA.Length, linesB.Length);
            bool anyDiff = false;
            for (int i = 0; i < max; i++)
            {
                var a = i < linesA.Length ? linesA[i] : "";
                var b = i < linesB.Length ? linesB[i] : "";
                if (a == b) continue;
                anyDiff = true;
                sb.AppendLine($"Γραμμή {i + 1}:");
                sb.AppendLine($"  - {a}");
                sb.AppendLine($"  + {b}");
            }
            return Task.FromResult(anyDiff ? sb.ToString().TrimEnd() : "Τα δύο κείμενα είναι ίδια.");
        }

        private static Task<string> CaseAsync(string arg)
        {
            var parts = arg.Split(' ', 2);
            if (parts.Length < 2) return Task.FromResult("Χρήση: /case upper|lower|title|snake|camel <κείμενο>");
            var mode = parts[0].ToLowerInvariant();
            var text = parts[1];
            string result = mode switch
            {
                "upper" => text.ToUpperInvariant(),
                "lower" => text.ToLowerInvariant(),
                "title" => string.Join(' ', text.Split(' ').Select(w => w.Length > 0 ? char.ToUpperInvariant(w[0]) + w[1..].ToLowerInvariant() : w)),
                "snake" => string.Join('_', text.Split(' ', StringSplitOptions.RemoveEmptyEntries)).ToLowerInvariant(),
                "camel" => ToCamelCase(text),
                _ => "Χρήση: /case upper|lower|title|snake|camel <κείμενο>"
            };
            return Task.FromResult(result);
        }

        private static string ToCamelCase(string text)
        {
            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0) return text;
            var sb = new StringBuilder(words[0].ToLowerInvariant());
            for (int i = 1; i < words.Length; i++)
                sb.Append(char.ToUpperInvariant(words[i][0])).Append(words[i][1..].ToLowerInvariant());
            return sb.ToString();
        }
    }
}
