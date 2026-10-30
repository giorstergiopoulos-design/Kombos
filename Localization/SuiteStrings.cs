using Kombos.Localization;

namespace Kombos.Localization
{
    /// <summary>Strings του οδηγού καλωσορίσματος / της βιτρίνας "Οικογένεια Opsis" / της ειδοποίησης νέας
    /// έκδοσης - 4 γλώσσες (EL/EN/FR/DE), ίδια λογική με το <see cref="L"/>.</summary>
    public static class SuiteStrings
    {
        private static string T(string el, string en, string fr, string de) => L.Current switch
        {
            Lang.EL => el, Lang.EN => en, Lang.FR => fr, _ => de
        };

        public static string CatalogLang => L.Current switch { Lang.EL => "el", Lang.FR => "fr", Lang.DE => "de", _ => "en" };

        public static string SuiteTitle => T("Η οικογένεια Opsis", "The Opsis family", "La famille Opsis", "Die Opsis-Familie");
        public static string SuiteSubtitle => T(
            "Οι εφαρμογές που φτιάχτηκαν μαζί, για Windows. Άνοιξε όποια έχεις ήδη ή κατέβασε τις υπόλοιπες από το GitHub.",
            "The apps built together, for Windows. Open the ones you already have or download the rest from GitHub.",
            "Les applications conçues ensemble pour Windows. Ouvrez celles que vous avez déjà ou téléchargez les autres depuis GitHub.",
            "Die zusammen entwickelten Apps für Windows. Öffne die bereits installierten oder lade die übrigen von GitHub.");
        public static string SuiteFooter => T(
            "Η λίστα ανανεώνεται το πολύ μία φορά την ημέρα από το GitHub. Η εγκατάσταση δεν ξεκινά ποτέ αυτόματα· κάθε λήψη ζητά επιβεβαίωση και ελέγχεται με SHA-256.",
            "The list refreshes at most once a day from GitHub. Nothing is ever installed automatically; every download asks first and is verified with SHA-256.",
            "La liste est actualisée au plus une fois par jour depuis GitHub. Rien n'est installé automatiquement ; chaque téléchargement demande confirmation et est vérifié par SHA-256.",
            "Die Liste wird höchstens einmal täglich von GitHub aktualisiert. Nichts wird automatisch installiert; jeder Download fragt nach und wird per SHA-256 geprüft.");
        public static string SuiteClose => T("Κλείσιμο", "Close", "Fermer", "Schließen");
        public static string YouAreHere => T("Βρίσκεσαι εδώ", "You are here", "Vous êtes ici", "Du bist hier");
        public static string NotInstalled => T("Δεν είναι εγκατεστημένο", "Not installed", "Non installé", "Nicht installiert");
        public static string Installed(string? v) => string.IsNullOrWhiteSpace(v)
            ? T("Εγκατεστημένο", "Installed", "Installé", "Installiert")
            : T($"Εγκατεστημένο v{v}", $"Installed v{v}", $"Installé v{v}", $"Installiert v{v}");
        public static string UpdateAvailable(string? v, string latest) =>
            T($"Διαθέσιμη ενημέρωση ({(v == null ? "" : "v" + v + " → ")}{latest})", $"Update available ({(v == null ? "" : "v" + v + " → ")}{latest})",
              $"Mise à jour disponible ({(v == null ? "" : "v" + v + " → ")}{latest})", $"Update verfügbar ({(v == null ? "" : "v" + v + " → ")}{latest})");
        public static string Open => T("Άνοιγμα", "Open", "Ouvrir", "Öffnen");
        public static string Install => T("Λήψη", "Download", "Télécharger", "Herunterladen");
        public static string Update => T("Ενημέρωση", "Update", "Mettre à jour", "Aktualisieren");
        public static string Cancel => T("Ακύρωση", "Cancel", "Annuler", "Abbrechen");
        public static string AppNotFound(string path) => T($"Δεν βρέθηκε το εκτελέσιμο: {path}", $"Executable not found: {path}", $"Exécutable introuvable : {path}", $"Programmdatei nicht gefunden: {path}");
        public static string Details => T("Λεπτομέρειες", "Details", "Détails", "Details");
        public static string Checking => T("Έλεγχος…", "Checking…", "Vérification…", "Prüfe…");
        public static string Downloading(int pct) => T($"Λήψη… {pct}%", $"Downloading… {pct}%", $"Téléchargement… {pct}%", $"Lade herunter… {pct}%");

        public static string ConfirmDownload(string file, string size, string source) => T(
            $"Λήψη του αρχείου:\n\n{file}\n{size}\nΠηγή: {source}\n\nΤο αρχείο θα αποθηκευτεί στα Λήψεις και θα ελεγχθεί με SHA-256. ΔΕΝ θα εκτελεστεί αυτόματα. Συνέχεια;",
            $"Download this file:\n\n{file}\n{size}\nSource: {source}\n\nIt will be saved to Downloads and verified with SHA-256. It will NOT be run automatically. Continue?",
            $"Télécharger ce fichier :\n\n{file}\n{size}\nSource : {source}\n\nIl sera enregistré dans Téléchargements et vérifié par SHA-256. Il ne sera PAS exécuté automatiquement. Continuer ?",
            $"Diese Datei herunterladen:\n\n{file}\n{size}\nQuelle: {source}\n\nSie wird in Downloads gespeichert und per SHA-256 geprüft. Sie wird NICHT automatisch ausgeführt. Fortfahren?");
        public static string DownloadVerified(string path) => T(
            $"Η λήψη ολοκληρώθηκε και το SHA-256 επαληθεύτηκε.\n\n{path}\n\nΕκτέλεσέ το μόνος/η σου όταν θελήσεις να εγκαταστήσεις.",
            $"Download complete and SHA-256 verified.\n\n{path}\n\nRun it yourself when you want to install.",
            $"Téléchargement terminé et SHA-256 vérifié.\n\n{path}\n\nLancez-le vous-même quand vous voulez installer.",
            $"Download abgeschlossen, SHA-256 geprüft.\n\n{path}\n\nFühre die Datei selbst aus, wenn du installieren möchtest.");
        public static string ChecksumRefused => T(
            "ΑΠΟΡΡΙΦΘΗΚΕ: το SHA-256 του αρχείου δεν ταιριάζει με το δηλωμένο. Το αρχείο διαγράφηκε.",
            "REFUSED: the file's SHA-256 does not match the published one. The file was deleted.",
            "REFUSÉ : le SHA-256 du fichier ne correspond pas à celui publié. Le fichier a été supprimé.",
            "ABGELEHNT: Das SHA-256 der Datei stimmt nicht mit dem veröffentlichten überein. Die Datei wurde gelöscht.");
        public static string NoChecksum => T(
            "Αυτό το release δεν έχει δημοσιευμένο SHA-256, οπότε δεν γίνεται ασφαλής λήψη μέσα από το Kombos. Να ανοίξω τη σελίδα του release;",
            "This release has no published SHA-256, so Kombos cannot download it safely. Open the release page instead?",
            "Cette version n'a pas de SHA-256 publié ; Kombos ne peut pas la télécharger en toute sécurité. Ouvrir la page de la version ?",
            "Für dieses Release ist kein SHA-256 veröffentlicht, daher kann Kombos es nicht sicher laden. Release-Seite öffnen?");
        public static string DownloadFailed(string msg) => T($"Η λήψη απέτυχε: {msg}", $"Download failed: {msg}", $"Échec du téléchargement : {msg}", $"Download fehlgeschlagen: {msg}");

        public static string UpdateToast(string version) => T(
            $"Το Κόμβος {version} είναι διαθέσιμο.", $"Kombos {version} is available.", $"Kombos {version} est disponible.", $"Kombos {version} ist verfügbar.");
        public static string WhatsNew => T("Τι νέο υπάρχει", "What's new", "Nouveautés", "Was ist neu");
        public static string Later => T("Αργότερα", "Later", "Plus tard", "Später");
        public static string BalloonText(string version) => T(
            $"Το Κόμβος {version} είναι διαθέσιμο - πάτησε για να δεις τι νέο υπάρχει.",
            $"Kombos {version} is available - click to see what's new.",
            $"Kombos {version} est disponible - cliquez pour voir les nouveautés.",
            $"Kombos {version} ist verfügbar - klicken, um Neuigkeiten zu sehen.");
        public static string NotifyNewVersions => T("Ειδοποίηση για νέες εκδόσεις (ένας έλεγχος GitHub το 24ωρο, χωρίς τηλεμετρία)",
            "Notify about new versions (one GitHub check per day, no telemetry)",
            "Notifier des nouvelles versions (une vérification GitHub par jour, sans télémétrie)",
            "Über neue Versionen informieren (eine GitHub-Abfrage pro Tag, keine Telemetrie)");

        public static string GuideDontShow => T("Να μην εμφανιστεί ξανά", "Don't show again", "Ne plus afficher", "Nicht mehr anzeigen");
        public static string WelcomeGuide => T("Οδηγός καλωσορίσματος", "Welcome guide", "Guide de bienvenue", "Willkommensführung");
        public static string SuiteButton => T("🧩 Η οικογένεια Opsis", "🧩 The Opsis family", "🧩 La famille Opsis", "🧩 Die Opsis-Familie");
    }
}
