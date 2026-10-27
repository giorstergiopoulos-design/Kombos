using System;
using System.IO;

namespace Kombos.Services
{
    /// <summary>Ο φάκελος δεδομένων της εφαρμογής: %AppData%\Kombos, εκτός αν οριστεί η μεταβλητή περιβάλλοντος
    /// KOMBOS_DATA_DIR (χρησιμοποιείται για απομονωμένες δοκιμές ώστε να μην αγγίζονται οι πραγματικές ρυθμίσεις).</summary>
    public static class AppPaths
    {
        public static string Root
        {
            get
            {
                var overrideDir = Environment.GetEnvironmentVariable("KOMBOS_DATA_DIR");
                return !string.IsNullOrWhiteSpace(overrideDir)
                    ? overrideDir
                    : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Kombos");
            }
        }
    }
}
