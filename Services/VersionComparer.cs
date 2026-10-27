using System;

namespace Kombos.Services
{
    /// <summary>Σημασιολογική (semver) σύγκριση εκδόσεων για tags τύπου "v4.9.0" / "4.9.0-beta.1".
    /// Οι pre-release εκδόσεις ΔΕΝ θεωρούνται ποτέ "νεότερες" προς ειδοποίηση.</summary>
    public static class VersionComparer
    {
        public static bool TryParse(string? text, out int[] core, out string pre)
        {
            core = Array.Empty<int>();
            pre = "";
            if (string.IsNullOrWhiteSpace(text)) return false;
            var s = text.Trim().TrimStart('v', 'V');
            var plus = s.IndexOf('+');
            if (plus >= 0) s = s[..plus];
            var dash = s.IndexOf('-');
            if (dash >= 0) { pre = s[(dash + 1)..]; s = s[..dash]; }
            var parts = s.Split('.');
            if (parts.Length < 1 || parts.Length > 4) return false;
            var nums = new int[4];
            for (int i = 0; i < parts.Length; i++)
                if (!int.TryParse(parts[i], System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out nums[i])) return false;
            core = nums;
            return true;
        }

        public static bool IsPreRelease(string? text) => TryParse(text, out _, out var pre) && pre.Length > 0;

        /// <summary>&lt;0 αν a&lt;b, 0 ίσα, &gt;0 αν a&gt;b. Μη έγκυρες εκδόσεις θεωρούνται 0.0.0.0.</summary>
        public static int Compare(string? a, string? b)
        {
            TryParse(a, out var ca, out var pa);
            TryParse(b, out var cb, out var pb);
            ca = ca.Length == 0 ? new int[4] : ca;
            cb = cb.Length == 0 ? new int[4] : cb;
            for (int i = 0; i < 4; i++)
                if (ca[i] != cb[i]) return ca[i].CompareTo(cb[i]);
            if (pa.Length == 0 && pb.Length == 0) return 0;
            if (pa.Length == 0) return 1;   // σταθερή > pre-release με ίδιο πυρήνα
            if (pb.Length == 0) return -1;
            return string.CompareOrdinal(pa, pb);
        }

        /// <summary>True αν το <paramref name="latestTag"/> είναι έγκυρη, σταθερή έκδοση νεότερη της τρέχουσας.</summary>
        public static bool IsNewer(string? latestTag, string? current)
        {
            if (!TryParse(latestTag, out _, out var pre) || pre.Length > 0) return false;
            if (!TryParse(current, out _, out _)) return false;
            return Compare(latestTag, current) > 0;
        }
    }
}
