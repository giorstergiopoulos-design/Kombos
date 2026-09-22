using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Kombos.Plugins;

namespace KombosBusinessToolkit
{
    /// <summary>Επίσημο πακέτο εργαλείων για γραφείο/λογιστική/οικονομικά - όλα τρέχουν 100% τοπικά, μηδενικό
    /// κόστος tokens. Μέρος της v3.8.0 "Εργαλειοθήκη Επιχειρήσεων" (Plugin Pack #5).</summary>
    public class BusinessToolkitPlugin : IKombosPlugin
    {
        public string Name => "Εργαλειοθήκη Επιχειρήσεων";
        public string Version => "1.0.0";

        public void Initialize(IKombosHost host)
        {
            host.RegisterSlashCommand("/vat", VatAsync);
            host.RegisterSlashCommand("/loan", LoanAsync);
            host.RegisterSlashCommand("/percent", PercentAsync);
            host.RegisterSlashCommand("/workdays", WorkdaysAsync);
            host.RegisterSlashCommand("/iban", IbanAsync);
            host.RegisterSlashCommand("/invoice-number", InvoiceNumberAsync);
            host.Log("Business Toolkit plugin initialized (6 commands).");
        }

        private static Task<string> VatAsync(string arg)
        {
            var parts = arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0 || !double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                return Task.FromResult("Χρήση: /vat <ποσό> [συντελεστής%, προεπιλογή 24]  (δείχνει και τις 2 κατευθύνσεις: καθαρό→μικτό, μικτό→καθαρό)");
            var rate = parts.Length > 1 && double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var r) ? r : 24.0;

            var vatIfNet = amount * rate / 100.0;
            var grossFromNet = amount + vatIfNet;
            var netFromGross = amount / (1 + rate / 100.0);
            var vatFromGross = amount - netFromGross;

            return Task.FromResult(
                $"ΦΠΑ {rate:0.##}%\n" +
                $"Αν {amount:0.##} είναι καθαρή αξία: ΦΠΑ = {vatIfNet:0.00}, μικτό = {grossFromNet:0.00}\n" +
                $"Αν {amount:0.##} είναι μικτή αξία: καθαρό = {netFromGross:0.00}, ΦΠΑ = {vatFromGross:0.00}");
        }

        private static Task<string> LoanAsync(string arg)
        {
            var parts = arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3
                || !double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var principal)
                || !double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var annualRatePct)
                || !int.TryParse(parts[2], out var months) || months <= 0)
                return Task.FromResult("Χρήση: /loan <κεφάλαιο> <ετήσιο επιτόκιο%> <μήνες>  (π.χ. /loan 10000 5 24)");

            var monthlyRate = annualRatePct / 100.0 / 12.0;
            double payment;
            if (monthlyRate == 0) payment = principal / months;
            else payment = principal * monthlyRate / (1 - Math.Pow(1 + monthlyRate, -months));

            var totalPaid = payment * months;
            var totalInterest = totalPaid - principal;

            return Task.FromResult(
                $"Μηνιαία δόση: {payment:0.00}\n" +
                $"Συνολικό κόστος: {totalPaid:0.00}\n" +
                $"Σύνολο τόκων: {totalInterest:0.00}");
        }

        private static Task<string> PercentAsync(string arg)
        {
            var parts = arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            // /percent <X> of <Y>  -> X% του Y
            if (parts.Length == 3 && parts[1].Equals("of", StringComparison.OrdinalIgnoreCase)
                && double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var pct)
                && double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var baseVal))
            {
                return Task.FromResult($"{pct:0.##}% του {baseVal:0.##} = {(pct / 100.0 * baseVal):0.00}");
            }
            // /percent change <A> <B> -> ποσοστιαία μεταβολή από Α σε Β
            if (parts.Length == 3 && parts[0].Equals("change", StringComparison.OrdinalIgnoreCase)
                && double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var a)
                && double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var b))
            {
                if (a == 0) return Task.FromResult("❌ Δεν ορίζεται μεταβολή από 0.");
                var change = (b - a) / a * 100.0;
                var sign = change >= 0 ? "+" : "";
                return Task.FromResult($"Μεταβολή από {a:0.##} σε {b:0.##}: {sign}{change:0.00}%");
            }
            return Task.FromResult("Χρήση: /percent <X> of <Y>  (π.χ. /percent 20 of 150)\nή: /percent change <A> <B>  (ποσοστιαία μεταβολή, π.χ. /percent change 100 120)");
        }

        /// <summary>Ορθόδοξο Πάσχα (αλγόριθμος Meeus, Ιουλιανό ημερολόγιο μετατρεπόμενο σε Γρηγοριανό) - " +
        /// χρειάζεται για τις κινητές αργίες (Καθαρά Δευτέρα, Μεγάλη Παρασκευή, Δευτέρα Πάσχα, Αγίου Πνεύματος).</summary>
        private static DateTime OrthodoxEaster(int year)
        {
            int a = year % 4, b = year % 7, c = year % 19;
            int d = (19 * c + 15) % 30, e = (2 * a + 4 * b - d + 34) % 7;
            int month = (d + e + 114) / 31;
            int day = (d + e + 114) % 31 + 1;
            var julian = new DateTime(year, month, day);
            // Ιουλιανό -> Γρηγοριανό: διαφορά 13 ημερών για τον 21ο/22ο αιώνα
            return julian.AddDays(13);
        }

        private static bool IsGreekHoliday(DateTime date)
        {
            var fixedHolidays = new (int Month, int Day)[]
            {
                (1, 1), (1, 6), (3, 25), (5, 1), (8, 15), (10, 28), (12, 25), (12, 26)
            };
            if (fixedHolidays.Any(h => h.Month == date.Month && h.Day == date.Day)) return true;

            var easter = OrthodoxEaster(date.Year);
            var movable = new[] { easter.AddDays(-48), easter.AddDays(-2), easter.AddDays(1), easter.AddDays(50) };
            return movable.Any(m => m.Date == date.Date);
        }

        private static Task<string> WorkdaysAsync(string arg)
        {
            var parts = arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2 || !DateTime.TryParse(parts[0], CultureInfo.InvariantCulture, DateTimeStyles.None, out var start)
                                   || !DateTime.TryParse(parts[1], CultureInfo.InvariantCulture, DateTimeStyles.None, out var end))
                return Task.FromResult("Χρήση: /workdays <ημερομηνία1> <ημερομηνία2>  (π.χ. /workdays 2026-01-01 2026-02-01) - εξαιρεί Σαββατοκύριακα και ελληνικές αργίες");

            if (end < start) (start, end) = (end, start);
            int businessDays = 0, holidaysSkipped = 0;
            for (var d = start.Date; d <= end.Date; d = d.AddDays(1))
            {
                if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday) continue;
                if (IsGreekHoliday(d)) { holidaysSkipped++; continue; }
                businessDays++;
            }
            return Task.FromResult($"Εργάσιμες ημέρες: {businessDays} (εξαιρέθηκαν {holidaysSkipped} ελληνικές αργίες σε καθημερινή)");
        }

        private static Task<string> IbanAsync(string arg)
        {
            var iban = Regex.Replace(arg.Trim().ToUpperInvariant(), @"\s+", "");
            if (iban.Length < 4) return Task.FromResult("Χρήση: /iban <κωδικός IBAN>");

            var rearranged = iban[4..] + iban[..4];
            var sb = new StringBuilder();
            foreach (var ch in rearranged)
            {
                if (char.IsDigit(ch)) sb.Append(ch);
                else if (ch is >= 'A' and <= 'Z') sb.Append(ch - 'A' + 10);
                else return Task.FromResult("❌ Μη έγκυροι χαρακτήρες στο IBAN.");
            }

            if (!BigInteger.TryParse(sb.ToString(), out var numeric))
                return Task.FromResult("❌ Δεν ήταν δυνατή η επεξεργασία του IBAN.");

            var valid = numeric % 97 == 1;
            return Task.FromResult(valid ? $"✅ Έγκυρο IBAN (checksum OK): {iban}" : $"❌ Μη έγκυρο IBAN (αποτυχία checksum): {iban}");
        }

        private static Task<string> InvoiceNumberAsync(string arg)
        {
            arg = arg.Trim();
            if (arg.Length == 0) return Task.FromResult("Χρήση: /invoice-number <τελευταίος αριθμός>  (π.χ. /invoice-number INV-000123 → επόμενο INV-000124)");

            var match = Regex.Match(arg, @"^(.*?)(\d+)$");
            if (!match.Success) return Task.FromResult("❌ Δεν βρέθηκε αριθμητικό τμήμα στο τέλος. Χρήση: /invoice-number INV-000123");

            var prefix = match.Groups[1].Value;
            var numberPart = match.Groups[2].Value;
            var next = (long.Parse(numberPart) + 1).ToString().PadLeft(numberPart.Length, '0');
            return Task.FromResult($"Επόμενος αριθμός: {prefix}{next}");
        }
    }
}
