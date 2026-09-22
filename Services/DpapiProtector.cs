using System;
using System.Security.Cryptography;
using System.Text;

namespace Kombos.Services
{
    /// <summary>Κρυπτογραφεί μικρά κείμενα (API keys) με το DPAPI των Windows, δεμένα στον τρέχοντα λογαριασμό
    /// χρήστη - μόνο ο ίδιος λογαριασμός Windows στο ίδιο μηχάνημα μπορεί να τα αποκρυπτογραφήσει, ακόμα κι αν
    /// κάποιος αντιγράψει το settings.json αλλού. Πριν το v2.3.0 τα κλειδιά αποθηκεύονταν σε καθαρό κείμενο -
    /// το TryUnprotect επιστρέφει null για τέτοιες παλιές τιμές ώστε ο καλών να τις αντιμετωπίσει ως ήδη plaintext
    /// (μεταναστεύονται αυτόματα σε κρυπτογραφημένη μορφή στην επόμενη αποθήκευση).</summary>
    internal static class DpapiProtector
    {
        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("Kombos.ApiKeys.v1");

        internal static string Protect(string plainText)
        {
            var bytes = Encoding.UTF8.GetBytes(plainText);
            var protectedBytes = ProtectedData.Protect(bytes, Entropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(protectedBytes);
        }

        internal static string? TryUnprotect(string protectedText)
        {
            try
            {
                var protectedBytes = Convert.FromBase64String(protectedText);
                var bytes = ProtectedData.Unprotect(protectedBytes, Entropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(bytes);
            }
            catch { return null; }
        }
    }
}
