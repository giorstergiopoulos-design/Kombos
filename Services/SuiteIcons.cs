using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Kombos.Services
{
    /// <summary>Τα πραγματικά εικονίδια των εφαρμογών της σουίτας, ενσωματωμένα ως resources (.ico πολλαπλών
    /// μεγεθών) - ποτέ από το δίκτυο. Το γράμμα-πλακίδιο μένει μόνο ως fallback για άγνωστη μελλοντική εφαρμογή.</summary>
    public static class SuiteIcons
    {
        public static string ResourceName(string id) => $"Kombos.Resources.suite.{id}.ico";

        public static bool Has(string id) =>
            Assembly.GetExecutingAssembly().GetManifestResourceNames().Contains(ResourceName(id));

        /// <summary>Επιλέγει το μικρότερο πλαίσιο του .ico με πλάτος ≥ pixels (ώστε να είναι καθαρό σε 100/150/200 % DPI),
        /// αλλιώς το μεγαλύτερο διαθέσιμο.</summary>
        public static BitmapSource? Load(string id, int pixels)
        {
            try
            {
                using var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName(id));
                if (s == null) return null;
                var ms = new MemoryStream();
                s.CopyTo(ms);
                ms.Position = 0;
                var decoder = new IconBitmapDecoder(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                var frames = decoder.Frames.OrderBy(f => f.PixelWidth).ToList();
                var pick = frames.FirstOrDefault(f => f.PixelWidth >= pixels) ?? frames.Last();
                pick.Freeze();
                return pick;
            }
            catch { return null; }
        }
    }
}
