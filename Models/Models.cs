using System.Collections.Generic;

namespace Kombos.Models
{
    public enum TransparencyLevel { None, Low, Medium }
    /// <summary>10 κινούμενα φόντα για την κεφαλίδα: Glow/Particles ήταν ήδη υπάρχοντα· Gears/Orbit/Waves/GridPulse
    /// εμπνευσμένα από τα αντίστοιχα στυλ κινούμενου φόντου του Windows Optimizer· Network/Bokeh/Aurora/Starfield
    /// νέα, φτιαγμένα για το Kombos (το Network ταιριάζει θεματικά με το λογότυπο "κόμβου").</summary>
    public enum BgAnimStyle { None, Glow, Particles, Gears, Orbit, Waves, GridPulse, Network, Bokeh, Aurora, Starfield }
    /// <summary>Οπτικό "θέμα" εμπνευσμένο από παλιότερες εκδόσεις Windows — γωνίες, σκιάσεις, glossy κουμπιά,
    /// στυλ πλακιδίων ανά εποχή, ΚΑΙ ένα σηματοδοτικό χρώμα κοντά στο αυθεντικό της κάθε εποχής (βλ.
    /// MainWindow.SkinAccentColor). Δεν χρησιμοποιεί πραγματικά DWM APIs (deprecated σε Win10/11)· είναι μια
    /// WPF-native αναπαράσταση της αισθητικής κάθε εποχής. Classic = το επίπεδο, γκρι, με ανάγλυφα πλαίσια
    /// στυλ Windows 95/98/2000/XP-classic· Nexus = πρωτότυπο στυλ του ίδιου του Kombos, με τις καρτέλες των
    /// Ρυθμίσεων σε κάθετη στήλη αντί για οριζόντια σειρά (βλ. SettingsWindow.ApplySkinLayout).</summary>
    public enum WindowsSkin { Modern, Vista, Win7, Win8, Win10, Classic, Nexus }
    /// <summary>Μέγεθος γραμματοσειράς του κειμένου των φουσκών συνομιλίας (προσβασιμότητα) - δεν επηρεάζει
    /// το υπόλοιπο UI (κουμπιά, ετικέτες κ.λπ.).</summary>
    public enum ChatTextScale { Small, Normal, Large }
    /// <summary>Light/Dark ήταν πάντα χειροκίνητο (κουμπί ήλιου/φεγγαριού) - System διαβάζει το πραγματικό
    /// θέμα των Windows (registry AppsUseLightTheme) στην εκκίνηση ΚΑΙ ζωντανά όταν αλλάζει, μέσω
    /// SystemEvents.UserPreferenceChanged (βλ. MainWindow.ApplyThemeMode).</summary>
    public enum AppThemeMode { Light, Dark, System }

    /// <summary>Ένα έτοιμο σημείο εκκίνησης στη "Βιβλιοθήκη Prompts" (v4.1.0) - επιλογή το γεμίζει στο πεδίο
    /// μηνύματος, μηδενικό κόστος tokens μέχρι να το στείλεις. CategoryKey ταιριάζει με Catalog.Categories.Id
    /// ώστε να ξαναχρησιμοποιείται το ίδιο localized label/εικονίδιο, χωρίς διπλή συντήρηση.</summary>
    /// <summary>Preset/Persona (v4.5.0, ιδέα από LibreChat presets / NextChat "Masks", φυσική συνέχεια της
    /// 📚 Βιβλιοθήκης Prompts v4.1.0) - ένας αποθηκευμένος συνδυασμός κεντρικού agent + οδηγιών, εναλλαγή με
    /// ένα κλικ αντί να ξαναγράφεις τις ίδιες οδηγίες κάθε φορά.</summary>
    public class AgentPreset
    {
        public string Id { get; set; } = System.Guid.NewGuid().ToString("N");
        public string Name { get; set; } = "";
        public string CentralAgent { get; set; } = "Claude";
        public string CustomInstructions { get; set; } = "";
    }

    /// <summary>Μία καταγεγραμμένη συνεδρία χρήσης (v4.5.0, τοπικό analytics dashboard) - στιγμιότυπο των
    /// σωρευτικών tokens ανά εργαλείο ΤΗ ΣΤΙΓΜΗ που έκλεισε πραγματικά η εφαρμογή (όχι ημερήσια κοκκομετρία -
    /// θα χρειαζόταν παρακολούθηση deltas ανά ημερολογιακή ημέρα, μεγαλύτερη αλλαγή για ένα tray-resident app
    /// που μπορεί να μείνει ανοιχτό για μέρες). Ειλικρινώς "ανά συνεδρία", όχι "ανά ημέρα".</summary>
    public class UsageLogEntry
    {
        public string TimestampUtc { get; set; } = System.DateTime.UtcNow.ToString("O");
        public Dictionary<string, long> PerToolTokens { get; set; } = new();
    }

    public class PromptLibraryItem
    {
        public string CategoryKey { get; set; } = "";
        public string Title { get; set; } = "";
        public string TitleEn { get; set; } = "";
        public string TitleFr { get; set; } = "";
        public string TitleDe { get; set; } = "";
        public string Prompt { get; set; } = "";
        public string PromptEn { get; set; } = "";
        public string PromptFr { get; set; } = "";
        public string PromptDe { get; set; } = "";
    }


    /// <summary>Ορισμός μιας κατηγορίας εργαλείων (π.χ. Κείμενο, Εικόνα, Κώδικας).</summary>
    public class CategoryDef
    {
        public string Id { get; set; } = "";
        public string Label { get; set; } = "";
        public string LabelEn { get; set; } = "";
        public string LabelFr { get; set; } = "";
        public string LabelDe { get; set; } = "";
        public string Icon { get; set; } = "";
        /// <summary>Κλειδί DynamicResource brush (π.χ. "TextCatBrush") ορισμένο στα Themes/*.xaml.</summary>
        public string BrushKey { get; set; } = "";
    }

    /// <summary>Ένα εργαλείο/agent του καταλόγου.</summary>
    public class ToolItem
    {
        public string Name { get; set; } = "";
        public string CategoryId { get; set; } = "";
        public string Url { get; set; } = "";
        public string? PricingUrl { get; set; }
        /// <summary>Σύντομη περιγραφή του δωρεάν πλάνου (π.χ. "Δωρεάν πλάνο με όρια").</summary>
        public string Free { get; set; } = "";
        public string FreeEn { get; set; } = "";
        public string FreeFr { get; set; } = "";
        public string FreeDe { get; set; } = "";
        public string Description { get; set; } = "";
        public string DescriptionEn { get; set; } = "";
        public string DescriptionFr { get; set; } = "";
        public string DescriptionDe { get; set; } = "";
        public bool HasMcp { get; set; }
        public string? McpUrl { get; set; }
        /// <summary>Προαιρετικό override λογότυπου όταν το generic favicon του domain δεν αντιπροσωπεύει καλά το εργαλείο.</summary>
        public string? LogoUrl { get; set; }
        /// <summary>Αν οριστεί, το εργαλείο έχει πραγματικό OpenAI-compatible chat completions endpoint
        /// (base URL, χωρίς το /chat/completions) και μπορεί να επιλεγεί ως κεντρικός agent.</summary>
        public string? OpenAiCompatBaseUrl { get; set; }
        public string? OpenAiCompatModel { get; set; }
    }

    /// <summary>Γραμμή χρήσης (tokens) ανά εργαλείο, για τη γραμμή κατάστασης.</summary>
    public class UsageRow
    {
        public string Name { get; set; } = "";
        /// <summary>True μόνο όταν υπάρχει πραγματική σύνδεση/API key για το εργαλείο.</summary>
        public bool IsConnected { get; set; }
        public long Used { get; set; }
        public long? Limit { get; set; }
        /// <summary>Μονάδα μέτρησης του Used (π.χ. "tokens", "characters") για σωστή ετικέτα στη γραμμή κατάστασης.</summary>
        public string Unit { get; set; } = "tokens";
    }

    /// <summary>Πρόταση εργαλείου που επιστρέφει ο Claude μέσα στην JSON απάντησή του.</summary>
    public class ToolRecommendation
    {
        public string ToolName { get; set; } = "";
        public string Why { get; set; } = "";
    }

    /// <summary>Ένα μήνυμα μέσα σε συζήτηση - χρησιμοποιείται και ως το ζωντανό _history στο MainWindow (class,
    /// όχι tuple, ΕΠΙΤΗΔΕΣ - επιτρέπει σε ένα μήνυμα-φούσκα να κρατά αναφορά στο ΣΥΓΚΕΚΡΙΜΕΝΟ ChatTurn του και
    /// να το αφαιρεί με _history.Remove(turn) κατά τη διαγραφή, χωρίς να βασίζεται σε ευθραυστο index).</summary>
    public class ChatTurn
    {
        public string Role { get; set; } = "";
        public string Content { get; set; } = "";

        // Επιτρέπει σε υπάρχοντα σημεία κώδικα να συνεχίσουν να γράφουν "var (role, content) = turn;" /
        // "foreach (var (role, content) in history)" ακριβώς όπως έκαναν με το παλιό (string,string) tuple.
        public void Deconstruct(out string role, out string content)
        {
            role = Role;
            content = Content;
        }
    }

    /// <summary>Μία αποθηκευμένη συζήτηση - επιτρέπει πολλαπλές, ξεχωριστές συνομιλίες αντί για μία συνεχόμενη.</summary>
    public class Conversation
    {
        public string Id { get; set; } = System.Guid.NewGuid().ToString("N");
        public string Title { get; set; } = "";
        public List<ChatTurn> History { get; set; } = new();
        public string UpdatedAtUtc { get; set; } = System.DateTime.UtcNow.ToString("O");
        /// <summary>Προαιρετικό Project (v3.9.0, στυλ Claude Desktop) που ανήκει αυτή η συζήτηση - κενό/null
        /// σημαίνει "χωρίς project", ίδιο με το προεπιλεγμένο συμπεριφορά πριν υπάρξουν καθόλου Projects.</summary>
        public string? ProjectId { get; set; }
    }

    /// <summary>Ένα ονομασμένο Project (v3.9.0) - ομαδοποιεί συζητήσεις κάτω από ένα κοινό όνομα και
    /// προαιρετικές δικές του οδηγίες, στυλ Claude Desktop Projects. Ανεξάρτητο από το ανά-φάκελο "Project
    /// Memory" (v3.6.0, βλ. ProjectMemoryStore) που αφορά αποκλειστικά εισηγμένο τοπικό κώδικα/αρχεία.</summary>
    public class KombosProject
    {
        public string Id { get; set; } = System.Guid.NewGuid().ToString("N");
        public string Name { get; set; } = "";
        public string Instructions { get; set; } = "";
        public string CreatedAtUtc { get; set; } = System.DateTime.UtcNow.ToString("O");
    }
}
