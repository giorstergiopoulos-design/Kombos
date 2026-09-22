using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Kombos.Services
{
    public class AppSettings
    {
        public string Language { get; set; } = "EL";
        public bool IsDark { get; set; } = false;
        /// <summary>"" = παλιά αποθηκευμένα ρυθμίσεις χωρίς αυτό το πεδίο· μεταναστεύει από το IsDark στο LoadSettings.
        /// Αλλιώς "Light"/"Dark"/"System" (Enum.ToString() του ThemeMode).</summary>
        public string ThemeMode { get; set; } = "";
        /// <summary>"owner/repo" για τον προαιρετικό, χειροκίνητο έλεγχο νέας έκδοσης (v4.5.0) - κενό
        /// σημαίνει "δεν έχει οριστεί ακόμα", ο έλεγχος απλά δεν τρέχει.</summary>
        public string UpdateCheckRepo { get; set; } = "";
        public bool SidebarRight { get; set; } = false;
        public string Transparency { get; set; } = "None";
        public string BgStyle { get; set; } = "None";
        public string CentralAgent { get; set; } = "Claude";
        public string Skin { get; set; } = "Modern";
        public string ChatTextScale { get; set; } = "Normal";
        public List<string> PinnedTools { get; set; } = new();
        public List<string> DisabledPlugins { get; set; } = new();
        public bool AlwaysOnTop { get; set; } = false;
        public bool StartWithWindows { get; set; } = false;
        public bool StartMinimized { get; set; } = false;
        public bool SoundOnReply { get; set; } = false;
        public bool OnboardingSeen { get; set; } = false;
        public string CustomInstructions { get; set; } = "";
        /// <summary>Απομακρυσμένοι MCP servers, μία γραμμή ανά server με μορφή "όνομα|https://url" - μόνο για
        /// τον Claude (mcp_servers param του Messages API). Απλή μορφή αντί για πλήρες UI λίστας, σκόπιμα.</summary>
        public string McpServersRaw { get; set; } = "";
        /// <summary>Προαιρετικός φάκελος (π.χ. OneDrive/Dropbox/δικτυακός δίσκος) όπου αποθηκεύονται οι
        /// συζητήσεις ΑΝΤΙ για το προεπιλεγμένο %AppData% - επιτρέπει να τις μοιράζεσαι ανάμεσα σε
        /// υπολογιστές μέσω ενός ήδη υπαρκτού μηχανισμού συγχρονισμού αρχείων. Κενό = προεπιλογή.</summary>
        public string ConversationsSyncFolder { get; set; } = "";
        /// <summary>Κλειδιά API ανά όνομα εργαλείου/agent. Το SettingsStore τα κρυπτογραφεί με DPAPI (δεμένα
        /// στον τρέχοντα λογαριασμό Windows) πριν τα γράψει στο δίσκο - εδώ, στη μνήμη, παραμένουν πάντα σε
        /// καθαρό κείμενο.</summary>
        public Dictionary<string, string> ApiKeys { get; set; } = new();
        /// <summary>Προαιρετικό username ανά εργαλείο/agent - απλή τοπική υπενθύμιση για λογαριασμούς που
        /// συνδέεσαι μέσω browser (όχι API key), ΔΕΝ χρησιμοποιείται από το Kombos για αυτόματη σύνδεση πουθενά.</summary>
        public Dictionary<string, string> AgentUsernames { get; set; } = new();
        /// <summary>Προαιρετικό password ανά εργαλείο/agent - ίδια λογική με το username, κρυπτογραφημένο με
        /// DPAPI στο δίσκο ακριβώς όπως τα ApiKeys. Καθαρά τοπική αποθήκευση για δική σου αναφορά· το Kombos
        /// δεν το στέλνει πουθενά και δεν το συμπληρώνει αυτόματα σε καμία φόρμα ιστοσελίδας.</summary>
        public Dictionary<string, string> AgentPasswords { get; set; } = new();
        /// <summary>Ο φάκελος του τρέχοντος "ενεργού" project (Project Memory, v3.6.0) - μαζί με το τοπικό
        /// ευρετήριο σε %AppData%\Kombos\projects\, επιτρέπει στοχευμένα /readfile και /verify χωρίς να
        /// χρειάζεται να ξαναγίνει "εισαγωγή φακέλου" σε κάθε επανεκκίνηση.</summary>
        public string ActiveProjectPath { get; set; } = "";
    }

    /// <summary>Αποθηκεύει ρυθμίσεις χρήστη (γλώσσα, θέμα, θέση sidebar) σε %AppData%\Kombos\settings.json,
    /// ανεξάρτητα από τον φάκελο bin/ που σβήνεται σε κάθε rebuild.</summary>
    public static class SettingsStore
    {
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Kombos", "settings.json");

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null)
                    {
                        var decrypted = new Dictionary<string, string>();
                        foreach (var kv in settings.ApiKeys)
                            // TryUnprotect() -> null σημαίνει παλιό (προ-v2.3.0) κλειδί σε καθαρό κείμενο, ή ένα
                            // settings.json που μεταφέρθηκε σε άλλο λογαριασμό/μηχάνημα όπου το DPAPI δεν μπορεί
                            // να το αποκρυπτογραφήσει - στην πράξη σχεδόν πάντα το πρώτο (μετανάστευση), οπότε
                            // περνάμε την τιμή ως έχει αντί να σβήσουμε σιωπηλά ένα υπαρκτό κλειδί του χρήστη.
                            // Θα κρυπτογραφηθεί κανονικά στην επόμενη αποθήκευση.
                            decrypted[kv.Key] = DpapiProtector.TryUnprotect(kv.Value) ?? kv.Value;
                        settings.ApiKeys = decrypted;

                        var decryptedPasswords = new Dictionary<string, string>();
                        foreach (var kv in settings.AgentPasswords)
                            decryptedPasswords[kv.Key] = DpapiProtector.TryUnprotect(kv.Value) ?? kv.Value;
                        settings.AgentPasswords = decryptedPasswords;
                        return settings;
                    }
                }
            }
            catch { /* corrupt/missing file - use defaults */ }
            return new AppSettings();
        }

        public static void Save(AppSettings settings)
        {
            try
            {
                var encryptedKeys = new Dictionary<string, string>();
                foreach (var kv in settings.ApiKeys)
                    if (!string.IsNullOrEmpty(kv.Value)) encryptedKeys[kv.Key] = DpapiProtector.Protect(kv.Value);
                var plainKeys = settings.ApiKeys;
                settings.ApiKeys = encryptedKeys;

                var encryptedPasswords = new Dictionary<string, string>();
                foreach (var kv in settings.AgentPasswords)
                    if (!string.IsNullOrEmpty(kv.Value)) encryptedPasswords[kv.Key] = DpapiProtector.Protect(kv.Value);
                var plainPasswords = settings.AgentPasswords;
                settings.AgentPasswords = encryptedPasswords;

                var dir = Path.GetDirectoryName(FilePath)!;
                Directory.CreateDirectory(dir);
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, json);

                settings.ApiKeys = plainKeys; // δεν αγγίζουμε το αντικείμενο του καλούντος μόνιμα
                settings.AgentPasswords = plainPasswords;
            }
            catch { /* best-effort - not critical if it fails */ }
        }
    }
}
