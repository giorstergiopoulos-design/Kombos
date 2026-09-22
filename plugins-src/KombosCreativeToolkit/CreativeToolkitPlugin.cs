using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

using Kombos.Plugins;

namespace KombosCreativeToolkit
{
    /// <summary>Επίσημο πακέτο εργαλείων για δημιουργικούς/media χρήστες (σχεδιαστές, marketers, μεταφορά
    /// περιεχομένου σε web/app) - όλα τρέχουν 100% τοπικά, μηδενικό κόστος tokens. Μέρος της v3.7.0
    /// "Δημιουργική Εργαλειοθήκη" (Plugin Pack #4).</summary>
    public class CreativeToolkitPlugin : IKombosPlugin
    {
        public string Name => "Δημιουργική Εργαλειοθήκη";
        public string Version => "1.0.0";

        public void Initialize(IKombosHost host)
        {
            host.RegisterSlashCommand("/gradient", GradientAsync);
            host.RegisterSlashCommand("/aspect-ratio", AspectRatioAsync);
            host.RegisterSlashCommand("/palette", PaletteAsync);
            host.RegisterSlashCommand("/contrast", ContrastAsync);
            host.RegisterSlashCommand("/favicon-checklist", FaviconChecklistAsync);
            host.Log("Creative Toolkit plugin initialized (5 commands).");
        }

        private static (byte R, byte G, byte B)? ParseHex(string hex)
        {
            hex = hex.Trim().TrimStart('#');
            if (hex.Length == 3) hex = string.Concat(hex[0], hex[0], hex[1], hex[1], hex[2], hex[2]);
            if (hex.Length != 6) return null;
            try
            {
                return (Convert.ToByte(hex[..2], 16), Convert.ToByte(hex[2..4], 16), Convert.ToByte(hex[4..6], 16));
            }
            catch { return null; }
        }

        private static string ToHex((byte R, byte G, byte B) c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

        private static Task<string> GradientAsync(string arg)
        {
            var parts = arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) return Task.FromResult("Χρήση: /gradient <hex1> <hex2> [γωνία σε μοίρες, προεπιλογή 90]");
            var c1 = ParseHex(parts[0]);
            var c2 = ParseHex(parts[1]);
            if (c1 == null || c2 == null) return Task.FromResult("❌ Μη έγκυρα χρώματα. Χρήση: /gradient #RRGGBB #RRGGBB [γωνία]");
            var angle = parts.Length > 2 && double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var a) ? a : 90;

            var sb = new StringBuilder();
            sb.AppendLine($"CSS: linear-gradient({angle:0}deg, {ToHex(c1.Value)}, {ToHex(c2.Value)})");
            sb.AppendLine("Ενδιάμεσα σημεία:");
            for (int i = 0; i <= 4; i++)
            {
                double t = i / 4.0;
                byte Lerp(byte a1, byte b1) => (byte)Math.Round(a1 + (b1 - a1) * t);
                var mid = (Lerp(c1.Value.R, c2.Value.R), Lerp(c1.Value.G, c2.Value.G), Lerp(c1.Value.B, c2.Value.B));
                sb.AppendLine($"  {t * 100:0}%: {ToHex(mid)}");
            }
            return Task.FromResult(sb.ToString().TrimEnd());
        }

        private static Task<string> AspectRatioAsync(string arg)
        {
            arg = arg.Trim();
            var sep = arg.Contains('x') ? 'x' : (arg.Contains(':') ? ':' : '\0');
            if (sep == '\0') return Task.FromResult("Χρήση: /aspect-ratio <πλάτος>x<ύψος>  (π.χ. /aspect-ratio 1920x1080)");
            var parts = arg.Split(sep);
            if (parts.Length != 2 || !double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var w)
                                   || !double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var h) || w <= 0 || h <= 0)
                return Task.FromResult("❌ Μη έγκυρες διαστάσεις. Χρήση: /aspect-ratio 1920x1080");

            static long Gcd(long a, long b) => b == 0 ? a : Gcd(b, a % b);
            var g = Gcd((long)Math.Round(w), (long)Math.Round(h));
            var ratioW = (long)Math.Round(w) / Math.Max(1, g);
            var ratioH = (long)Math.Round(h) / Math.Max(1, g);

            var sb = new StringBuilder();
            sb.AppendLine($"Αναλογία: {ratioW}:{ratioH}");
            sb.AppendLine("Κοινά μεγέθη στην ίδια αναλογία:");
            foreach (var targetW in new[] { 3840, 1920, 1280, 1080, 720, 640 })
            {
                var targetH = Math.Round(targetW * h / w);
                sb.AppendLine($"  {targetW} x {targetH:0}");
            }
            return Task.FromResult(sb.ToString().TrimEnd());
        }

        private static Task<string> PaletteAsync(string arg)
        {
            var seed = ParseHex(arg);
            if (seed == null) return Task.FromResult("Χρήση: /palette <hex>  (π.χ. /palette #3d95e0) - δίνει complementary + 2 shades + 2 tints");
            double rf = seed.Value.R / 255.0, gf = seed.Value.G / 255.0, bf = seed.Value.B / 255.0;
            double max = Math.Max(rf, Math.Max(gf, bf)), min = Math.Min(rf, Math.Min(gf, bf));
            double h = 0, l = (max + min) / 2, d = max - min;
            double s = d == 0 ? 0 : d / (1 - Math.Abs(2 * l - 1));
            if (d != 0)
            {
                if (max == rf) h = 60 * (((gf - bf) / d) % 6);
                else if (max == gf) h = 60 * ((bf - rf) / d + 2);
                else h = 60 * ((rf - gf) / d + 4);
            }
            if (h < 0) h += 360;

            (byte, byte, byte) FromHsl(double hh, double ss, double ll)
            {
                double c = (1 - Math.Abs(2 * ll - 1)) * ss;
                double x = c * (1 - Math.Abs((hh / 60) % 2 - 1));
                double m = ll - c / 2;
                (double r1, double g1, double b1) = hh < 60 ? (c, x, 0.0) : hh < 120 ? (x, c, 0.0) : hh < 180 ? (0.0, c, x)
                    : hh < 240 ? (0.0, x, c) : hh < 300 ? (x, 0.0, c) : (c, 0.0, x);
                return ((byte)Math.Round((r1 + m) * 255), (byte)Math.Round((g1 + m) * 255), (byte)Math.Round((b1 + m) * 255));
            }

            var complementary = FromHsl((h + 180) % 360, s, l);
            var shade = FromHsl(h, s, Math.Max(0, l - 0.2));
            var darkShade = FromHsl(h, s, Math.Max(0, l - 0.35));
            var tint = FromHsl(h, s, Math.Min(1, l + 0.2));
            var lightTint = FromHsl(h, s, Math.Min(1, l + 0.35));

            return Task.FromResult(
                $"Βάση: {ToHex(seed.Value)}\n" +
                $"Complementary: {ToHex(complementary)}\n" +
                $"Σκούρο: {ToHex(shade)}  ·  Πιο σκούρο: {ToHex(darkShade)}\n" +
                $"Ανοιχτό: {ToHex(tint)}  ·  Πιο ανοιχτό: {ToHex(lightTint)}");
        }

        private static Task<string> ContrastAsync(string arg)
        {
            var parts = arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) return Task.FromResult("Χρήση: /contrast <hex1> <hex2>  (υπολογίζει τον λόγο αντίθεσης WCAG)");
            var c1 = ParseHex(parts[0]);
            var c2 = ParseHex(parts[1]);
            if (c1 == null || c2 == null) return Task.FromResult("❌ Μη έγκυρα χρώματα. Χρήση: /contrast #RRGGBB #RRGGBB");

            double Luminance((byte R, byte G, byte B) c)
            {
                double Lin(byte ch) { double v = ch / 255.0; return v <= 0.03928 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4); }
                return 0.2126 * Lin(c.R) + 0.7152 * Lin(c.G) + 0.0722 * Lin(c.B);
            }

            var l1 = Luminance(c1.Value);
            var l2 = Luminance(c2.Value);
            var ratio = (Math.Max(l1, l2) + 0.05) / (Math.Min(l1, l2) + 0.05);
            var aaNormal = ratio >= 4.5 ? "✅ περνάει" : "❌ αποτυγχάνει";
            var aaLarge = ratio >= 3.0 ? "✅ περνάει" : "❌ αποτυγχάνει";
            var aaa = ratio >= 7.0 ? "✅ περνάει" : "❌ αποτυγχάνει";
            return Task.FromResult(
                $"Λόγος αντίθεσης: {ratio:0.00}:1\n" +
                $"WCAG AA (κανονικό κείμενο, ≥4.5): {aaNormal}\n" +
                $"WCAG AA (μεγάλο κείμενο, ≥3.0): {aaLarge}\n" +
                $"WCAG AAA (κανονικό κείμενο, ≥7.0): {aaa}");
        }

        private static Task<string> FaviconChecklistAsync(string arg) => Task.FromResult(
            "Μεγέθη εικονιδίων για δημοσίευση web/app:\n" +
            "  favicon.ico: 16x16, 32x32, 48x48 (πολλαπλά μεγέθη στο ίδιο .ico)\n" +
            "  apple-touch-icon: 180x180\n" +
            "  android-chrome: 192x192, 512x512\n" +
            "  Windows tile: 150x150\n" +
            "  Open Graph (κοινωνικά δίκτυα): 1200x630\n" +
            "  Twitter card: 1200x675 (summary_large_image)\n" +
            "  App Store (iOS): 1024x1024\n" +
            "  Google Play: 512x512");
    }
}
