using Kombos.Models;

namespace Kombos.Localization
{
    public enum Lang { EL, EN, FR, DE }

    /// <summary>Στατικά UI strings σε 4 γλώσσες. Το <see cref="Current"/> ελέγχει τη γλώσσα.
    /// Οι περιγραφές εργαλείων του καταλόγου υπάρχουν μόνο σε EL/EN· για FR/DE γίνεται fallback
    /// στα αγγλικά μέσω <see cref="PickToolText"/> — από v1.3.0 πλέον πλήρεις μεταφράσεις σε EL/EN/FR/DE.</summary>
    public static class L
    {
        public static Lang Current { get; set; } = Lang.EL;
        public const string AppVersion = "4.8.1";

        public static Lang Next => Current switch
        {
            Lang.EL => Lang.EN,
            Lang.EN => Lang.FR,
            Lang.FR => Lang.DE,
            _ => Lang.EL
        };

        public static string FlagFor(Lang l) => l switch
        {
            Lang.EL => "\U0001F1EC\U0001F1F7",
            Lang.EN => "\U0001F1EC\U0001F1E7",
            Lang.FR => "\U0001F1EB\U0001F1F7",
            _ => "\U0001F1E9\U0001F1EA"
        };

        public static string LangToggleContent => FlagFor(Current);
        public static string LangToggleTooltip => Current switch
        {
            Lang.EL => "Επιλογή γλώσσας",
            Lang.EN => "Select language",
            Lang.FR => "Choisir la langue",
            _ => "Sprache auswählen"
        };

        public static string PickToolText(string el, string en, string fr, string de) => Current switch
        {
            Lang.EL => el,
            Lang.EN => en,
            Lang.FR => fr,
            _ => de
        };

        public static string WindowTitle => Current switch
        {
            Lang.EL => "Κόμβος — Claude & η ομάδα agents",
            Lang.EN => "Kombos — Claude & the agent team",
            Lang.FR => "Kombos — Claude & l'équipe d'agents",
            _ => "Kombos — Claude & das Agenten-Team"
        };

        public static string HeaderTitle => Current == Lang.EL ? "Κόμβος" : "Kombos";

        public static string HeaderSubtitle(string agentName) => Current switch
        {
            Lang.EL => $"Ο {agentName} είναι ο κεντρικός agent. Μιλάς μαζί του — φέρνει τους κατάλληλους agents ανάλογα με το task, είτε λειτουργούν μέσω ιστοσελίδας είτε ως MCP connector μέσα στο Claude Desktop.",
            Lang.EN => $"{agentName} is the central agent. Talk to him — he brings in the right agents for the task, whether they work through a website or as an MCP connector inside Claude Desktop.",
            Lang.FR => $"{agentName} est l'agent central. Parle-lui — il fait appel aux agents adaptés à la tâche, qu'ils fonctionnent via un site web ou comme connecteur MCP dans Claude Desktop.",
            _ => $"{agentName} ist der zentrale Agent. Sprich mit ihm — er zieht die passenden Agenten für die Aufgabe hinzu, egal ob über eine Website oder als MCP-Connector in Claude Desktop."
        };

        public static string ApiKeyHint => Current switch
        {
            Lang.EL => "Ρυθμίσεις agents / κλειδιά API: πάτησε το εικονίδιο \"C\" πάνω-αριστερά.",
            Lang.EN => "Agent settings / API keys: click the \"C\" icon at the top left.",
            Lang.FR => "Paramètres des agents / clés API : clique sur l'icône « C » en haut à gauche.",
            _ => "Agent-Einstellungen / API-Schlüssel: klicke oben links auf das „C“-Symbol."
        };

        public static string SettingsTitle => Current switch
        {
            Lang.EL => "Ρυθμίσεις",
            Lang.EN => "Settings",
            Lang.FR => "Paramètres",
            _ => "Einstellungen"
        };

        public static string SettingsAgentLabel => "Agent:";

        public static string SettingsKeyLabel => Current switch
        {
            Lang.EL => "Κλειδί API:",
            Lang.EN => "API key:",
            Lang.FR => "Clé API :",
            _ => "API-Schlüssel:"
        };

        public static string SetCentralAgentButton => Current switch
        {
            Lang.EL => "⭐ Ορισμός ως κεντρικός agent",
            Lang.EN => "⭐ Set as central agent",
            Lang.FR => "⭐ Définir comme agent central",
            _ => "⭐ Als zentralen Agenten festlegen"
        };

        public static string CustomInstructionsLabel => Current switch
        {
            Lang.EL => "Προσαρμοσμένες οδηγίες (πώς θέλεις να σου απαντά):",
            Lang.EN => "Custom instructions (how you'd like it to respond):",
            Lang.FR => "Instructions personnalisées (comment tu veux qu'il réponde) :",
            _ => "Benutzerdefinierte Anweisungen (wie er antworten soll):"
        };

        public static string McpServersLabel => Current switch
        {
            Lang.EL => "Απομακρυσμένοι MCP servers (μόνο Claude):",
            Lang.EN => "Remote MCP servers (Claude only):",
            Lang.FR => "Serveurs MCP distants (Claude uniquement) :",
            _ => "Externe MCP-Server (nur Claude):"
        };

        public static string McpServersHint => Current switch
        {
            Lang.EL => "Μία γραμμή ανά server: όνομα|https://url-του-server. Ο Claude θα μπορεί να τους καλεί απευθείας μέσα στη συζήτηση. Το GitHub είναι προσυμπληρωμένο σαν παράδειγμα αλλά χρειάζεται OAuth login στο GitHub που το Kombos δεν κάνει ακόμα αυτόματα - θα λάβεις σφάλμα εξουσιοδότησης μέχρι να προστεθεί.",
            Lang.EN => "One line per server: name|https://server-url. Claude will be able to call them directly within the chat. GitHub is pre-filled as an example but needs GitHub OAuth login that Kombos doesn't yet automate - you'll get an authorization error until that's added.",
            Lang.FR => "Une ligne par serveur : nom|https://url-du-serveur. Claude pourra les appeler directement dans la conversation. GitHub est pré-rempli à titre d'exemple mais nécessite une connexion OAuth GitHub que Kombos n'automatise pas encore - tu obtiendras une erreur d'autorisation jusqu'à ce que ce soit ajouté.",
            _ => "Eine Zeile pro Server: Name|https://server-url. Claude kann sie dann direkt im Chat aufrufen. GitHub ist als Beispiel vorausgefüllt, benötigt aber eine GitHub-OAuth-Anmeldung, die Kombos noch nicht automatisiert - bis das hinzugefügt ist, erhältst du einen Autorisierungsfehler."
        };

        public static string CustomInstructionsHint => Current switch
        {
            Lang.EL => "Προαιρετικό. Προστίθεται σε κάθε συνομιλία με τον κεντρικό agent — π.χ. \"απάντα πολύ σύντομα\" ή \"είμαι προγραμματιστής, μη μου εξηγείς τα βασικά\".",
            Lang.EN => "Optional. Added to every conversation with the central agent — e.g. \"keep replies very short\" or \"I'm a developer, skip the basics\".",
            Lang.FR => "Optionnel. Ajouté à chaque conversation avec l'agent central — ex. « réponds très brièvement » ou « je suis développeur, ne m'explique pas les bases ».",
            _ => "Optional. Wird jeder Unterhaltung mit dem zentralen Agenten hinzugefügt — z. B. „antworte sehr kurz“ oder „ich bin Entwickler, erspar mir die Grundlagen“."
        };

        public static string CentralAgentStatus(string agentName, bool hasIntegration) => (Current, hasIntegration) switch
        {
            (Lang.EL, true) => $"Κεντρικός agent: {agentName} — πραγματική σύνδεση chat ενεργή.",
            (Lang.EL, false) => $"Κεντρικός agent: {agentName} — δεν υπάρχει ακόμα πραγματική σύνδεση chat για αυτό το εργαλείο, θα βλέπεις εξήγηση στο chat.",
            (Lang.EN, true) => $"Central agent: {agentName} — real chat connection active.",
            (Lang.EN, false) => $"Central agent: {agentName} — no real chat connection exists yet for this tool, you'll see an explanation in the chat.",
            (Lang.FR, true) => $"Agent central : {agentName} — connexion de chat réelle active.",
            (Lang.FR, false) => $"Agent central : {agentName} — pas encore de connexion de chat réelle pour cet outil, tu verras une explication dans le chat.",
            (_, true) => $"Zentraler Agent: {agentName} — echte Chat-Verbindung aktiv.",
            (_, false) => $"Zentraler Agent: {agentName} — für dieses Tool gibt es noch keine echte Chat-Verbindung, du siehst eine Erklärung im Chat."
        };

        public static string NoIntegrationReply(string agentName) => Current switch
        {
            Lang.EL => $"Δεν υπάρχει ακόμα πραγματική σύνδεση chat για το {agentName} σε αυτή την εφαρμογή — δοκίμασε το κλειδί σου με το Claude, ή άνοιξε το {agentName} απευθείας από τον κατάλογο.",
            Lang.EN => $"There's no real chat connection for {agentName} in this app yet — try your key with Claude instead, or open {agentName} directly from the catalog.",
            Lang.FR => $"Il n'y a pas encore de connexion de chat réelle pour {agentName} dans cette application — essaie ta clé avec Claude à la place, ou ouvre {agentName} directement depuis le catalogue.",
            _ => $"Für {agentName} gibt es in dieser App noch keine echte Chat-Verbindung — probiere deinen Schlüssel stattdessen mit Claude, oder öffne {agentName} direkt aus dem Katalog."
        };

        public static string SettingsHintClaude => Current switch
        {
            Lang.EL => "Αυτό το κλειδί τροφοδοτεί το chat σε αυτή την εφαρμογή (κρατιέται μόνο στη μνήμη αυτής της συνεδρίας, στέλνεται μόνο στο api.anthropic.com).",
            Lang.EN => "This key powers the chat in this app (kept only in this session's memory, sent only to api.anthropic.com).",
            Lang.FR => "Cette clé alimente le chat de cette application (conservée uniquement en mémoire pour cette session, envoyée uniquement à api.anthropic.com).",
            _ => "Dieser Schlüssel treibt den Chat in dieser App an (nur im Speicher dieser Sitzung gehalten, nur an api.anthropic.com gesendet)."
        };

        public static string SettingsHintOther => Current switch
        {
            Lang.EL => "Αυτό το εργαλείο δεν έχει ενεργή σύνδεση chat σε αυτή την έκδοση — το κλειδί κρατιέται μόνο τοπικά για δική σου αναφορά (μόνο στη μνήμη αυτής της συνεδρίας).",
            Lang.EN => "This tool has no active chat integration in this version — the key is only kept locally for your own reference (this session's memory only).",
            Lang.FR => "Cet outil n'a pas d'intégration de chat active dans cette version — la clé est seulement conservée localement pour ta propre référence (mémoire de cette session uniquement).",
            _ => "Dieses Tool hat in dieser Version keine aktive Chat-Integration — der Schlüssel wird nur lokal zu deiner eigenen Referenz gespeichert (nur im Speicher dieser Sitzung)."
        };

        public static string Greeting(string agentName) => Current switch
        {
            Lang.EL => $"Γεια! Είμαι ο {agentName}, ο κεντρικός agent αυτής της εφαρμογής. Πρόσθεσε το κλειδί API σου από πάνω και πες μου τι θέλεις να φτιάξεις — αν χρειαστεί, φέρνω μαζί μου τους κατάλληλους agents.",
            Lang.EN => $"Hi! I'm {agentName}, the central agent of this app. Add your API key above and tell me what you'd like to build — I'll bring in the right agents along the way if needed.",
            Lang.FR => $"Salut ! Je suis {agentName}, l'agent central de cette application. Ajoute ta clé API ci-dessus et dis-moi ce que tu veux créer — j'appellerai les bons agents si besoin.",
            _ => $"Hallo! Ich bin {agentName}, der zentrale Agent dieser App. Füge oben deinen API-Schlüssel hinzu und sag mir, was du bauen möchtest — ich ziehe bei Bedarf die passenden Agenten hinzu."
        };

        public static string SendButton => Current switch
        {
            Lang.EL => "Αποστολή",
            Lang.EN => "Send",
            Lang.FR => "Envoyer",
            _ => "Senden"
        };

        public static string StopButton => Current switch
        {
            Lang.EL => "Διακοπή",
            Lang.EN => "Stop",
            Lang.FR => "Arrêter",
            _ => "Stopp"
        };

        public static string CopyButton => Current switch
        {
            Lang.EL => "📋 Αντιγραφή",
            Lang.EN => "📋 Copy",
            Lang.FR => "📋 Copier",
            _ => "📋 Kopieren"
        };

        public static string ToolSearchPlaceholder => Current switch
        {
            Lang.EL => "Αναζήτηση εργαλείου...",
            Lang.EN => "Search tools...",
            Lang.FR => "Rechercher un outil...",
            _ => "Tools durchsuchen..."
        };

        public static string OnboardingSkip => Current switch
        {
            Lang.EL => "Παράλειψη",
            Lang.EN => "Skip",
            Lang.FR => "Passer",
            _ => "Überspringen"
        };

        public static string OnboardingGetStarted => Current switch
        {
            Lang.EL => "🚀 Ξεκίνα",
            Lang.EN => "🚀 Get started",
            Lang.FR => "🚀 Commencer",
            _ => "🚀 Loslegen"
        };

        public static string ReplayOnboardingButton => Current switch
        {
            Lang.EL => "🔰 Προβολή οδηγού πρώτης χρήσης ξανά",
            Lang.EN => "🔰 Replay the getting-started guide",
            Lang.FR => "🔰 Revoir le guide de démarrage",
            _ => "🔰 Einführungstour erneut ansehen"
        };

        public static (string Icon, string Title, string Body)[] OnboardingPages => Current switch
        {
            Lang.EL => new[]
            {
                ("🧭", "Καλώς ήρθες στο Κόμβος",
                    "Το Κόμβος είναι ο κεντρικός σου σταθμός για 39+ εργαλεία AI, μαζί με δεκάδες τοπικές εντολές \"/κάτι\" που τρέχουν χωρίς κόστος tokens. Μίλα στον Claude σαν κεντρικό agent - θα σου φέρνει τους κατάλληλους agents ανάλογα με το task σου."),
                ("💬", "Συνομιλία & agents",
                    "Γράψε τι θέλεις να φτιάξεις στο πεδίο κάτω από τον τίτλο. Πάτησε Ctrl+K οποτεδήποτε για γρήγορη αναζήτηση σε εργαλεία/εντολές/ρυθμίσεις. Το ⚖ συγκρίνει πολλαπλούς agents ταυτόχρονα, το 🔗 στέλνει την απάντηση ενός agent σε δεύτερο για βελτίωση (Handoff)."),
                ("🗂", "Κατάλογος & Projects",
                    "Οι έγχρωμες καρτέλες φιλτράρουν τον κατάλογο ανά κατηγορία - καρφίτσωσε αγαπημένα με ⭐. Στο εικονίδιο 📁 βρίσκεις τα Projects: ομαδοποίησε συζητήσεις κάτω από ένα όνομα με το \"+ Νέο Project\", ίδια λογική με το Claude Desktop."),
                ("🔌", "Επεκτάσεις, MCP & Hive Memory",
                    "Ρυθμίσεις → Επεκτάσεις → \"Λήψη & Ενσωμάτωση\" εγκαθιστά επίσημα plugins με ένα κλικ (νέες εντολές \"/κάτι\"). Γράψε /remember <κάτι> στη συζήτηση και το θυμούνται ΟΛΟΙ οι agents (Hive Memory) - σύνδεσε και απομακρυσμένους MCP servers για τον Claude."),
                ("⚙", "Ρυθμίσεις, skins & συντομεύσεις",
                    "Το γρανάζι πάνω-αριστερά ανοίγει τις γενικές ρυθμίσεις, το \"C\" τις ρυθμίσεις του agent. Δοκίμασε διαφορετικά skins (Windows Classic, Nexus κ.ά.) από την Εμφάνιση. Ctrl+N νέα συζήτηση, Ctrl+K γρήγορη εντολή, Ctrl+, ρυθμίσεις, Ctrl+Alt+K φέρνει το Κόμβος μπροστά από οπουδήποτε στα Windows."),
                ("🚀", "Έτοιμος να ξεκινήσεις",
                    "Αυτό ήταν! Μπορείς να ξαναδείς αυτόν τον οδηγό όποτε θες από Ρυθμίσεις → Πληροφορίες → Βοήθεια.")
            },
            Lang.EN => new[]
            {
                ("🧭", "Welcome to Kombos",
                    "Kombos is your central hub for 39+ AI tools, plus dozens of local \"/something\" commands that run at zero token cost. Talk to Claude as the central agent - it brings in the right specialized agents for your task."),
                ("💬", "Chat & agents",
                    "Type what you want to build in the box under the title. Press Ctrl+K anytime for a quick search across tools/commands/settings. ⚖ compares multiple agents at once, 🔗 sends one agent's reply to a second for refinement (Handoff)."),
                ("🗂", "Catalog & Projects",
                    "The colored tabs filter the catalog by category - pin favorites with ⭐. The 📁 icon opens Projects: group conversations under a name with \"+ New Project\", same idea as Claude Desktop."),
                ("🔌", "Plugins, MCP & Hive Memory",
                    "Settings → Plugins → \"Download & Install\" adds official plugins with one click (new \"/something\" commands). Type /remember <something> in chat and EVERY agent remembers it (Hive Memory) - you can also connect remote MCP servers for Claude."),
                ("⚙", "Settings, skins & shortcuts",
                    "The gear top-left opens general settings, the \"C\" avatar opens the agent's own settings. Try different skins (Windows Classic, Nexus, and more) under Appearance. Ctrl+N new chat, Ctrl+K command palette, Ctrl+, settings, Ctrl+Alt+K brings Kombos to the front from anywhere in Windows."),
                ("🚀", "Ready to go",
                    "That's it! You can replay this guide anytime from Settings → Info → Help.")
            },
            Lang.FR => new[]
            {
                ("🧭", "Bienvenue sur Kombos",
                    "Kombos est ton point central pour 39+ outils IA, plus des dizaines de commandes locales \"/truc\" qui tournent sans aucun coût en tokens. Parle à Claude comme agent central - il fait venir les bons agents spécialisés selon ta tâche."),
                ("💬", "Chat & agents",
                    "Écris ce que tu veux créer dans le champ sous le titre. Appuie sur Ctrl+K à tout moment pour une recherche rapide dans les outils/commandes/paramètres. ⚖ compare plusieurs agents à la fois, 🔗 envoie la réponse d'un agent à un second pour l'améliorer (Handoff)."),
                ("🗂", "Catalogue & Projets",
                    "Les onglets colorés filtrent le catalogue par catégorie - épingle tes favoris avec ⭐. L'icône 📁 ouvre les Projets : regroupe des conversations sous un nom avec « + Nouveau projet », même principe que Claude Desktop."),
                ("🔌", "Extensions, MCP & mémoire collective",
                    "Paramètres → Extensions → « Télécharger et installer » ajoute des extensions officielles en un clic (nouvelles commandes \"/truc\"). Tape /remember <quelque chose> dans le chat et TOUS les agents s'en souviennent (Hive Memory) - tu peux aussi connecter des serveurs MCP distants pour Claude."),
                ("⚙", "Paramètres, skins & raccourcis",
                    "L'engrenage en haut à gauche ouvre les paramètres généraux, l'avatar « C » ouvre les paramètres de l'agent. Essaie différents skins (Windows Classic, Nexus, et plus) dans Apparence. Ctrl+N nouvelle conversation, Ctrl+K palette de commandes, Ctrl+, paramètres, Ctrl+Alt+K ramène Kombos au premier plan depuis n'importe où."),
                ("🚀", "Prêt à commencer",
                    "C'est tout ! Tu peux revoir ce guide à tout moment depuis Paramètres → Infos → Aide.")
            },
            _ => new[]
            {
                ("🧭", "Willkommen bei Kombos",
                    "Kombos ist deine zentrale Anlaufstelle für 39+ KI-Tools, plus Dutzende lokale „/etwas“-Befehle ohne jegliche Token-Kosten. Sprich mit Claude als zentralem Agenten - er holt die passenden Spezial-Agenten für deine Aufgabe dazu."),
                ("💬", "Chat & Agenten",
                    "Schreibe unter dem Titel, was du bauen möchtest. Drücke jederzeit Strg+K für eine schnelle Suche über Tools/Befehle/Einstellungen. ⚖ vergleicht mehrere Agenten gleichzeitig, 🔗 schickt die Antwort eines Agenten zur Verfeinerung an einen zweiten (Handoff)."),
                ("🗂", "Katalog & Projekte",
                    "Die farbigen Tabs filtern den Katalog nach Kategorie - heft Favoriten mit ⭐ an. Das 📁-Symbol öffnet Projekte: gruppiere Unterhaltungen unter einem Namen mit „+ Neues Projekt“, gleiches Prinzip wie bei Claude Desktop."),
                ("🔌", "Plugins, MCP & Kollektivgedächtnis",
                    "Einstellungen → Erweiterungen → „Herunterladen & Installieren“ fügt offizielle Plugins per Klick hinzu (neue „/etwas“-Befehle). Tippe /remember <etwas> im Chat und JEDER Agent merkt es sich (Hive Memory) - du kannst auch externe MCP-Server für Claude verbinden."),
                ("⚙", "Einstellungen, Skins & Shortcuts",
                    "Das Zahnrad oben links öffnet die allgemeinen Einstellungen, der „C“-Avatar die Einstellungen des Agenten. Probiere verschiedene Skins (Windows Classic, Nexus und mehr) unter Erscheinungsbild aus. Strg+N neuer Chat, Strg+K Befehlspalette, Strg+, Einstellungen, Strg+Alt+K holt Kombos von überall in Windows nach vorne."),
                ("🚀", "Bereit loszulegen",
                    "Das war's! Du kannst diese Tour jederzeit über Einstellungen → Info → Hilfe erneut ansehen.")
            }
        };

        public static string ScrollToBottomTooltip => Current switch
        {
            Lang.EL => "Μετάβαση στο τέλος",
            Lang.EN => "Jump to bottom",
            Lang.FR => "Aller en bas",
            _ => "Zum Ende springen"
        };

        public static string DeleteMessageButton => Current switch
        {
            Lang.EL => "🗑 Διαγραφή",
            Lang.EN => "🗑 Delete",
            Lang.FR => "🗑 Supprimer",
            _ => "🗑 Löschen"
        };

        public static string EditButton => Current switch
        {
            Lang.EL => "✏ Επεξεργασία",
            Lang.EN => "✏ Edit",
            Lang.FR => "✏ Modifier",
            _ => "✏ Bearbeiten"
        };

        public static string CopyAsMarkdown => Current switch
        {
            Lang.EL => "Αντιγραφή ως Markdown",
            Lang.EN => "Copy as Markdown",
            Lang.FR => "Copier en Markdown",
            _ => "Als Markdown kopieren"
        };

        public static string CopyAsPlainText => Current switch
        {
            Lang.EL => "Αντιγραφή ως απλό κείμενο",
            Lang.EN => "Copy as plain text",
            Lang.FR => "Copier en texte brut",
            _ => "Als reinen Text kopieren"
        };

        public static string QuoteInNewMessage => Current switch
        {
            Lang.EL => "Παράθεση σε νέο μήνυμα",
            Lang.EN => "Quote in new message",
            Lang.FR => "Citer dans un nouveau message",
            _ => "In neuer Nachricht zitieren"
        };

        public static string SpeakButton => Current switch
        {
            Lang.EL => "🔊 Ανάγνωση",
            Lang.EN => "🔊 Read aloud",
            Lang.FR => "🔊 Lire à voix haute",
            _ => "🔊 Vorlesen"
        };

        public static string SpeakButtonTooltip => Current switch
        {
            Lang.EL => "Ανάγνωση της απάντησης δυνατά (τοπικά, δωρεάν - System.Speech)",
            Lang.EN => "Read this reply aloud (local, free - System.Speech)",
            Lang.FR => "Lire cette réponse à voix haute (local, gratuit - System.Speech)",
            _ => "Diese Antwort vorlesen (lokal, kostenlos - System.Speech)"
        };

        /// <summary>Τοπικό analytics dashboard (v4.5.0) - βλ. AnalyticsWindow.</summary>
        public static string AnalyticsTitle => Current switch
        {
            Lang.EL => "Αναλυτικά χρήσης",
            Lang.EN => "Usage analytics",
            Lang.FR => "Statistiques d'utilisation",
            _ => "Nutzungsstatistik"
        };

        public static string AnalyticsSessionsLabel => Current switch
        {
            Lang.EL => "Tokens ανά συνεδρία (τελευταίες 20)",
            Lang.EN => "Tokens per session (last 20)",
            Lang.FR => "Tokens par session (20 dernières)",
            _ => "Tokens pro Sitzung (letzte 20)"
        };

        public static string AnalyticsSessionsHint => Current switch
        {
            Lang.EL => "Μία \"συνεδρία\" = από τη μία πραγματική έξοδο μέχρι την επόμενη (όχι ημερολογιακή ημέρα). 100% τοπικό, καμία τηλεμετρία.",
            Lang.EN => "One \"session\" = from one real exit to the next (not calendar day). 100% local, no telemetry.",
            Lang.FR => "Une « session » = d'une sortie réelle à la suivante (pas un jour calendaire). 100% local, aucune télémétrie.",
            _ => "Eine \"Sitzung\" = von einem echten Beenden bis zum nächsten (kein Kalendertag). 100% lokal, keine Telemetrie."
        };

        public static string AnalyticsCurrentSessionLabel => Current switch
        {
            Lang.EL => "Τρέχουσα συνεδρία - ανά εργαλείο",
            Lang.EN => "Current session - per tool",
            Lang.FR => "Session actuelle - par outil",
            _ => "Aktuelle Sitzung - pro Tool"
        };

        public static string AnalyticsNoHistory => Current switch
        {
            Lang.EL => "Δεν υπάρχει ακόμα ιστορικό - θα εμφανιστεί μετά το πρώτο πραγματικό κλείσιμο της εφαρμογής με χρήση κάποιου εργαλείου.",
            Lang.EN => "No history yet - it will appear after the first real app exit where a tool was used.",
            Lang.FR => "Aucun historique pour l'instant - il apparaîtra après la première fermeture réelle avec un outil utilisé.",
            _ => "Noch keine Historie - sie erscheint nach dem ersten echten Beenden mit genutztem Tool."
        };

        public static string AnalyticsNoUsageYet => Current switch
        {
            Lang.EL => "Καμία χρήση ακόμα σε αυτή τη συνεδρία.",
            Lang.EN => "No usage yet this session.",
            Lang.FR => "Aucune utilisation pour l'instant dans cette session.",
            _ => "Noch keine Nutzung in dieser Sitzung."
        };

        /// <summary>Plugin SDK v2 (v4.5.0) - βλ. Services/PluginScaffolder.cs.</summary>
        public static string PluginSdkHeader => Current switch
        {
            Lang.EL => "Plugin SDK v2 - Νέο Plugin",
            Lang.EN => "Plugin SDK v2 - New Plugin",
            Lang.FR => "Plugin SDK v2 - Nouveau plugin",
            _ => "Plugin SDK v2 - Neues Plugin"
        };

        public static string PluginSdkHint => Current switch
        {
            Lang.EL => "Δώσε ένα όνομα και δημιούργησε ένα έτοιμο-προς-build .csproj + starter κώδικα - χτίσε το με \"dotnet build\" και μετά εγκατέστησέ το από το Marketplace παρακάτω ή αντίγραψε το .dll χειροκίνητα.",
            Lang.EN => "Give it a name and generate a ready-to-build .csproj + starter code - build it with \"dotnet build\", then install it from the Marketplace below or copy the .dll manually.",
            Lang.FR => "Donne un nom et génère un .csproj prêt à compiler + du code de départ - compile-le avec « dotnet build », puis installe-le via le Marketplace ci-dessous ou copie le .dll manuellement.",
            _ => "Gib einen Namen ein und erzeuge eine build-fertige .csproj + Startcode - baue sie mit \"dotnet build\" und installiere sie dann über den Marketplace unten oder kopiere die .dll manuell."
        };

        public static string ScaffoldPluginButton => Current switch
        {
            Lang.EL => "🛠 Δημιουργία project plugin",
            Lang.EN => "🛠 Generate plugin project",
            Lang.FR => "🛠 Générer le projet du plugin",
            _ => "🛠 Plugin-Projekt erzeugen"
        };

        public static string ScaffoldNeedsName => Current switch
        {
            Lang.EL => "Γράψε πρώτα ένα όνομα για το plugin.",
            Lang.EN => "Enter a name for the plugin first.",
            Lang.FR => "Indique d'abord un nom pour le plugin.",
            _ => "Gib zuerst einen Namen für das Plugin ein."
        };

        public static string ScaffoldSuccess(string path) => Current switch
        {
            Lang.EL => $"✅ Δημιουργήθηκε: {path}",
            Lang.EN => $"✅ Created: {path}",
            Lang.FR => $"✅ Créé : {path}",
            _ => $"✅ Erstellt: {path}"
        };

        /// <summary>Nexus Marketplace (v4.5.0) - βλ. Services/PluginMarketplace.cs.</summary>
        public static string MarketplaceHeader => Current switch
        {
            Lang.EL => "Nexus Marketplace",
            Lang.EN => "Nexus Marketplace",
            Lang.FR => "Nexus Marketplace",
            _ => "Nexus Marketplace"
        };

        public static string MarketplaceHint => Current switch
        {
            Lang.EL => "Εγκατάσταση plugin από δικό σου φάκελο (ήδη-χτισμένα .dll) ή από Git URL (π.χ. https://github.com/owner/repo) - ίδιο μοντέλο εμπιστοσύνης με ένα χειροκίνητο αντίγραφο DLL, απλά πιο βολικό. Χρειάζεται επανεκκίνηση για να ενεργοποιηθεί.",
            Lang.EN => "Install a plugin from your own folder (already-built .dll files) or from a Git URL (e.g. https://github.com/owner/repo) - same trust model as a manual DLL copy, just more convenient. Needs a restart to activate.",
            Lang.FR => "Installe un plugin depuis ton propre dossier (fichiers .dll déjà compilés) ou depuis une URL Git (ex. https://github.com/owner/repo) - même modèle de confiance qu'une copie manuelle de DLL, juste plus pratique. Nécessite un redémarrage pour s'activer.",
            _ => "Installiere ein Plugin aus einem eigenen Ordner (bereits gebaute .dll-Dateien) oder von einer Git-URL (z. B. https://github.com/owner/repo) - gleiches Vertrauensmodell wie ein manuelles DLL-Kopieren, nur bequemer. Braucht einen Neustart zur Aktivierung."
        };

        public static string InstallFromFolderButton => Current switch
        {
            Lang.EL => "📁 Εγκατάσταση από φάκελο...",
            Lang.EN => "📁 Install from folder...",
            Lang.FR => "📁 Installer depuis un dossier...",
            _ => "📁 Aus Ordner installieren..."
        };

        public static string InstallFromGitButton => Current switch
        {
            Lang.EL => "🔗 Εγκατάσταση από Git URL",
            Lang.EN => "🔗 Install from Git URL",
            Lang.FR => "🔗 Installer depuis une URL Git",
            _ => "🔗 Von Git-URL installieren"
        };

        public static string MarketplaceNoDlls => Current switch
        {
            Lang.EL => "Δεν βρέθηκαν .dll αρχεία σε αυτόν τον φάκελο.",
            Lang.EN => "No .dll files found in that folder.",
            Lang.FR => "Aucun fichier .dll trouvé dans ce dossier.",
            _ => "Keine .dll-Dateien in diesem Ordner gefunden."
        };

        public static string MarketplaceDownloading => Current switch
        {
            Lang.EL => "Λήψη...",
            Lang.EN => "Downloading...",
            Lang.FR => "Téléchargement...",
            _ => "Wird heruntergeladen..."
        };

        public static string MarketplaceInstalled(string name) => Current switch
        {
            Lang.EL => $"✅ Εγκαταστάθηκε: {name} - χρειάζεται επανεκκίνηση για να ενεργοποιηθεί.",
            Lang.EN => $"✅ Installed: {name} - needs a restart to activate.",
            Lang.FR => $"✅ Installé : {name} - un redémarrage est nécessaire pour l'activer.",
            _ => $"✅ Installiert: {name} - braucht einen Neustart zur Aktivierung."
        };

        /// <summary>"Κόμβος Gateway" (v4.6.0, OmniRoute-style τοπική πύλη) - βλ. Services/GatewayServer.cs.</summary>
        /// <summary>Αυτόματη συνέχιση σε άλλο agent όταν το Claude πλησιάζει στο όριό του (v4.6.0) - βλ.
        /// MainWindow.TryAutoContinueOnLimitAsync.</summary>
        public static string AutoContinueLabel => Current switch
        {
            Lang.EL => "Αυτόματη συνέχιση σε άλλο agent όταν το Claude πλησιάζει στο όριό του",
            Lang.EN => "Automatically continue with another agent when Claude nears its limit",
            Lang.FR => "Continuer automatiquement avec un autre agent quand Claude approche de sa limite",
            _ => "Automatisch mit einem anderen Agent fortfahren, wenn Claude an sein Limit stößt"
        };

        public static string AutoContinueHint => Current switch
        {
            Lang.EL => "Όταν φτάσει κοντά στο όριο tokens της συνεδρίας ή πάρει 429 (rate limit) από το Anthropic API, η ίδια συζήτηση συνεχίζει αυτόματα με τον επόμενο agent που έχεις ήδη ρυθμίσει με κλειδί - η φούσκα δείχνει ποιος πραγματικά απάντησε.",
            Lang.EN => "When the session's token budget gets close, or the Anthropic API returns 429 (rate limit), the same conversation automatically continues with the next agent you've already configured with a key - the bubble shows who actually answered.",
            Lang.FR => "Quand le budget de tokens de la session approche, ou que l'API Anthropic renvoie 429 (limite de débit), la même conversation continue automatiquement avec le prochain agent déjà configuré avec une clé - la bulle indique qui a réellement répondu.",
            _ => "Wenn das Token-Budget der Sitzung sich dem Limit nähert oder die Anthropic-API 429 (Rate Limit) zurückgibt, wird dieselbe Unterhaltung automatisch mit dem nächsten bereits konfigurierten Agent fortgesetzt - die Sprechblase zeigt, wer wirklich geantwortet hat."
        };

        public static string AutoContinuedAgentLabel(string toolName) => Current switch
        {
            Lang.EL => $"{toolName} (αυτόματη συνέχιση)",
            Lang.EN => $"{toolName} (auto-continued)",
            Lang.FR => $"{toolName} (continué automatiquement)",
            _ => $"{toolName} (automatisch fortgesetzt)"
        };

        /// <summary>v4.6.0 - pop-up ΤΗΝ ΠΡΩΤΗ ΦΟΡΑ που το Claude πλησιάζει στο όριό του· μετά το Κόμβος
        /// θυμάται την επιλογή και δεν ξαναρωτάει (βλ. MainWindow.ResolveFallbackAgent).</summary>
        public static string ClaudeLimitNearAsk(string toolName) => Current switch
        {
            Lang.EL => $"Το Claude πλησιάζει στο όριο tokens αυτής της συνεδρίας.\n\nΘέλεις να συνεχίσει η συζήτηση αυτόματα με το {toolName} (έχεις ήδη αποθηκευμένο κλειδί);\n\nΗ επιλογή σου θα θυμάται από εδώ και πέρα - δεν θα ξαναρωτηθείς.",
            Lang.EN => $"Claude is nearing this session's token limit.\n\nDo you want the conversation to continue automatically with {toolName} (you already have a saved key for it)?\n\nYour choice will be remembered from now on - you won't be asked again.",
            Lang.FR => $"Claude approche de la limite de tokens de cette session.\n\nVeux-tu que la conversation continue automatiquement avec {toolName} (tu as déjà une clé enregistrée) ?\n\nTon choix sera mémorisé désormais - on ne te redemandera plus.",
            _ => $"Claude nähert sich dem Token-Limit dieser Sitzung.\n\nSoll die Unterhaltung automatisch mit {toolName} fortgesetzt werden (du hast dafür bereits einen gespeicherten Schlüssel)?\n\nDeine Wahl wird ab jetzt gemerkt - du wirst nicht erneut gefragt."
        };

        public static string ClaudeLimitReachedAsk(string toolName) => Current switch
        {
            Lang.EL => $"Το Claude έφτασε σε όριο (rate limit) αυτή τη στιγμή.\n\nΘέλεις να συνεχίσει η συζήτηση αυτόματα με το {toolName} (έχεις ήδη αποθηκευμένο κλειδί);\n\nΗ επιλογή σου θα θυμάται από εδώ και πέρα - δεν θα ξαναρωτηθείς.",
            Lang.EN => $"Claude just hit a rate limit.\n\nDo you want the conversation to continue automatically with {toolName} (you already have a saved key for it)?\n\nYour choice will be remembered from now on - you won't be asked again.",
            Lang.FR => $"Claude vient d'atteindre une limite de débit.\n\nVeux-tu que la conversation continue automatiquement avec {toolName} (tu as déjà une clé enregistrée) ?\n\nTon choix sera mémorisé désormais - on ne te redemandera plus.",
            _ => $"Claude hat gerade ein Rate Limit erreicht.\n\nSoll die Unterhaltung automatisch mit {toolName} fortgesetzt werden (du hast dafür bereits einen gespeicherten Schlüssel)?\n\nDeine Wahl wird ab jetzt gemerkt - du wirst nicht erneut gefragt."
        };

        public static string RememberedFallbackText(string toolName) => Current switch
        {
            Lang.EL => $"✓ Θυμάται: αυτόματη συνέχιση με {toolName}",
            Lang.EN => $"✓ Remembered: auto-continue with {toolName}",
            Lang.FR => $"✓ Mémorisé : continuation automatique avec {toolName}",
            _ => $"✓ Gemerkt: automatische Fortsetzung mit {toolName}"
        };

        public static string NoRememberedFallback => Current switch
        {
            Lang.EL => "Δεν έχεις επιλέξει ακόμα - θα ρωτηθείς την πρώτη φορά που θα χρειαστεί.",
            Lang.EN => "You haven't chosen yet - you'll be asked the first time it's needed.",
            Lang.FR => "Tu n'as pas encore choisi - on te demandera la première fois que ce sera nécessaire.",
            _ => "Du hast noch nichts ausgewählt - du wirst beim ersten Bedarf gefragt."
        };

        public static string ResetFallbackButton => Current switch
        {
            Lang.EL => "↺ Επαναφορά (ρώτα ξανά)",
            Lang.EN => "↺ Reset (ask again)",
            Lang.FR => "↺ Réinitialiser (redemander)",
            _ => "↺ Zurücksetzen (erneut fragen)"
        };

        public static string GatewayHeader => Current switch
        {
            Lang.EL => "Κόμβος Gateway",
            Lang.EN => "Kombos Gateway",
            Lang.FR => "Passerelle Kombos",
            _ => "Kombos-Gateway"
        };

        public static string GatewayHint => Current switch
        {
            Lang.EL => "Τοπική πύλη (σαν OmniRoute) - άλλα εργαλεία (VS Code, Cursor, Claude Code κ.ά.) μπορούν να στέλνουν εδώ αντί απευθείας σε κάποιο provider, και το Κόμβος δρομολογεί αυτόματα σε όποιο δικό σου agent έχει αποθηκευμένο κλειδί, με αυτόματο fallback αν ο πρώτος αποτύχει. Ακούει ΜΟΝΟ στον υπολογιστή σου (127.0.0.1) - ποτέ στο δίκτυο.",
            Lang.EN => "A local gateway (OmniRoute-style) - other tools (VS Code, Cursor, Claude Code, etc.) can send requests here instead of directly to a provider, and Kombos automatically routes to whichever of your configured agents has a saved key, with automatic fallback if the first one fails. Listens ONLY on this computer (127.0.0.1) - never on the network.",
            Lang.FR => "Une passerelle locale (façon OmniRoute) - d'autres outils (VS Code, Cursor, Claude Code, etc.) peuvent envoyer leurs requêtes ici au lieu d'un fournisseur direct, et Kombos route automatiquement vers l'un de tes agents configurés ayant une clé enregistrée, avec repli automatique si le premier échoue. N'écoute QUE sur cet ordinateur (127.0.0.1) - jamais sur le réseau.",
            _ => "Ein lokales Gateway (im Stil von OmniRoute) - andere Tools (VS Code, Cursor, Claude Code usw.) können Anfragen hierher statt direkt an einen Anbieter senden, und Kombos leitet automatisch an einen deiner konfigurierten Agents mit gespeichertem Schlüssel weiter, mit automatischem Fallback, falls der erste fehlschlägt. Lauscht NUR auf diesem Computer (127.0.0.1) - nie im Netzwerk."
        };

        public static string GatewayEnabledLabel => Current switch
        {
            Lang.EL => "Ενεργοποίηση τοπικής πύλης",
            Lang.EN => "Enable local gateway",
            Lang.FR => "Activer la passerelle locale",
            _ => "Lokales Gateway aktivieren"
        };

        public static string GatewayPortLabel => Current switch
        {
            Lang.EL => "Θύρα:",
            Lang.EN => "Port:",
            Lang.FR => "Port :",
            _ => "Port:"
        };

        public static string GatewayLogLabel => Current switch
        {
            Lang.EL => "Τελευταία αιτήματα",
            Lang.EN => "Recent requests",
            Lang.FR => "Requêtes récentes",
            _ => "Letzte Anfragen"
        };

        public static string GatewayRunning(int port) => Current switch
        {
            Lang.EL => $"🟢 Ενεργό: http://127.0.0.1:{port}/v1/chat/completions",
            Lang.EN => $"🟢 Running: http://127.0.0.1:{port}/v1/chat/completions",
            Lang.FR => $"🟢 Actif : http://127.0.0.1:{port}/v1/chat/completions",
            _ => $"🟢 Aktiv: http://127.0.0.1:{port}/v1/chat/completions"
        };

        public static string GatewayStopped => Current switch
        {
            Lang.EL => "⚪ Ανενεργό",
            Lang.EN => "⚪ Off",
            Lang.FR => "⚪ Désactivé",
            _ => "⚪ Aus"
        };

        public static string GatewayStartFailed(string error) => Current switch
        {
            Lang.EL => $"⚠ Απέτυχε η εκκίνηση: {error}",
            Lang.EN => $"⚠ Failed to start: {error}",
            Lang.FR => $"⚠ Échec du démarrage : {error}",
            _ => $"⚠ Start fehlgeschlagen: {error}"
        };

        public static string GatewayNoRequestsYet => Current switch
        {
            Lang.EL => "Κανένα αίτημα ακόμα.",
            Lang.EN => "No requests yet.",
            Lang.FR => "Aucune requête pour l'instant.",
            _ => "Noch keine Anfragen."
        };

        public static string UpdatesHeader => Current switch
        {
            Lang.EL => "Ενημερώσεις",
            Lang.EN => "Updates",
            Lang.FR => "Mises à jour",
            _ => "Updates"
        };

        public static string UpdatesHint => Current switch
        {
            Lang.EL => "Προαιρετικό: γράψε \"owner/repo\" ενός GitHub repo με δημόσια releases για να μπορείς να ελέγχεις χειροκίνητα για νέα έκδοση. Ποτέ αυτόματος έλεγχος στο παρασκήνιο, ποτέ σιωπηλή λήψη/εγκατάσταση - μόνο ειδοποίηση + link.",
            Lang.EN => "Optional: enter a GitHub repo's \"owner/repo\" with public releases so you can manually check for a new version. Never checks automatically in the background, never silently downloads/installs - notification + link only.",
            Lang.FR => "Optionnel : indique le \"owner/repo\" d'un dépôt GitHub avec des releases publiques pour vérifier manuellement une nouvelle version. Jamais de vérification automatique en arrière-plan, jamais de téléchargement/installation silencieux - juste une notification + un lien.",
            _ => "Optional: Gib das \"owner/repo\" eines GitHub-Repos mit öffentlichen Releases ein, um manuell auf eine neue Version zu prüfen. Nie automatische Prüfung im Hintergrund, nie stilles Herunterladen/Installieren - nur Benachrichtigung + Link."
        };

        public static string CheckUpdateButton => Current switch
        {
            Lang.EL => "🔄 Έλεγχος για ενημερώσεις",
            Lang.EN => "🔄 Check for updates",
            Lang.FR => "🔄 Vérifier les mises à jour",
            _ => "🔄 Nach Updates suchen"
        };

        public static string CheckUpdateChecking => Current switch
        {
            Lang.EL => "Έλεγχος...",
            Lang.EN => "Checking...",
            Lang.FR => "Vérification...",
            _ => "Wird geprüft..."
        };

        public static string CheckUpdateNoRepo => Current switch
        {
            Lang.EL => "Δεν έχει οριστεί repo ακόμα - γράψε \"owner/repo\" παραπάνω.",
            Lang.EN => "No repo set yet - enter \"owner/repo\" above.",
            Lang.FR => "Aucun dépôt défini - indique \"owner/repo\" ci-dessus.",
            _ => "Noch kein Repo festgelegt - gib oben \"owner/repo\" ein."
        };

        public static string CheckUpdateError => Current switch
        {
            Lang.EL => "⚠ Δεν ήταν δυνατός ο έλεγχος (δίκτυο ή λάθος repo).",
            Lang.EN => "⚠ Couldn't check (network issue or wrong repo).",
            Lang.FR => "⚠ Vérification impossible (problème réseau ou dépôt incorrect).",
            _ => "⚠ Prüfung nicht möglich (Netzwerkproblem oder falsches Repo)."
        };

        public static string CheckUpdateAvailable(string version) => Current switch
        {
            Lang.EL => $"🆕 Νέα έκδοση διαθέσιμη: v{version} (πάτησε εδώ για τη σελίδα λήψης)",
            Lang.EN => $"🆕 New version available: v{version} (click here for the download page)",
            Lang.FR => $"🆕 Nouvelle version disponible : v{version} (clique ici pour la page de téléchargement)",
            _ => $"🆕 Neue Version verfügbar: v{version} (hier klicken für die Download-Seite)"
        };

        public static string CheckUpdateLatest => Current switch
        {
            Lang.EL => "✓ Έχεις την τελευταία έκδοση.",
            Lang.EN => "✓ You have the latest version.",
            Lang.FR => "✓ Tu as la dernière version.",
            _ => "✓ Du hast die neueste Version."
        };

        /// <summary>Kombos Anywhere (v4.5.0) - global-hotkey mini quick-capture bar, βλ. AnywhereBarWindow.</summary>
        public static string AnywhereHint => Current switch
        {
            Lang.EL => "Enter για αποστολή · Esc για κλείσιμο · Ctrl+Alt+Space ανοίγει από οπουδήποτε",
            Lang.EN => "Enter to send · Esc to close · Ctrl+Alt+Space opens this from anywhere",
            Lang.FR => "Entrée pour envoyer · Échap pour fermer · Ctrl+Alt+Espace ouvre ceci de n'importe où",
            _ => "Enter zum Senden · Esc zum Schließen · Strg+Alt+Leertaste öffnet dies von überall"
        };

        public static string AnywhereThinking => Current switch
        {
            Lang.EL => "...",
            Lang.EN => "...",
            Lang.FR => "...",
            _ => "..."
        };

        public static string AnywhereAddToChat => Current switch
        {
            Lang.EL => "➕ Προσθήκη στη συζήτηση",
            Lang.EN => "➕ Add to conversation",
            Lang.FR => "➕ Ajouter à la conversation",
            _ => "➕ Zur Unterhaltung hinzufügen"
        };

        /// <summary>"Artifact live preview" (v4.5.0) - βλ. ArtifactPreviewWindow.</summary>
        public static string ArtifactPreviewButton => Current switch
        {
            Lang.EL => "▶ Προεπισκόπηση",
            Lang.EN => "▶ Preview",
            Lang.FR => "▶ Aperçu",
            _ => "▶ Vorschau"
        };

        public static string ArtifactPreviewTitle => Current switch
        {
            Lang.EL => "Προεπισκόπηση Artifact",
            Lang.EN => "Artifact Preview",
            Lang.FR => "Aperçu de l'artéfact",
            _ => "Artifact-Vorschau"
        };

        public static string PresetsLabel => Current switch
        {
            Lang.EL => "Presets (agent + οδηγίες, ένα κλικ):",
            Lang.EN => "Presets (agent + instructions, one click):",
            Lang.FR => "Presets (agent + instructions, un clic) :",
            _ => "Presets (Agent + Anweisungen, ein Klick):"
        };

        public static string PresetsHint => Current switch
        {
            Lang.EL => "Ένα preset θυμάται ποιος agent + ποιες οδηγίες χρησιμοποιούσες, ώστε να μην τα ξαναγράφεις κάθε φορά.",
            Lang.EN => "A preset remembers which agent + instructions you were using, so you don't retype them every time.",
            Lang.FR => "Un preset retient l'agent + les instructions utilisés, pour ne pas les retaper à chaque fois.",
            _ => "Ein Preset merkt sich, welchen Agent + welche Anweisungen du verwendet hast, damit du sie nicht jedes Mal neu eingibst."
        };

        public static string SavePresetButtonText => Current switch
        {
            Lang.EL => "💾 Αποθήκευση τρέχοντος ως νέο preset",
            Lang.EN => "💾 Save current as new preset",
            Lang.FR => "💾 Enregistrer l'actuel comme nouveau preset",
            _ => "💾 Aktuelles als neues Preset speichern"
        };

        public static string MicUnavailable => Current switch
        {
            Lang.EL => "Η φωνητική εισαγωγή δεν είναι διαθέσιμη σε αυτόν τον υπολογιστή (χρειάζεται μικρόφωνο + Windows Speech Recognition).",
            Lang.EN => "Voice input isn't available on this computer (needs a microphone + Windows Speech Recognition).",
            Lang.FR => "La saisie vocale n'est pas disponible sur cet ordinateur (nécessite un micro + Windows Speech Recognition).",
            _ => "Spracheingabe ist auf diesem Computer nicht verfügbar (benötigt ein Mikrofon + Windows Sprach­erkennung)."
        };

        public static string SpeakUnavailable => Current switch
        {
            Lang.EL => "⚠ Μη διαθέσιμο",
            Lang.EN => "⚠ Unavailable",
            Lang.FR => "⚠ Indisponible",
            _ => "⚠ Nicht verfügbar"
        };

        public static string CopiedLabel => Current switch
        {
            Lang.EL => "✓ Αντιγράφηκε",
            Lang.EN => "✓ Copied",
            Lang.FR => "✓ Copié",
            _ => "✓ Kopiert"
        };

        public static string TrayOpen => Current switch
        {
            Lang.EL => "Άνοιγμα Κόμβου",
            Lang.EN => "Open Kombos",
            Lang.FR => "Ouvrir Kombos",
            _ => "Kombos öffnen"
        };

        public static string TrayNewChat => Current switch
        {
            Lang.EL => "Νέα συζήτηση",
            Lang.EN => "New chat",
            Lang.FR => "Nouvelle conversation",
            _ => "Neuer Chat"
        };

        public static string TrayAccounts => Current switch
        {
            Lang.EL => "Λογαριασμοί (login/logout)",
            Lang.EN => "Accounts (login/logout)",
            Lang.FR => "Comptes (connexion/déconnexion)",
            _ => "Konten (An-/Abmelden)"
        };

        public static string TrayExit => Current switch
        {
            Lang.EL => "Έξοδος",
            Lang.EN => "Exit",
            Lang.FR => "Quitter",
            _ => "Beenden"
        };

        public static string HistoryButtonLabel => Current switch
        {
            Lang.EL => "🕘 Ιστορικό",
            Lang.EN => "🕘 History",
            Lang.FR => "🕘 Historique",
            _ => "🕘 Verlauf"
        };

        public static string HistoryTitle => Current switch
        {
            Lang.EL => "Αποθηκευμένες συζητήσεις",
            Lang.EN => "Saved conversations",
            Lang.FR => "Conversations enregistrées",
            _ => "Gespeicherte Chats"
        };

        public static string HistoryEmpty => Current switch
        {
            Lang.EL => "Δεν υπάρχουν ακόμα αποθηκευμένες συζητήσεις.",
            Lang.EN => "No saved conversations yet.",
            Lang.FR => "Aucune conversation enregistrée pour l'instant.",
            _ => "Noch keine gespeicherten Chats."
        };

        public static string HistorySearchPlaceholder => Current switch
        {
            Lang.EL => "Αναζήτηση στις συζητήσεις...",
            Lang.EN => "Search conversations...",
            Lang.FR => "Rechercher des conversations...",
            _ => "Chats durchsuchen..."
        };

        public static string HistoryNoResults => Current switch
        {
            Lang.EL => "Καμία συζήτηση δεν ταιριάζει με την αναζήτηση.",
            Lang.EN => "No conversations match your search.",
            Lang.FR => "Aucune conversation ne correspond à la recherche.",
            _ => "Keine Chats entsprechen der Suche."
        };

        /// <summary>"Multi-window" (v4.5.0) - βλ. ConversationViewerWindow.</summary>
        public static string DetachButton => Current switch
        {
            Lang.EL => "Απόσπαση σε νέο παράθυρο",
            Lang.EN => "Detach into new window",
            Lang.FR => "Détacher dans une nouvelle fenêtre",
            _ => "In neues Fenster lösen"
        };

        public static string CopyAllAsMarkdown => Current switch
        {
            Lang.EL => "Αντιγραφή όλων ως Markdown",
            Lang.EN => "Copy all as Markdown",
            Lang.FR => "Copier tout en Markdown",
            _ => "Alles als Markdown kopieren"
        };

        public static string DuplicateButton => Current switch
        {
            Lang.EL => "Δημιουργία αντιγράφου",
            Lang.EN => "Duplicate",
            Lang.FR => "Dupliquer",
            _ => "Duplizieren"
        };

        public static string DuplicateTitle(string original) => Current switch
        {
            Lang.EL => $"{original} (αντίγραφο)",
            Lang.EN => $"{original} (copy)",
            Lang.FR => $"{original} (copie)",
            _ => $"{original} (Kopie)"
        };

        /// <summary>"Fork" (LibreChat-style ιδέα, v4.1.0): κλάδεμα νέας συζήτησης από ένα συγκεκριμένο μήνυμα -
        /// βλ. MainWindow.ForkConversationFrom.</summary>
        public static string ForkButton => Current switch
        {
            Lang.EL => "🔀 Κλάδεμα",
            Lang.EN => "🔀 Fork",
            Lang.FR => "🔀 Dériver",
            _ => "🔀 Abzweigen"
        };

        public static string ForkButtonTooltip => Current switch
        {
            Lang.EL => "Δημιουργεί νέα συζήτηση με το ιστορικό μέχρι εδώ, για να δοκιμάσεις άλλη κατεύθυνση χωρίς να αλλάξεις την αρχική.",
            Lang.EN => "Creates a new conversation with the history up to here, so you can try a different direction without changing the original.",
            Lang.FR => "Crée une nouvelle conversation avec l'historique jusqu'ici, pour essayer une autre direction sans modifier l'originale.",
            _ => "Erstellt eine neue Unterhaltung mit dem Verlauf bis hierher, um eine andere Richtung auszuprobieren, ohne das Original zu ändern."
        };

        public static string PromptLibraryTitle => Current switch
        {
            Lang.EL => "📚 Βιβλιοθήκη Prompts",
            Lang.EN => "📚 Prompt Library",
            Lang.FR => "📚 Bibliothèque de prompts",
            _ => "📚 Prompt-Bibliothek"
        };

        public static string ForkedTitle(string original) => Current switch
        {
            Lang.EL => $"{(string.IsNullOrWhiteSpace(original) ? "Συζήτηση" : original)} (κλαδί)",
            Lang.EN => $"{(string.IsNullOrWhiteSpace(original) ? "Conversation" : original)} (fork)",
            Lang.FR => $"{(string.IsNullOrWhiteSpace(original) ? "Conversation" : original)} (dérivée)",
            _ => $"{(string.IsNullOrWhiteSpace(original) ? "Unterhaltung" : original)} (Abzweigung)"
        };

        public static string RenameButton => Current switch
        {
            Lang.EL => "Μετονομασία",
            Lang.EN => "Rename",
            Lang.FR => "Renommer",
            _ => "Umbenennen"
        };

        public static string RegenerateButton => Current switch
        {
            Lang.EL => "↻ Επανάληψη",
            Lang.EN => "↻ Regenerate",
            Lang.FR => "↻ Régénérer",
            _ => "↻ Neu generieren"
        };

        public static string SourcesLabel => Current switch
        {
            Lang.EL => "Πηγές από αναζήτηση ιστού:",
            Lang.EN => "Sources from web search:",
            Lang.FR => "Sources de la recherche web :",
            _ => "Quellen aus der Websuche:"
        };

        public static string NewChatButton => Current switch
        {
            Lang.EL => "↻ Νέα συζήτηση",
            Lang.EN => "↻ New chat",
            Lang.FR => "↻ Nouvelle conversation",
            _ => "↻ Neuer Chat"
        };

        public static string Ready => Current switch
        {
            Lang.EL => "Έτοιμο.",
            Lang.EN => "Ready.",
            Lang.FR => "Prêt.",
            _ => "Bereit."
        };

        public static string Thinking => Current switch
        {
            Lang.EL => "Ο Claude σκέφτεται…",
            Lang.EN => "Claude is thinking…",
            Lang.FR => "Claude réfléchit…",
            _ => "Claude denkt nach…"
        };

        public static string ThinkingBubble => Current switch
        {
            Lang.EL => "Σκέφτεται…",
            Lang.EN => "Thinking…",
            Lang.FR => "Réflexion…",
            _ => "Denkt nach…"
        };

        public static string AgentsInTask => Current switch
        {
            Lang.EL => "Agents σε αυτό το task:",
            Lang.EN => "Agents for this task:",
            Lang.FR => "Agents pour cette tâche :",
            _ => "Agenten für diese Aufgabe:"
        };

        public static string PinToolTooltip => Current switch
        {
            Lang.EL => "Καρφίτσωμα (εμφανίζεται πρώτο στην κατηγορία του)",
            Lang.EN => "Pin (shows first within its category)",
            Lang.FR => "Épingler (s'affiche en premier dans sa catégorie)",
            _ => "Anheften (wird zuerst in seiner Kategorie angezeigt)"
        };

        public static string OpenFreeVersion => Current switch
        {
            Lang.EL => "Άνοιγμα δωρεάν έκδοσης ↗",
            Lang.EN => "Open free version ↗",
            Lang.FR => "Ouvrir la version gratuite ↗",
            _ => "Kostenlose Version öffnen ↗"
        };

        public static string AddToClaudeDesktop => Current switch
        {
            Lang.EL => "Πρόσθεσε στο Claude Desktop (MCP) ↗",
            Lang.EN => "Add to Claude Desktop (MCP) ↗",
            Lang.FR => "Ajouter à Claude Desktop (MCP) ↗",
            _ => "Zu Claude Desktop hinzufügen (MCP) ↗"
        };

        public static string UpgradePlan => Current switch
        {
            Lang.EL => "Αναβάθμιση σε πληρωμένο πλάνο ↗",
            Lang.EN => "Upgrade to a paid plan ↗",
            Lang.FR => "Passer à un plan payant ↗",
            _ => "Auf einen kostenpflichtigen Plan upgraden ↗"
        };

        public static string PerToolBreakdown => Current switch
        {
            Lang.EL => "Ανά εργαλείο ▾",
            Lang.EN => "Per tool ▾",
            Lang.FR => "Par outil ▾",
            _ => "Pro Tool ▾"
        };

        public static string CharactersUnit => Current switch
        {
            Lang.EL => "χαρακτήρες",
            Lang.EN => "characters",
            Lang.FR => "caractères",
            _ => "Zeichen"
        };

        public static string NotConnected => Current switch
        {
            Lang.EL => "— (δεν έχει συνδεθεί)",
            Lang.EN => "— (not connected)",
            Lang.FR => "— (non connecté)",
            _ => "— (nicht verbunden)"
        };

        public static string LinkOpenError => Current switch
        {
            Lang.EL => "Δεν ήταν δυνατό το άνοιγμα του link",
            Lang.EN => "Could not open the link",
            Lang.FR => "Impossible d'ouvrir le lien",
            _ => "Der Link konnte nicht geöffnet werden"
        };

        public static string ThemeToggleTooltip => Current switch
        {
            Lang.EL => "Εναλλαγή Light / Dark mode",
            Lang.EN => "Toggle Light / Dark mode",
            Lang.FR => "Basculer mode clair / sombre",
            _ => "Hell-/Dunkelmodus umschalten"
        };

        public static string DisclaimerHeader => Current switch
        {
            Lang.EL => "Αποποίηση ευθύνης ⚖",
            Lang.EN => "Disclaimer ⚖",
            Lang.FR => "Avertissement ⚖",
            _ => "Haftungsausschluss ⚖"
        };

        public static string ThinkingHeader => Current switch
        {
            Lang.EL => "🧠 Σκεπτικό (κλικ για προβολή)",
            Lang.EN => "🧠 Thinking (click to view)",
            Lang.FR => "🧠 Raisonnement (cliquer pour afficher)",
            _ => "🧠 Denkprozess (klicken zum Anzeigen)"
        };

        public static string DisclaimerText => Current switch
        {
            Lang.EL =>
                "Η εφαρμογή παρέχεται \"ΩΣ ΕΧΕΙ\", χωρίς καμία εγγύηση, ρητή ή σιωπηρή, συμπεριλαμβανομένων -χωρίς περιορισμό- εγγυήσεων " +
                "εμπορευσιμότητας, καταλληλότητας για συγκεκριμένο σκοπό και μη παραβίασης δικαιωμάτων τρίτων. Τη χρησιμοποιείς αποκλειστικά " +
                "με δική σου ευθύνη. Σε καμία περίπτωση ο δημιουργός δεν φέρει ευθύνη για οποιαδήποτε αξίωση, ζημία, απώλεια δεδομένων ή άλλη " +
                "ευθύνη -είτε στο πλαίσιο σύμβασης, αδικοπραξίας είτε άλλως- που προκύπτει από ή σχετίζεται με την εφαρμογή ή τη χρήση της, " +
                "συμπεριλαμβανομένης τυχόν δυσλειτουργίας του συστήματός σου. Τα κλειδιά API που εισάγεις χρησιμοποιούνται με δική σου ευθύνη " +
                "και οποιαδήποτε χρέωση γίνεται στον δικό σου λογαριασμό στον αντίστοιχο πάροχο (π.χ. Anthropic). Τα εργαλεία/agents τρίτων " +
                "που εμφανίζονται στον κατάλογο δεν ανήκουν ούτε ελέγχονται από αυτή την εφαρμογή· η χρήση τους διέπεται από τους δικούς τους " +
                "όρους χρήσης και πολιτικές απορρήτου.",
            Lang.EN =>
                "This app is provided \"AS IS\", without warranty of any kind, express or implied, including but not limited to " +
                "warranties of merchantability, fitness for a particular purpose, and non-infringement. You use it entirely at your own risk. " +
                "In no event shall the author be liable for any claim, damages, data loss, or other liability — whether in an action of " +
                "contract, tort, or otherwise — arising from, out of, or in connection with the app or its use, including any malfunction of " +
                "your system. API keys you enter are used at your own risk and any charges are billed to your own account with the " +
                "corresponding provider (e.g. Anthropic). The third-party tools/agents listed in the catalog are not owned or controlled by " +
                "this app; their use is governed by their own terms of service and privacy policies.",
            Lang.FR =>
                "Cette application est fournie \"TELLE QUELLE\", sans garantie d'aucune sorte, expresse ou implicite, y compris, sans s'y " +
                "limiter, les garanties de qualité marchande, d'adéquation à un usage particulier et d'absence de contrefaçon. Tu l'utilises " +
                "entièrement à tes propres risques. En aucun cas l'auteur ne pourra être tenu responsable de toute réclamation, dommage, perte " +
                "de données ou autre responsabilité — contractuelle, délictuelle ou autre — découlant de ou liée à l'application ou à son " +
                "utilisation, y compris tout dysfonctionnement de ton système. Les clés API que tu saisis sont utilisées à tes propres " +
                "risques et tout frais est facturé sur ton propre compte auprès du fournisseur concerné (par ex. Anthropic). Les outils/agents " +
                "tiers listés dans le catalogue n'appartiennent pas à cette application et ne sont pas contrôlés par elle ; leur utilisation " +
                "est régie par leurs propres conditions d'utilisation et politiques de confidentialité.",
            _ =>
                "Diese App wird \"WIE BESEHEN\" bereitgestellt, ohne jegliche Gewährleistung, weder ausdrücklich noch stillschweigend, " +
                "einschließlich, aber nicht beschränkt auf Gewährleistungen der Marktgängigkeit, der Eignung für einen bestimmten Zweck und " +
                "der Nichtverletzung von Rechten Dritter. Du nutzt sie vollständig auf eigenes Risiko. Der Autor haftet unter keinen Umständen " +
                "für Ansprüche, Schäden, Datenverlust oder sonstige Haftung — sei es aus Vertrag, unerlaubter Handlung oder anderweitig — die " +
                "sich aus oder im Zusammenhang mit der App oder ihrer Nutzung ergeben, einschließlich einer Fehlfunktion deines Systems. " +
                "Eingegebene API-Schlüssel werden auf eigenes Risiko verwendet, und etwaige Kosten werden deinem eigenen Konto beim " +
                "jeweiligen Anbieter (z. B. Anthropic) belastet. Die im Katalog aufgeführten Drittanbieter-Tools/Agenten gehören nicht dieser " +
                "App und werden nicht von ihr kontrolliert; ihre Nutzung unterliegt deren eigenen Nutzungsbedingungen und Datenschutzrichtlinien."
        };

        public static string HelpHeader => Current switch
        {
            Lang.EL => "Βοήθεια ℹ",
            Lang.EN => "Help ℹ",
            Lang.FR => "Aide ℹ",
            _ => "Hilfe ℹ"
        };

        public static string HistoryHeader => Current switch
        {
            Lang.EL => $"Ιστορικό εκδόσεων (v{AppVersion}) 🕘",
            Lang.EN => $"Version history (v{AppVersion}) 🕘",
            Lang.FR => $"Historique des versions (v{AppVersion}) 🕘",
            _ => $"Versionsverlauf (v{AppVersion}) 🕘"
        };

        public static string HelpText => Current switch
        {
            Lang.EL =>
                "1. Κλειδί API: πάτησε το πορτοκαλί εικονίδιο \"C\" πάνω-αριστερά → επίλεξε \"Claude\" στο Agent → επικόλλησε το " +
                "κλειδί σου από το console.anthropic.com. Χρειάζεται μόνο για τον Claude· τα υπόλοιπα εργαλεία δεν έχουν ενεργή σύνδεση.\n" +
                "2. Chat: γράψε στο πεδίο κάτω από τον τίτλο τι θέλεις να φτιάξεις και πάτησε \"Αποστολή\" (ή Enter). Ο Claude θα " +
                "προτείνει σχετικά εργαλεία σαν chips κάτω από την απάντησή του — πάτησέ τα για λεπτομέρειες.\n" +
                "3. Κατηγορίες: οι έγχρωμες καρτέλες (Κείμενο, Εικόνα, Βίντεο...) φιλτράρουν τον κατάλογο — μόνο μία κατηγορία " +
                "φαίνεται κάθε φορά.\n" +
                "4. Λεπτομέρειες εργαλείου: πάτησε πάνω σε οποιοδήποτε πλακίδιο για να ανοίξει πλαϊνό πάνελ με περιγραφή και " +
                "κουμπιά \"Άνοιγμα δωρεάν έκδοσης\", \"Αναβάθμιση\" και (αν υπάρχει) \"Πρόσθεσε στο Claude Desktop (MCP)\".\n" +
                "5. Θέμα: το κουμπί 🌙/☀ στην πλευρική μπάρα εναλλάσσει light/dark mode.\n" +
                "6. Γλώσσα: πάτησε τη σημαία στην πλευρική μπάρα και διάλεξε Ελληνικά/English/Français/Deutsch από τη λίστα.\n" +
                "7. Εισαγωγή project: στις Ρυθμίσεις (γρανάζι) → Project & Συζήτηση υπάρχουν κουμπιά για εισαγωγή τοπικού " +
                "αρχείου/φακέλου που μπαίνει σαν πλαίσιο στη συζήτηση. Το κουμπί \"C\" ανοίγει αποκλειστικά τις ρυθμίσεις " +
                "του agent (κλειδί, οδηγίες, MCP, Hive Memory).\n" +
                "8. Tokens: η κάτω γραμμή δείχνει πραγματική χρήση tokens του Claude και συνολικά όλων των εργαλείων (\"Ανά " +
                "εργαλείο\" για ανάλυση).\n" +
                "9. Συντομεύσεις πληκτρολογίου: Ctrl+N νέα συζήτηση, Ctrl+, ρυθμίσεις, Ctrl+K γρήγορη εντολή " +
                "(αναζήτηση σε εργαλεία/εντολές/ρυθμίσεις), Esc κλείνει το ανοιχτό παράθυρο/πάνελ, Ctrl+Alt+K " +
                "φέρνει το Kombos μπροστά από οπουδήποτε στα Windows.\n" +
                "10. Εντολές με / (τοπικές, μηδενικό κόστος tokens): /remember <κάτι> και /hive για τη συλλογική μνήμη, " +
                "/readfile <διαδρομή> και /verify για το ενεργό project, /json /base64 /hash /uuid /color /timestamp /regex " +
                "/git /http /cron από την Εργαλειοθήκη Developer, /wordcount /lorem /password /diff /case από το Office Toolkit, " +
                "/gradient /aspect-ratio /palette /contrast /favicon-checklist από τη Δημιουργική Εργαλειοθήκη " +
                "(ενεργοποιούνται από Ρυθμίσεις → Επεκτάσεις → Λήψη & Ενσωμάτωση).",
            Lang.EN =>
                "1. API key: click the orange \"C\" icon top-left → select \"Claude\" as the Agent → paste your key from " +
                "console.anthropic.com. Only Claude has an active connection; the other tools don't.\n" +
                "2. Chat: type what you'd like to build in the box under the title and click \"Send\" (or Enter). Claude will " +
                "suggest relevant tools as chips under its reply — click one for details.\n" +
                "3. Categories: the colored tabs (Text, Image, Video...) filter the catalog — only one category shows at a time.\n" +
                "4. Tool details: click any tile to open a side panel with a description and \"Open free version\", " +
                "\"Upgrade\", and (if available) \"Add to Claude Desktop (MCP)\" buttons.\n" +
                "5. Theme: the 🌙/☀ button in the sidebar toggles light/dark mode.\n" +
                "6. Language: click the flag in the sidebar and pick Greek/English/French/German from the list.\n" +
                "7. Import a project: in Settings (gear) → Project & Chat there are buttons to import a local file/folder as " +
                "context into the chat. The \"C\" button opens only the agent's own settings (key, instructions, MCP, Hive Memory).\n" +
                "8. Tokens: the bottom bar shows real Claude token usage and the combined total across tools (\"Per tool\" for a " +
                "breakdown).\n" +
                "9. Keyboard shortcuts: Ctrl+N new chat, Ctrl+, settings, Ctrl+K command palette (search tools/" +
                "commands/settings), Esc closes the open window/panel, Ctrl+Alt+K brings Kombos to the front " +
                "from anywhere in Windows.\n" +
                "10. Slash commands (local, zero token cost): /remember <something> and /hive for hive memory, /readfile " +
                "<path> and /verify for the active project, /json /base64 /hash /uuid /color /timestamp /regex /git /http " +
                "/cron from the Dev Toolkit, /wordcount /lorem /password /diff /case from the Office Toolkit, " +
                "/gradient /aspect-ratio /palette /contrast /favicon-checklist from the Creative Toolkit (enable them " +
                "from Settings → Extensions → Download & Install).",
            Lang.FR =>
                "1. Clé API : clique sur l'icône orange « C » en haut à gauche → choisis « Claude » comme Agent → colle ta clé " +
                "depuis console.anthropic.com. Seul Claude a une connexion active ; les autres outils non.\n" +
                "2. Chat : écris ce que tu veux créer dans le champ sous le titre et clique sur « Envoyer » (ou Entrée). Claude " +
                "suggérera des outils pertinents sous forme de puces sous sa réponse — clique dessus pour les détails.\n" +
                "3. Catégories : les onglets colorés (Texte, Image, Vidéo...) filtrent le catalogue — une seule catégorie " +
                "s'affiche à la fois.\n" +
                "4. Détails d'un outil : clique sur une carte pour ouvrir un panneau latéral avec une description et les " +
                "boutons « Ouvrir la version gratuite », « Passer à un plan payant » et (si disponible) « Ajouter à Claude " +
                "Desktop (MCP) ».\n" +
                "5. Thème : le bouton 🌙/☀ dans la barre latérale bascule le mode clair/sombre.\n" +
                "6. Langue : clique sur le drapeau dans la barre latérale et choisis grec/anglais/français/allemand dans la liste.\n" +
                "7. Importer un projet : dans Paramètres (roue dentée) → Projet & Discussion, des boutons permettent d'importer " +
                "un fichier/dossier local comme contexte dans le chat. Le bouton « C » n'ouvre que les paramètres de l'agent " +
                "(clé, instructions, MCP, mémoire collective).\n" +
                "8. Tokens : la barre du bas affiche l'usage réel de tokens de Claude et le total combiné de tous les outils " +
                "(« Par outil » pour le détail).\n" +
                "9. Raccourcis clavier : Ctrl+N nouvelle conversation, Ctrl+, paramètres, Ctrl+K palette de " +
                "commandes (recherche outils/commandes/paramètres), Échap ferme la fenêtre/le panneau ouvert, " +
                "Ctrl+Alt+K ramène Kombos au premier plan depuis n'importe où sous Windows.\n" +
                "10. Commandes slash (locales, coût en tokens nul) : /remember <quelque chose> et /hive pour la mémoire " +
                "collective, /readfile <chemin> et /verify pour le projet actif, /json /base64 /hash /uuid /color /timestamp " +
                "/regex /git /http /cron depuis la boîte à outils développeur, /wordcount /lorem /password /diff /case depuis " +
                "la boîte à outils bureautique, /gradient /aspect-ratio /palette /contrast /favicon-checklist depuis la " +
                "boîte à outils créative (à activer depuis Paramètres → Extensions → Télécharger et installer).",
            _ =>
                "1. API-Schlüssel: Klicke oben links auf das orangefarbene „C“-Symbol → wähle „Claude“ als Agent → füge deinen " +
                "Schlüssel von console.anthropic.com ein. Nur Claude hat eine aktive Verbindung, die anderen Tools nicht.\n" +
                "2. Chat: Schreibe unter dem Titel, was du bauen möchtest, und klicke auf „Senden“ (oder Enter). Claude schlägt " +
                "passende Tools als Chips unter seiner Antwort vor — klicke darauf für Details.\n" +
                "3. Kategorien: Die farbigen Tabs (Text, Bild, Video...) filtern den Katalog — es wird immer nur eine Kategorie " +
                "angezeigt.\n" +
                "4. Tool-Details: Klicke auf eine Kachel, um ein Seitenpanel mit Beschreibung und den Schaltflächen „Kostenlose " +
                "Version öffnen“, „Upgrade“ und (falls verfügbar) „Zu Claude Desktop hinzufügen (MCP)“ zu öffnen.\n" +
                "5. Theme: Die Schaltfläche 🌙/☀ in der Seitenleiste wechselt zwischen Hell-/Dunkelmodus.\n" +
                "6. Sprache: Klicke auf die Flagge in der Seitenleiste und wähle Griechisch/Englisch/Französisch/Deutsch aus " +
                "der Liste.\n" +
                "7. Projekt importieren: Unter Einstellungen (Zahnrad) → Projekt & Chat gibt es Schaltflächen, um eine lokale " +
                "Datei/einen Ordner als Kontext in den Chat zu importieren. Die Schaltfläche „C“ öffnet ausschließlich die " +
                "Agenteneinstellungen (Schlüssel, Anweisungen, MCP, Kollektivgedächtnis).\n" +
                "8. Tokens: Die untere Leiste zeigt den echten Claude-Token-Verbrauch und die Gesamtsumme aller Tools („Pro " +
                "Tool“ für eine Aufschlüsselung).\n" +
                "9. Tastenkombinationen: Strg+N neuer Chat, Strg+, Einstellungen, Strg+K Befehlspalette (Suche " +
                "über Tools/Befehle/Einstellungen), Esc schließt das offene Fenster/Panel, Strg+Alt+K holt " +
                "Kombos von überall in Windows in den Vordergrund.\n" +
                "10. Slash-Befehle (lokal, keine Token-Kosten): /remember <etwas> und /hive für das Kollektivgedächtnis, " +
                "/readfile <Pfad> und /verify für das aktive Projekt, /json /base64 /hash /uuid /color /timestamp /regex " +
                "/git /http /cron aus dem Entwickler-Toolkit, /wordcount /lorem /password /diff /case aus dem Office-Toolkit, " +
                "/gradient /aspect-ratio /palette /contrast /favicon-checklist aus dem Kreativ-Toolkit " +
                "(aktivierbar unter Einstellungen → Erweiterungen → Herunterladen & Installieren)."
        };

        public static string VersionHistoryText => Current switch
        {
            Lang.EL =>
                "v4.8.0 — <b>Αναδίπλωση πλευρικού μενού, όπως στο MotionDeskTop</b>: η \"αναπτυγμένη\" " +
                "κατάσταση (προεπιλογή) δείχνει τώρα εικονίδιο+ετικέτα σε κάθε κουμπί (Θέμα/Γλώσσα/Εμφάνιση/" +
                "Artifacts/Projects/Ρυθμίσεις), όχι μόνο γυμνά εικονίδια - η \"διπλωμένη\" κατάσταση παραμένει " +
                "η γνωστή στενή στήλη μόνο-εικονιδίων· το κουμπί αναδίπλωσης πλέον ζει μέσα στην ίδια στήλη " +
                "(κάτω από τα εικονίδια) και ακολουθεί αυτόματα το sidebar είτε είναι αριστερά είτε δεξιά, " +
                "χωρίς πλέον χειροκίνητους υπολογισμούς θέσης μεταξύ στηλών.\n" +
                "v4.7.0 — Οπτικό & χρηστικότητας πέρασμα: <b>vector εικονίδια</b> αντί για emoji σε chrome " +
                "παραθύρου, κεντρική εργαλειοθήκη και sidebar (καθαρό στυλ γραμμής) · <b>dark mode σε " +
                "ουδέτερο ανθρακί #171717</b> (αντί για το προηγούμενο ζεστό καφέ-μαύρο) · <b>αναδίπλωση " +
                "πλευρικού μενού</b> με κουμπί κάτω από τα εικονίδια, κεντραρισμένο μαζί τους · κουμπιά " +
                "<b>📋 Επικόλληση</b> στα πεδία κλειδιού API/username/password (ασφαλής εναλλακτική αντί για " +
                "ανάγνωση αποθηκευμένων κωδικών browser, που αρνηθήκαμε ρητά για λόγους ασφαλείας) · η " +
                "αυτόματη συνέχιση σε άλλο agent (v4.6.0) έγινε πιο σωστή: pop-up Ναι/Όχι ΤΗΝ ΠΡΩΤΗ ΦΟΡΑ, " +
                "μετά θυμάται την επιλογή και συνεχίζει αυτόματα χωρίς να ξαναρωτήσει · οι Ρυθμίσεις Agent " +
                "αναδιοργανώθηκαν σε αναδιπλούμενες ενότητες (λιγότερη συμφόρηση). Ζωντανό τεστ σε πραγματικό " +
                "instance (UI Automation, χωρίς κλειδιά): Gateway endpoint επιβεβαιώθηκε λειτουργικό άκρη-σε-" +
                "άκρη· βρέθηκε ότι το Ctrl+Alt+Space (Kombos Anywhere) μπορεί να συγκρούεται σιωπηλά με άλλη " +
                "εφαρμογή σε ορισμένα μηχανήματα (γνωστό ζήτημα, ανοιχτό για διόρθωση).\n" +
                "v4.6.0 — Κόμβος Gateway (ρητό αίτημα μετά την «τελική» v4.5.0, εξαίρεση όχι scope creep): " +
                "νέα τοπική HTTP πύλη, εμπνευσμένη από το ανοιχτού-κώδικα OmniRoute " +
                "(github.com/diegosouzapw/OmniRoute, MIT) - ένα OpenAI-compatible endpoint " +
                "(<code>http://127.0.0.1:PORT/v1/chat/completions</code>) που άλλα εργαλεία (VS Code, Cursor, " +
                "Claude Code κ.ά.) μπορούν να χρησιμοποιήσουν αντί να μιλάνε απευθείας σε κάποιον provider. " +
                "Δρομολογεί αυτόματα στον κεντρικό agent του Κόμβου, με αυτόματο <b>fallback</b> στον επόμενο " +
                "ρυθμισμένο agent αν ο πρώτος αποτύχει (rate limit/quota/δίκτυο) - το ίδιο πνεύμα με το " +
                "\"automatic fallback across 359 providers\" του OmniRoute, αλλά ρυθμισμένο εξ ολοκλήρου μέσα " +
                "από το ήδη φιλικό GUI του Κόμβου (Ρυθμίσεις → Συμπεριφορά: ένα κουτάκι + μία θύρα), όχι " +
                ".env αρχεία/CLI/Docker/ξεχωριστό dashboard - αυτό είναι το \"πιο ευχάριστο OmniRoute\" που " +
                "ζητήθηκε. Ασφάλεια by design: ακούει ΑΠΟΚΛΕΙΣΤΙΚΑ σε 127.0.0.1, ποτέ στο δίκτυο. Καμία " +
                "γραμμή κώδικα δεν αντιγράφηκε από το OmniRoute (διαφορετική γλώσσα/αρχιτεκτονική - Node.js " +
                "εκεί, C#/.NET εδώ) - μόνο η ιδέα της ενιαίας τοπικής πύλης με αυτόματο fallback υλοποιήθηκε " +
                "από την αρχή, χρησιμοποιώντας τα κλειδιά που έχεις ήδη βάλει στο Κόμβος. Μαζί με αυτό, νέα " +
                "<b>αυτόματη συνέχιση όταν το Claude πλησιάζει/χτυπάει το όριό του</b> - ρητό αίτημα χρήστη: " +
                "ΤΗΝ ΠΡΩΤΗ ΦΟΡΑ εμφανίζεται pop-up προτείνοντας τον πρώτο ήδη-ρυθμισμένο agent (Ναι/Όχι), και " +
                "η επιλογή σου θυμάται από εκεί και πέρα - το Κόμβος τραβάει αυτόματα από τον ίδιο agent τις " +
                "επόμενες φορές, χωρίς να ξαναρωτήσει, ώστε η συζήτηση/project να συνεχίζει απρόσκοπτα. " +
                "Ρυθμίσεις → Συμπεριφορά έχει το κουτάκι ενεργοποίησης + κουμπί \"Επαναφορά\" για να διαλέξεις " +
                "διαφορετικό agent αργότερα. Η φούσκα της απάντησης πάντα δείχνει ποιος agent απάντησε στην " +
                "πραγματικότητα - καμία σιωπηλή/κρυφή εναλλαγή.\n" +
                "v4.5.0 — ΤΕΛΙΚΗ ΕΚΔΟΣΗ: το Κόμβος ολοκληρώνεται ως πλήρες πολυεργαλείο, καμία έκδοση δεν " +
                "προγραμματίζεται μετά από αυτή. Μεγάλο κύμα προσθηκών: <b>🔄 Έλεγχος για ενημερώσεις</b> " +
                "(Ρυθμίσεις → Πληροφορίες, μόνο χειροκίνητα, ποτέ σιωπηλή λήψη/εγκατάσταση) · <b>Εισαγωγή " +
                "από ChatGPT/Claude.ai</b> (αρχεία εξαγωγής conversations.json) · <b>Kombos Anywhere</b> " +
                "(Ctrl+Alt+Space ανοίγει από οπουδήποτε ένα mini quick-capture bar, με δυνατότητα προσθήκης " +
                "της απάντησης στην κύρια συζήτηση) · <b>Multi-window</b> (απόσπαση οποιασδήποτε συζήτησης σε " +
                "δικό της παράθυρο, με \"Αντιγραφή όλων ως Markdown\") · <b>/image</b> (πραγματική δημιουργία " +
                "εικόνας μέσα στη συνομιλία μέσω OpenAI Images API, με το ήδη αποθηκευμένο κλειδί ChatGPT) · " +
                "<b>Ζωντανή συνεργασία</b> (δύο υπολογιστές με τον ίδιο φάκελο συγχρονισμού βλέπουν τις " +
                "αλλαγές του άλλου αυτόματα, μέσω παρακολούθησης αρχείων) · <b>Τοπικό analytics dashboard</b> " +
                "(γράφημα tokens ανά συνεδρία + ανάλυση ανά εργαλείο, καμία τηλεμετρία) · <b>Plugin SDK v2</b> " +
                "(οδηγός \"Νέο Plugin\" δημιουργεί έτοιμο-προς-build project) · <b>Nexus Marketplace</b> " +
                "(εγκατάσταση plugin από φάκελο ή Git URL) · <b>Artifact live preview</b> (πραγματική απόδοση " +
                "HTML/SVG/Markdown σε δικό του παράθυρο, όχι μόνο λίστα κώδικα) · πέρασμα προσβασιμότητας " +
                "(AutomationProperties.Name σε βασικά εικονίδια/κουμπιά παραθύρου). Σχετικά με \"ταχύτητα/" +
                "κόστος απάντησης\": το standing cache-friendly system prompt (από v2.8.0) ισχύει ήδη σε κάθε " +
                "agent· πραγματική αυτόματη δρομολόγηση σε φθηνότερο εργαλείο ή batching αξιολογήθηκαν και " +
                "ΔΕΝ υλοποιήθηκαν σκόπιμα (θα σήμαινε το Κόμβος να παρακάμπτει σιωπηλά την επιλογή agent του " +
                "χρήστη, ή δεν ταιριάζει σε πραγματικού-χρόνου chat) - ειλικρινής επιλογή, όχι παράλειψη.\n" +
                "v4.3.0 — Τοπική αναζήτηση project + ενέργειες μηνύματος (δεύτερο κύμα προς την τελική " +
                "v4.5.0): νέα εντολή <code>/search &lt;λέξεις-κλειδιά&gt;</code> - ψάχνει ΟΛΟ το περιεχόμενο " +
                "του ενεργού project (όχι μόνο το δέντρο αρχείων όπως το /readfile), τοπικά, με βαθμολόγηση " +
                "συχνότητας όρου, μηδενικό κόστος tokens ('local RAG-lite' χωρίς embeddings/vector DB). Δεξί " +
                "κλικ σε οποιοδήποτε μήνυμα πλέον προσφέρει \"Αντιγραφή ως Markdown\", \"Αντιγραφή ως απλό " +
                "κείμενο\" και \"Παράθεση σε νέο μήνυμα\".\n" +
                "v4.2.0 — Φωνή + Presets + Focus mode (πρώτο κύμα προς την τελική v4.5.0): νέο κουμπί " +
                "🔊 \"Ανάγνωση\" σε κάθε απάντηση agent (τοπική, δωρεάν ανάγνωση δυνατά μέσω System.Speech, " +
                "χωρίς cloud/API key), και νέο κουμπί 🎤 φωνητικής εισαγωγής δίπλα στο πεδίο μηνύματος " +
                "(push-to-talk: πάτα, μίλα μία φορά, ξαναπάτα - όχι συνεχής ακρόαση). Νέα ενότητα " +
                "\"Presets\" στις ρυθμίσεις Agent: αποθήκευσε τον τρέχοντα συνδυασμό agent + οδηγιών με ένα " +
                "όνομα, εφάρμοσέ τον ξανά αργότερα με ένα κλικ αντί να ξαναγράφεις τις ίδιες οδηγίες. Νέα " +
                "\"Λειτουργία εστίασης\" (F11 ή μενού Προβολή στο Windows Classic): κρύβει sidebar/menu " +
                "bar/κεφαλίδα/κατάλογο εργαλείων/γραμμή κατάστασης, αφήνοντας μόνο τη συζήτηση - Esc βγάζει " +
                "από αυτήν. Η v4.5.0 έχει οριστεί ως η ΤΕΛΙΚΗ έκδοση του Κόμβου· ό,τι ακολουθεί χτίζεται προς " +
                "εκείνο το σημείο, δες το πλήρες roadmap για τα υπόλοιπα εκκρεμή.\n" +
                "v4.1.0 — Πραγματική διόρθωση Light/Dark + κλάδεμα συζήτησης + Βιβλιοθήκη Prompts: το dark " +
                "mode \"δεν λειτουργούσε\" ορατά στο skin Windows Classic - βρέθηκε η πραγματική αιτία (η " +
                "παλέτα του Classic αγνοούσε εντελώς το dark mode) και προστέθηκε πραγματική σκοτεινή " +
                "παραλλαγή. Νέα επιλογή \"Θέμα\" στην Εμφάνιση με 3 καταστάσεις: Φωτεινό / Σκοτεινό / Σύστημα " +
                "(ακολουθεί αυτόματα και ζωντανά το θέμα των Windows). Διορθώθηκε και η θέση των pop-up " +
                "γλώσσας/Artifacts στο Windows Classic (αγκυρώνονταν σωστά αλλά με λάθος Placement, έβγαιναν " +
                "εκτός παραθύρου) και το πολύ στενό πεδίο τίτλου στο \"+ Νέο Project\". Έρευνα σε παρόμοιες " +
                "εφαρμογές ανοιχτού κώδικα (LibreChat, NextChat, Cherry Studio) έδωσε δύο νέες, δικές μας " +
                "υλοποιήσεις: \"🔀 Κλάδεμα\" συζήτησης από οποιοδήποτε μήνυμα (νέα ανεξάρτητη συζήτηση με το " +
                "ιστορικό μέχρι εκείνο το σημείο, χωρίς να αλλοιώνεται η αρχική), και \"📚 Βιβλιοθήκη Prompts\" " +
                "(10 δικά μας, πρωτότυπα σημεία εκκίνησης σε 5 κατηγορίες - μηδενικό κόστος tokens μέχρι να " +
                "στείλεις ένα). Η καθολική αναζήτηση σε ΟΛΕΣ τις συζητήσεις (τίτλο + περιεχόμενο) ήταν ήδη " +
                "υλοποιημένη από νωρίτερα.\n" +
                "v4.0.0 — Ορόσημο: διορθώσεις skins/παραθύρου + Λογαριασμοί + σελιδοποίηση καταλόγου. Το skin " +
                "Windows Classic είχε ξεχασμένα popups (γλώσσα, Artifacts, Projects/ιστορικό) που έμεναν " +
                "στρογγυλεμένα και εμφανίζονταν στην (κρυμμένη πλέον) θέση της πλευρικής μπάρας - " +
                "διορθώθηκαν πλήρως, μαζί με μια βαθύτερη αιτία: ένα MenuItem του classic menu bar δεν είναι " +
                "ασφαλές σημείο αγκύρωσης μόλις κλείσει το μενού του, οπότε τα popups αποτύγχαναν σιωπηλά να " +
                "ανοίξουν. Το skin Nexus σε dark mode πήρε νέα ανθρακί/γραφίτη παλέτα (στυλ Adobe Acrobat Pro " +
                "dark mode) αντί για το πολύ σκοτεινό προηγούμενο. Η μεγιστοποίηση σέβεται πλέον ακριβώς τα " +
                "όρια της οθόνης (taskbar/πολυ-οθονικές διατάξεις), αντί να ξεχειλίζει. Ο κατάλογος εργαλείων " +
                "σελιδοποιείται πλέον με πλευρικά βελάκια αντί για scrollbar. Νέο παράθυρο \"Λογαριασμοί\" " +
                "(από το εικονίδιο στη γραμμή εργασιών) που δείχνει ποια εργαλεία έχουν αποθηκευμένα στοιχεία " +
                "και ανοίγει την πραγματική σελίδα σύνδεσης/εγγραφής στον browser σου - το Kombos δεν κάνει " +
                "ποτέ σύνδεση ή εγγραφή για λογαριασμό σου.\n" +
                "v3.9.0 — Projects (στυλ Claude Desktop) + διορθώσεις: νέο section \"PROJECTS\" πάνω από τη " +
                "λίστα συζητήσεων με κουμπί \"+ Νέο Project\" - ομαδοποίησε συζητήσεις κάτω από ένα όνομα, νέες " +
                "συζητήσεις μέσα σε ένα Project ανατίθενται αυτόματα σε αυτό. Το κουμπί επισύναψης δέχεται " +
                "πλέον ΚΑΘΕ είδος αρχείου (όχι μόνο εικόνες) - .md/.txt/.json/κώδικας μπαίνουν σαν πλαίσιο " +
                "κειμένου στη συζήτηση. Νέα προαιρετικά πεδία username/password ανά agent στις ρυθμίσεις " +
                "(κρυπτογραφημένα με DPAPI, μόνο για δική σου αναφορά - ποτέ αυτόματη σύνδεση σε ιστοσελίδα). " +
                "Νέο εργαλείο καταλόγου \"OmniRoute\" (τοπικό AI gateway τρίτου κατασκευαστή, community " +
                "project - χρειάζεται να το εκκινήσεις εσύ). Το GitHub MCP connector προσυμπληρώνεται σαν " +
                "παράδειγμα στους MCP servers (χρειάζεται OAuth που δεν αυτοματοποιείται ακόμα). Διορθώσεις: το " +
                "κινούμενο φόντο \"Τροχιές\" δείχνει πλέον τη γραμμή της τροχιάς, και το αριστερό γρανάζι στο " +
                "\"Γρανάζια\" δεν επικαλύπτεται πια με το κινούμενο εικονίδιο του τίτλου.\n" +
                "v3.8.0 — Command Palette + Εργαλειοθήκη Επιχειρήσεων + Κοινόχρηστα Projects: νέο Ctrl+K " +
                "παράθυρο γρήγορης εντολής με αναζήτηση σε εργαλεία/εντολές/ρυθμίσεις. Νέα Εργαλειοθήκη " +
                "Επιχειρήσεων (τοπικά plugins, μηδενικό κόστος tokens): /vat, /loan, /percent, /workdays " +
                "(εργάσιμες ημέρες με ελληνικές αργίες), /iban, /invoice-number. Νέα \"Κοινόχρηστα Projects\": " +
                "δημοσίευσε ένα Project Memory project σαν ZIP μέσα στον φάκελο συγχρονισμού - κάθε υπολογιστής " +
                "με τον ίδιο φάκελο βλέπει τη λίστα με κατάσταση (Εκκρεμεί/Σε εξέλιξη/Ολοκληρώθηκε) και μπορεί " +
                "να κατεβάσει το ZIP (δεν υπάρχει δημόσιο API για τα Projects του claude.ai, οπότε αυτό είναι η " +
                "πραγματική εναλλακτική). Διορθώσεις: το skin Windows Classic έχει πλέον σωστό γκρι φόντο/navy " +
                "τίτλου παντού, και οι ετικέτες της κάθετης ράγας στο skin Nexus δεν κόβονται πια.\n" +
                "v3.7.0 — Οπτική αναβάθμιση + Δημιουργική Εργαλειοθήκη: δύο νέα skins (Windows Classic - " +
                "τετράγωνες γωνίες παντού, ανάγλυφα 3D πλαίσια στα πλακίδια, navy τίτλου, και ΚΛΑΣΙΚΟ ΜΕΝΟΥ " +
                "Αρχείο/Επεξεργασία/Προβολή/Εργαλεία/Βοήθεια αντί για την πλευρική μπάρα εικονιδίων· Nexus - " +
                "πρωτότυπο πορτοκαλί branding, κάθετη ράγα καρτελών στις Ρυθμίσεις). Κάθε skin απέκτησε " +
                "αυθεντικό χρώμα εποχής (Aero μπλε, Metro μπλε, Fluent μπλε, navy Classic). Νέα Δημιουργική " +
                "Εργαλειοθήκη (τοπικά plugins, μηδενικό κόστος tokens): /gradient, /aspect-ratio, /palette, " +
                "/contrast (έλεγχος αντίθεσης WCAG), /favicon-checklist.\n" +
                "v3.6.0 — Μεγάλη ενημέρωση \"πολυεργαλείο\": νέα γκαλερί \"Λήψη & Ενσωμάτωση\" στις Ρυθμίσεις → " +
                "Επεκτάσεις για εγκατάσταση/αφαίρεση επίσημων plugins με ένα κλικ (Εργαλειοθήκη Developer με " +
                "/json /base64 /hash /uuid /color /timestamp /regex /git /http /cron, Εργαλειοθήκη Γραφείου με " +
                "/wordcount /lorem /password /diff /case, plugin Καιρού) — όλα τοπικά, μηδενικό κόστος tokens. " +
                "Νέα \"Συλλογική Μνήμη\" (Hive Memory): ό,τι μαθαίνει ένας agent (/remember, /hive) το ξέρουν " +
                "πλέον όλοι οι agents. Νέο Handoff pipeline (🔗 κουμπί δίπλα στο πεδίο μηνύματος): πρόχειρη " +
                "απάντηση από έναν agent, βελτίωση από δεύτερο. Νέα \"Μνήμη Project\": εισαγωγή project ως " +
                "ελαφρύ ευρετήριο (δέντρο αρχείων) αντί για πλήρες περιεχόμενο, στοχευμένη ανάγνωση αρχείου με " +
                "/readfile, και τοπικός έλεγχος συμβατότητας με /verify (τρέχει το build/lint του project χωρίς " +
                "καμία κλήση AI). Ένδειξη ποσοστού prompt-cache στην κάτω γραμμή, το catalog JSON πλέον " +
                "χτίζεται μία φορά αντί σε κάθε μήνυμα, και αυτόματη περικοπή παλιών μηνυμάτων σε πολύ μεγάλες " +
                "συζητήσεις για εξοικονόμηση tokens.\n" +
                "v3.0.0 — Πρώτη πλήρης, «ολοκληρωμένη» έκδοση: αρχείο εγκατάστασης (KombosSetup.exe), χωρίς " +
                "δικαιώματα διαχειριστή, με προαιρετική συντόμευση επιφάνειας εργασίας και εκκίνηση με τα " +
                "Windows κατά την εγκατάσταση. Πλήρες σύστημα plugins, prompt caching + MCP servers για τον " +
                "Claude, ασφαλής διαγραφή/διπλότυπο συζητήσεων, πάνελ Artifacts, φάκελος συγχρονισμού " +
                "συζητήσεων ανάμεσα σε υπολογιστές, οδηγός πρώτης χρήσης, αυτοματοποιημένα tests, και " +
                "σημαντικές διορθώσεις κατανάλωσης μνήμης/επεξεργαστή - όλα ό,τι χτίστηκε από το v2.7.0 έως " +
                "εδώ, μαζεμένα σε μία σταθερή, δοκιμασμένη έκδοση.\n" +
                "v2.9.9 — Μειωμένη κατανάλωση μνήμης/επεξεργαστή: τα λογότυπα εργαλείων πλέον αποθηκεύονται " +
                "σε cache αντί να ξαναφορτώνονται από το διαδίκτυο σε κάθε αλλαγή θέματος/γλώσσας/αναζήτησης, " +
                "η αναζήτηση εργαλείων δεν ξαναχτίζει όλον τον κατάλογο σε κάθε πάτημα πλήκτρου, και τα " +
                "κινούμενα φόντα σταματούν εντελώς όσο το Kombos τρέχει κρυφό στην περιοχή ειδοποιήσεων αντί " +
                "να συνεχίζουν επ' αόριστον στο παρασκήνιο.\n" +
                "v2.9.5 — Καθαρός διαχωρισμός ρυθμίσεων: το γρανάζι ανοίγει τις γενικές ρυθμίσεις εφαρμογής " +
                "(Συμπεριφορά), το \"C\" ανοίγει τις εξατομικευμένες ρυθμίσεις του πρωτεύοντα agent και τη " +
                "συνεργασία με άλλους agents (MCP) - τα ίδια δύο και μοναδικά σημεία εισόδου στις ρυθμίσεις· " +
                "νέο εικονίδιο \"Συζητήσεις\" στην πλευρική μπάρα για γρήγορη πρόσβαση σε τρέχουσες και " +
                "παλαιότερες συζητήσεις.\n" +
                "v2.9.0 — Οδηγός πρώτης χρήσης (onboarding) με tutorial σελίδες, ασφαλής διαγραφή " +
                "μεμονωμένου μηνύματος, διπλότυπο συζήτησης, πάνελ \"Artifacts\" στην πλευρική μπάρα με όλα " +
                "τα μπλοκ κώδικα από κάθε συζήτηση (αντιγραφή/αποθήκευση), φάκελος συγχρονισμού συζητήσεων " +
                "(OneDrive/Dropbox/δίκτυο) για μοίρασμα ανάμεσα σε υπολογιστές, και διόρθωση bug όπου το " +
                "κουμπί \"Επανάληψη\" σε φούσκες σφάλματος μπορούσε να σβήσει πραγματικό ιστορικό.\n" +
                "v2.8.0 — Prompt caching για τον Claude (φθηνότερα turns εφόσον το σύστημα prompt/κατάλογος " +
                "δεν αλλάζει), υποστήριξη απομακρυσμένων MCP servers στις Ρυθμίσεις → Agent (μόνο Claude), και " +
                "κανόνας ελάχιστου κόστους tokens για όλους τους agents χωρίς εξαίρεση (τεκμηριωμένο στον " +
                "κώδικα ανά πάροχο).\n" +
                "v2.7.0 — Σύστημα plugins: νέα καρτέλα \"Επεκτάσεις\" στις Ρυθμίσεις, φάκελος " +
                "%AppData%\\Kombos\\plugins\\ όπου μπορείς να βάλεις επεκτάσεις τρίτων, εντολές \"/κάτι\" στο " +
                "chat που καταχωρούν τα plugins (δείγμα: /weather <πόλη>).\n" +
                "v2.6.0 — Αναζήτηση εργαλείων δίπλα στις καρτέλες κατηγοριών - ψάχνει σε όλες τις κατηγορίες " +
                "ταυτόχρονα.\n" +
                "v2.5.0 — Καρφίτσωμα αγαπημένων εργαλείων (⭐ σε κάθε πλακίδιο και στις λεπτομέρειες - " +
                "εμφανίζονται πρώτα στην κατηγορία τους), νέα καρτέλα \"Συμπεριφορά\" στις Ρυθμίσεις (πάντα σε " +
                "πρώτο πλάνο, εκκίνηση με τα Windows, εκκίνηση ελαχιστοποιημένο, ήχος ειδοποίησης), εξαγωγή/" +
                "εισαγωγή πλήρους αντιγράφου ασφαλείας των συζητήσεων σε JSON, συντομεύσεις πληκτρολογίου " +
                "(Ctrl+N νέα συζήτηση, Ctrl+, ρυθμίσεις, Esc κλείσιμο), και κουμπί \"μετάβαση στο τέλος\" όταν " +
                "σκρολάρεις πάνω σε παλιότερα μηνύματα.\n" +
                "v2.4.0 — Ειδοποίηση από την περιοχή ειδοποιήσεων όταν φτάνει απάντηση ενώ το παράθυρο δεν " +
                "είναι στο προσκήνιο, αναζήτηση στο ιστορικό συζητήσεων πλέον ψάχνει και μέσα στο περιεχόμενο " +
                "(όχι μόνο στον τίτλο), το μισο-γραμμένο κείμενο διατηρείται όταν αλλάζεις συζήτηση, ρυθμιζόμενο " +
                "μέγεθος κειμένου συνομιλίας (Μικρό/Κανονικό/Μεγάλο, στην Εμφάνιση) για προσβασιμότητα, και " +
                "κουμπί διαγραφής όλων των αποθηκευμένων συζητήσεων μαζί (με επιβεβαίωση) στις Ρυθμίσεις.\n" +
                "v2.3.0 — Τα κλειδιά API κρυπτογραφούνται πλέον με DPAPI των Windows πριν αποθηκευτούν στο " +
                "δίσκο (δεμένα στον λογαριασμό σου των Windows) αντί για καθαρό κείμενο.\n" +
                "v2.2.0 — Απόδοση markdown στις απαντήσεις (έντονα/πλάγια/inline code, μπλοκ κώδικα με δικό " +
                "τους κουμπί αντιγραφής), επεξεργασία δικού σου μηνύματος πριν το ξαναστείλεις (✏ σε κάθε " +
                "φούσκα χρήστη), αναζήτηση μέσα στο ιστορικό συζητήσεων, και αρκετές διορθώσεις σταθερότητας: " +
                "το \"Επανάληψη\" τώρα αντικαθιστά σωστά την παλιά απάντηση αντί να προσθέτει δεύτερη από κάτω " +
                "και δεν σβήνει πια πραγματικό ιστορικό αν αποτύχει· τα κλειδιά API πλέον διατηρούνται ανάμεσα " +
                "σε επανεκκινήσεις· διορθώθηκε πιθανή ανάμειξη συζητήσεων αν έφτανε καθυστερημένη απάντηση " +
                "μετά από εναλλαγή σε άλλη συζήτηση.\n" +
                "v2.1.0 — Μετονομασία αποθηκευμένων συζητήσεων (✏ στη λίστα ιστορικού), προσαρμοσμένες οδηγίες " +
                "προς τον κεντρικό agent (Ρυθμίσεις → Agent), μεγαλύτερα και πιο ευδιάκριτα κουμπιά δίπλα στο " +
                "πεδίο μηνύματος, πλήρης διαχωρισμός Βοήθειας από Ιστορικό εκδόσεων σε ξεχωριστές, με έντονη " +
                "γραμματοσειρά ενότητες.\n" +
                "v2.0.0 — Επανάληψη (regenerate) απάντησης, σύγκριση απαντήσεων από πολλαπλά agents ταυτόχρονα " +
                "(⚖ δίπλα στο πεδίο μηνύματος), πολλαπλές αποθηκευμένες συζητήσεις με δικό τους ιστορικό (🕘) " +
                "αντί για μία συνεχόμενη, εικονίδιο στην περιοχή ειδοποιήσεων (system tray) ώστε το Kombos να " +
                "τρέχει στο παρασκήνιο με κλείσιμο παραθύρου, και global hotkey (Ctrl+Alt+K) για άμεση κλήση από " +
                "οπουδήποτε στα Windows.\n" +
                "v1.9.0 — Πραγματική αναζήτηση στο διαδίκτυο (εναλλάξιμη, 🌐 δίπλα στο πεδίο μηνύματος) μέσω του " +
                "επίσημου server-side web search εργαλείου του Claude — αποφασίζει μόνο του πότε χρειάζεται να " +
                "ψάξει, και οι πηγές που βρήκε εμφανίζονται ως clickable links κάτω από την απάντηση.\n" +
                "v1.8.0 — Επικόλληση εικόνας από το clipboard με Ctrl+V (π.χ. στιγμιότυπο οθόνης) απευθείας στο " +
                "chat, drag & drop εικόνας/αρχείου/φακέλου πάνω στο πλαίσιο συζήτησης (αυτόματη αναγνώριση αν " +
                "πρόκειται για επισύναψη εικόνας ή εισαγωγή context), αυξημένη αντίθεση σε όλα τα κινούμενα φόντα " +
                "ώστε να φαίνονται καθαρά σε κάθε θέμα/skin.\n" +
                "v1.7.0 — 10 κινούμενα φόντα κεφαλίδας για επιλογή (Γρανάζια/Τροχιές/Κύματα/Πλέγμα εμπνευσμένα από " +
                "τον Windows Optimizer, plus Δίκτυο κόμβων/Bokeh/Βόρειο σέλας/Έναστρος ουρανός), κουμπί \"Νέα " +
                "συζήτηση\", κουμπί αντιγραφής σε κάθε απάντηση, δυνατότητα διακοπής απάντησης εν εξελίξει, " +
                "πολυγραμμικό πεδίο εισαγωγής (το Shift+Enter τώρα πράγματι βάζει νέα γραμμή), διόρθωση εικονιδίου " +
                "παλέτας στο παράθυρο Εμφάνισης.\n" +
                "v1.6.0 — Επισύναψη εικόνας στο chat (ο Claude τη \"βλέπει\" πραγματικά μέσω vision API), εναλλάξιμο " +
                "ορατό σκεπτικό (extended thinking) σε αναδιπλούμενο πάνελ πάνω από την απάντηση, νέο ξεχωριστό " +
                "κουμπί-γρανάζι για τις γενικές ρυθμίσεις εφαρμογής (Agent/Project/Πληροφορίες) ενώ το κουμπί-παλέτα " +
                "άνοιγε πλέον αποκλειστικό, μικρότερο παράθυρο μόνο για Εμφάνιση/Θέματα/Skins.\n" +
                "v1.5.0 — Οι Ρυθμίσεις Εμφάνισης έγιναν ξεχωριστό παράθυρο με καρτέλες (Agent/Εμφάνιση/Project/Πληροφορίες) αντί " +
                "για αναδυόμενο μενού που έκλεινε μόνο του όταν άλλαζε το ενεργό παράθυρο· διόρθωση bug όπου τα logos εργαλείων " +
                "εμφανίζονταν ως απλό γράμμα (αντί για το πραγματικό εικονίδιο) μετά από αλλαγή θέματος Windows.\n" +
                "v1.4.0 — Επιλέξιμα οπτικά \"θέματα\" εμπνευσμένα από Windows Vista/7/8/10-11 (γωνίες, σκιάσεις, glossy " +
                "κουμπιά, Metro πλακίδια στο Windows 8) στο μενού Εμφάνιση.\n" +
                "v1.3.0 — Πραγματική διαφάνεια σε επίπεδο παραθύρου (όχι πια σε πλαίσια/κουμπιά) με custom title bar, " +
                "splash οθόνη εκκίνησης που μαζεύεται στη θέση του τίτλου, custom vector εικονίδια στις καρτέλες " +
                "κατηγοριών (αντί για emoji), επιλογή κεντρικού AI agent (πέραν του Claude) με πραγματική σύνδεση " +
                "chat για ChatGPT/Mistral/DeepSeek/Grok/Perplexity/Gemini, ρυθμιζόμενα επίπεδα διαφάνειας & επιλογές " +
                "κινούμενου φόντου, πλήρεις μεταφράσεις καταλόγου εργαλείων σε Français/Deutsch.\n" +
                "v1.2.0 — Ομαδοποιημένο μενού ρυθμίσεων (Agent, Εμφάνιση, Project, Πληροφορίες), εικονίδιο υδρόγειου για " +
                "επιλογή γλώσσας (πιο διεθνές από σημαία), σωστή σημαία Ηνωμένου Βασιλείου, λευκή \"πλάκα\" πίσω από κάθε " +
                "logo για αντίθεση σε dark mode, οπτικό στυλ Aero (διαφάνεια/σκίαση/κινούμενο φόντο), εισαγωγή ολόκληρου " +
                "φακέλου ως project, εξαγωγή συζήτησης σε αρχείο, πραγματικό usage tracking για ElevenLabs, persist " +
                "ρυθμίσεων μεταξύ επανεκκινήσεων.\n" +
                "v1.1.0 — Πολυγλωσσικό UI σε 4 γλώσσες (Ελληνικά/English/Français/Deutsch) με επιλογέα σημαιών, λογότυπα ανά " +
                "εργαλείο (με διόρθωση για Microsoft Copilot), αυτό το πάνελ βοήθειας/ιστορικού, εισαγωγή project από αρχείο, " +
                "ρύθμιση θέσης πλευρικής μπάρας, κινούμενο εικονίδιο & γραμματοσειρά τίτλου, γενικές διορθώσεις " +
                "αντίθεσης/rendering.\n" +
                "v1.0.0 — Πρώτη πλήρης έκδοση: build/run επιβεβαιωμένο σε Windows, εικονίδιο εφαρμογής, tabs κατηγοριών με πραγματική " +
                "εναλλαγή περιεχομένου, μενού agents/login στο \"C\" με αποθήκευση κλειδιών ανά εργαλείο.",
            Lang.EN =>
                "v4.8.0 — <b>Sidebar collapse, MotionDeskTop-style</b>: the \"expanded\" state (default) now " +
                "shows an icon+label on every button (Theme/Language/Appearance/Artifacts/Projects/Settings) " +
                "instead of bare icons - the \"collapsed\" state stays the familiar narrow icon-only rail. The " +
                "collapse button now lives inside that same column (below the icons) and automatically follows " +
                "the sidebar whether it's on the left or right, no more manual cross-column position math.\n" +
                "v4.7.0 — Visual & usability pass: <b>vector icons</b> instead of emoji in window chrome, the " +
                "main toolbar, and the sidebar (clean line-icon style) · <b>dark mode in neutral charcoal " +
                "#171717</b> (instead of the previous warm brownish-black) · <b>sidebar collapse</b> with a " +
                "toggle button below the icons, centered with them · <b>📋 Paste</b> buttons on the API " +
                "key/username/password fields (a safe alternative to reading saved browser passwords, which " +
                "was explicitly declined for security reasons) · the auto-continue-with-another-agent " +
                "feature (v4.6.0) got more correct: a Yes/No pop-up the FIRST time, then it remembers the " +
                "choice and continues automatically without asking again · Agent settings were reorganized " +
                "into collapsible sections (less clutter). Live-tested on a real running instance (UI " +
                "Automation, no keys): the Gateway endpoint was confirmed working end-to-end; found that " +
                "Ctrl+Alt+Space (Kombos Anywhere) can silently conflict with another app on some machines " +
                "(known issue, open for a fix).\n" +
                "v4.6.0 — Kombos Gateway (an explicit request after the \"final\" v4.5.0 - an exception, not " +
                "scope creep): a new local HTTP gateway, inspired by the open-source OmniRoute " +
                "(github.com/diegosouzapw/OmniRoute, MIT) - an OpenAI-compatible endpoint " +
                "(<code>http://127.0.0.1:PORT/v1/chat/completions</code>) other tools (VS Code, Cursor, " +
                "Claude Code, etc.) can point at instead of talking to a provider directly. Routes " +
                "automatically to Kombos's central agent, with automatic <b>fallback</b> to the next " +
                "configured agent if the first fails (rate limit/quota/network) - the same spirit as " +
                "OmniRoute's \"automatic fallback across 359 providers,\" but configured entirely through " +
                "Kombos's already-friendly GUI (Settings → Behavior: one checkbox + one port), not .env " +
                "files/a CLI/Docker/a separate dashboard - this is the \"friendlier OmniRoute\" that was " +
                "asked for. Secure by design: listens ONLY on 127.0.0.1, never on the network. No code was " +
                "copied from OmniRoute (different language/architecture - Node.js there, C#/.NET here) - " +
                "only the idea of a single local gateway with automatic fallback was built from scratch, " +
                "using the keys you've already entered into Kombos. Alongside it, new <b>automatic " +
                "continuation when Claude nears/hits its limit</b> - explicit user request: the FIRST time, " +
                "a pop-up proposes the first already-configured agent (Yes/No), and your choice is " +
                "remembered from then on - Kombos automatically draws on that same agent next time, without " +
                "asking again, so the conversation/project keeps going uninterrupted. Settings → Behavior has " +
                "the enable checkbox plus a \"Reset\" button to pick a different agent later. The reply " +
                "bubble always shows which agent actually answered - no silent/hidden switching.\n" +
                "v4.5.0 — FINAL VERSION: Kombos is now complete as a multi-tool - no version is planned " +
                "after this one. A large final wave: <b>🔄 Check for updates</b> (Settings → About, manual " +
                "only, never a silent download/install) · <b>Import from ChatGPT/Claude.ai</b> (their " +
                "conversations.json export files) · <b>Kombos Anywhere</b> (Ctrl+Alt+Space opens a mini " +
                "quick-capture bar from anywhere, with an option to add the answer to the main conversation) " +
                "· <b>Multi-window</b> (detach any conversation into its own window, with \"Copy all as " +
                "Markdown\") · <b>/image</b> (real image generation inside the conversation via the OpenAI " +
                "Images API, using the already-saved ChatGPT key) · <b>Real-time collaboration</b> (two " +
                "computers pointed at the same sync folder see each other's changes automatically, via file " +
                "watching) · <b>Local analytics dashboard</b> (tokens-per-session chart plus a per-tool " +
                "breakdown, no telemetry) · <b>Plugin SDK v2</b> (a \"New Plugin\" wizard scaffolds a ready-" +
                "to-build project) · <b>Nexus Marketplace</b> (install a plugin from a folder or Git URL) · " +
                "<b>Artifact live preview</b> (real HTML/SVG/Markdown rendering in its own window, not just a " +
                "code list) · an accessibility pass (AutomationProperties.Name on key icons/window controls). " +
                "On \"response speed/cost\": the standing cache-friendly system prompt (since v2.8.0) already " +
                "applies to every agent; real automatic routing to a cheaper tool or batching were evaluated " +
                "and deliberately NOT built (it would mean Kombos silently overriding the user's own agent " +
                "choice, or doesn't fit real-time chat) - an honest choice, not an oversight.\n" +
                "v4.3.0 — Local project search + message actions (second wave toward the final v4.5.0): new " +
                "<code>/search &lt;keywords&gt;</code> command - searches the active project's full content " +
                "(not just the file tree like /readfile), locally, with term-frequency scoring, zero token " +
                "cost (a \"local RAG-lite\" without embeddings/vector DB). Right-click on any message now " +
                "offers \"Copy as Markdown\", \"Copy as plain text\", and \"Quote in new message\".\n" +
                "v4.2.0 — Voice + Presets + Focus mode (first wave toward the final v4.5.0): new 🔊 " +
                "\"Read aloud\" button on every agent reply (local, free text-to-speech via System.Speech, " +
                "no cloud/API key), and a new 🎤 voice-input button next to the message field (push-to-talk: " +
                "press, speak once, press again - not continuous listening). New \"Presets\" section in Agent " +
                "settings: save the current agent + instructions combo under a name, apply it again later " +
                "with one click instead of retyping the same instructions. New \"Focus mode\" (F11, or the " +
                "View menu on Windows Classic): hides sidebar/menu bar/header/tool catalog/status bar, " +
                "leaving only the conversation - Esc exits it. v4.5.0 has been set as Kombos's FINAL version " +
                "- everything from here builds toward that point; see the full roadmap for what's left.\n" +
                "v4.1.0 — Real Light/Dark fix + conversation forking + Prompt Library: dark mode visibly " +
                "\"didn't work\" on the Windows Classic skin - the real cause was found (Classic's palette " +
                "completely ignored dark mode) and a real dark variant was added. New \"Theme\" option in " +
                "Appearance with 3 states: Light / Dark / System (automatically and live follows the Windows " +
                "theme). Also fixed the language/Artifacts popup position on Windows Classic (correctly " +
                "anchored but with the wrong Placement, landing outside the window) and the too-narrow title " +
                "field in \"+ New Project\". Research into similar open-source apps (LibreChat, NextChat, " +
                "Cherry Studio) produced two new, original implementations: \"🔀 Fork\" a conversation from any " +
                "message (a new independent conversation with the history up to that point, without altering " +
                "the original), and a \"📚 Prompt Library\" (10 original starter prompts across 5 categories - " +
                "zero token cost until you send one). Search across ALL conversations (title + content) was " +
                "already implemented earlier.\n" +
                "v4.0.0 — Milestone: skin/window fixes + Accounts + catalog pagination. The Windows Classic " +
                "skin had forgotten popups (language, Artifacts, Projects/history) that stayed rounded and " +
                "appeared at the sidebar's now-hidden position - fully fixed, along with a deeper cause: a " +
                "MenuItem in the classic menu bar isn't a safe anchor once its menu closes, so popups were " +
                "silently failing to open. The Nexus skin in dark mode now uses a neutral charcoal/graphite " +
                "palette (Adobe Acrobat Pro dark-mode style) instead of the previous very dark one. " +
                "Maximizing now respects the exact screen boundaries (taskbar/multi-monitor setups) instead " +
                "of overflowing. The tool catalog now pages through tiles with side arrows instead of a " +
                "scrollbar. New \"Accounts\" window (from the tray icon) showing which tools have saved " +
                "details and opening the tool's real login/signup page in your browser - Kombos never logs " +
                "in or signs up on your behalf.\n" +
                "v3.9.0 — Projects (Claude Desktop style) + fixes: new \"PROJECTS\" section above the " +
                "conversation list with a \"+ New Project\" button - group conversations under a name, new " +
                "chats started inside a Project are auto-assigned to it. The attach button now accepts ANY " +
                "file type (not just images) - .md/.txt/.json/code get added as text context to the chat. New " +
                "optional username/password fields per agent in settings (DPAPI-encrypted, for your own " +
                "reference only - never auto-fills any website login). New catalog tool \"OmniRoute\" (a " +
                "third-party local AI gateway, community project - you need to start it yourself). GitHub's " +
                "MCP connector is pre-filled as an example in the MCP servers list (needs OAuth that isn't " +
                "automated yet). Fixes: the \"Orbit\" animated background now shows the orbit path line, and " +
                "the leftmost gear in \"Gears\" no longer overlaps the animated title icon.\n" +
                "v3.8.0 — Command Palette + Business Toolkit + Shared Projects: new Ctrl+K quick-command window " +
                "searching across tools/commands/settings. New Business Toolkit (local plugins, zero token " +
                "cost): /vat, /loan, /percent, /workdays (business days aware of Greek public holidays), /iban, " +
                "/invoice-number. New \"Shared Projects\": publish a Project Memory project as a ZIP inside the " +
                "sync folder - every computer with the same folder sees the list with a status (Pending/In " +
                "progress/Completed) and can download the ZIP (there's no public API for claude.ai's Projects, " +
                "so this is the real alternative). Fixes: the Windows Classic skin now has the correct gray " +
                "background/navy titlebar everywhere, and the Nexus skin's vertical tab labels no longer get cut off.\n" +
                "v3.7.0 — Visual upgrade + Creative Toolkit: two new skins (Windows Classic — square corners " +
                "everywhere, raised 3D-bevel tiles, navy titlebar, and a CLASSIC MENU BAR — File/Edit/View/" +
                "Tools/Help — replacing the icon sidebar entirely; Nexus — original orange branding, vertical " +
                "tab rail in Settings). Every skin now has an authentic period accent color (Aero blue, Metro " +
                "blue, Fluent blue, Classic navy). New Creative Toolkit (local plugins, zero token cost): " +
                "/gradient, /aspect-ratio, /palette, /contrast (WCAG contrast checker), /favicon-checklist.\n" +
                "v3.6.0 — Big \"multi-tool\" update: a new \"Download & Install\" gallery in Settings → " +
                "Extensions for one-click install/uninstall of official plugins (Dev Toolkit with /json /base64 " +
                "/hash /uuid /color /timestamp /regex /git /http /cron, Office Toolkit with /wordcount /lorem " +
                "/password /diff /case, a Weather plugin) - all local, zero token cost. New \"Hive Memory\": " +
                "whatever one agent learns (/remember, /hive) every agent now knows. New Handoff pipeline (🔗 " +
                "button next to the message box): a draft reply from one agent, refined by a second. New " +
                "\"Project Memory\": import a project as a lightweight index (file tree) instead of full " +
                "content, pull in a specific file on demand with /readfile, and run a local compatibility check " +
                "with /verify (runs the project's own build/lint, no AI call). A prompt-cache hit-rate indicator " +
                "in the status bar, the catalog JSON is now built once instead of on every message, and older " +
                "messages are automatically trimmed in very long conversations to save tokens.\n" +
                "v3.0.0 — First complete, \"shipped\" release: an installer (KombosSetup.exe), no admin rights " +
                "needed, with an optional desktop shortcut and start-with-Windows during setup. Full plugin " +
                "system, prompt caching + MCP servers for Claude, safe message delete/conversation duplicate, " +
                "an Artifacts panel, a conversations sync folder across computers, a first-run guide, automated " +
                "tests, and major memory/CPU fixes - everything built since v2.7.0, bundled into one stable, " +
                "tested release.\n" +
                "v2.9.9 — Reduced memory/CPU usage: tool logos are now cached instead of being re-downloaded " +
                "from the web on every theme/language/search change, the tool search box no longer rebuilds " +
                "the whole catalog on every keystroke, and animated backgrounds now fully stop while Kombos " +
                "is running hidden in the system tray instead of continuing indefinitely in the background.\n" +
                "v2.9.5 — Clear split of settings entry points: the gear opens general app settings " +
                "(Behavior), the \"C\" avatar opens the central agent's own personalized settings and " +
                "collaboration with other agents (MCP) - still the same two, and only two, entry points into " +
                "settings; a new \"Conversations\" sidebar icon for quick access to current and older chats.\n" +
                "v2.9.0 — A first-run onboarding guide with tutorial pages, safe deletion of a single message, " +
                "duplicate a conversation, an \"Artifacts\" panel in the sidebar listing every code block from " +
                "every conversation (copy/save), a conversations sync folder (OneDrive/Dropbox/network) for " +
                "sharing across computers, and a fix for a bug where \"Regenerate\" on an error bubble could " +
                "erase real history.\n" +
                "v2.8.0 — Prompt caching for Claude (cheaper turns whenever the system prompt/catalog doesn't " +
                "change), remote MCP server support in Settings → Agent (Claude only), and a minimum-token-cost " +
                "rule applied to every agent without exception (documented in code per provider).\n" +
                "v2.7.0 — Plugin system: a new \"Plugins\" tab in Settings, an %AppData%\\Kombos\\plugins\\ " +
                "folder for third-party extensions, \"/something\" chat commands registered by plugins " +
                "(sample included: /weather <city>).\n" +
                "v2.6.0 — A tool search box next to the category tabs - searches across all categories at once.\n" +
                "v2.5.0 — Pin favorite tools (⭐ on every tile and in the details panel - pinned ones show first " +
                "within their category), a new \"Behavior\" tab in Settings (always on top, start with Windows, " +
                "start minimized, notification sound), export/import a full JSON backup of your conversations, " +
                "keyboard shortcuts (Ctrl+N new chat, Ctrl+, settings, Esc to close), and a \"jump to bottom\" " +
                "button when you scroll up to older messages.\n" +
                "v2.4.0 — A system-tray notification when a reply arrives while the window isn't in the " +
                "foreground, searching saved conversations now also matches their content (not just the " +
                "title), an unsent draft is kept when you switch conversations, adjustable chat text size " +
                "(Small/Normal/Large, in Appearance) for accessibility, and a \"delete all saved conversations\" " +
                "button (with confirmation) in Settings.\n" +
                "v2.3.0 — API keys are now encrypted with Windows DPAPI (tied to your Windows account) before " +
                "being written to disk, instead of plain text.\n" +
                "v2.2.0 — Markdown rendering in replies (bold/italic/inline code, code blocks with their own " +
                "copy button), edit your own message before resending it (✏ on every user bubble), search " +
                "within saved conversations, and several stability fixes: \"Regenerate\" now correctly replaces " +
                "the old reply instead of adding a second one below it and no longer erases real history on " +
                "failure; API keys now persist across restarts; fixed a possible conversation mix-up if a " +
                "delayed reply arrived after switching to another conversation.\n" +
                "v2.1.0 — Rename saved conversations (✏ in the history list), custom instructions for the central " +
                "agent (Settings → Agent), bigger and more distinguishable buttons next to the message box, Help " +
                "and Version history fully split into separate, bold-headed sections.\n" +
                "v2.0.0 — Regenerate a reply, compare answers from multiple agents at once (⚖ next to the message " +
                "box), multiple saved conversations with their own history (🕘) instead of one continuous chat, a " +
                "system tray icon so Kombos keeps running in the background when the window is closed, and a " +
                "global hotkey (Ctrl+Alt+K) to summon it from anywhere in Windows.\n" +
                "v1.9.0 — Real web search (toggleable, 🌐 next to the message box) via Claude's official server-side " +
                "web search tool — it decides on its own when it needs to search, and the sources it found show up " +
                "as clickable links under the reply.\n" +
                "v1.8.0 — Paste an image from the clipboard with Ctrl+V (e.g. a screenshot) straight into the chat, " +
                "drag & drop an image/file/folder onto the chat panel (auto-detects whether it's an image " +
                "attachment or context to import), boosted contrast on every animated background so they read " +
                "clearly in any theme/skin.\n" +
                "v1.7.0 — 10 selectable animated header backgrounds (Gears/Orbit/Waves/Grid Pulse inspired by " +
                "Windows Optimizer, plus Node Network/Bokeh/Aurora/Starfield), a \"New chat\" button, a copy button " +
                "on every reply, the ability to stop a reply in progress, a multi-line input box (Shift+Enter now " +
                "actually inserts a newline), a fixed palette icon in the Appearance window.\n" +
                "v1.6.0 — Image attachments in chat (Claude actually \"sees\" them via the vision API), a toggleable " +
                "visible extended-thinking panel above replies, a new dedicated gear button for general app settings " +
                "(Agent/Project/Info) while the palette button now opens its own smaller window just for " +
                "Appearance/Themes/Skins.\n" +
                "v1.5.0 — Appearance Settings is now a separate tabbed window (Agent/Appearance/Project/Info) instead of a " +
                "popup menu that closed itself whenever another window got focus; fixed a bug where tool logos would show a " +
                "plain letter instead of the real icon after switching the Windows theme.\n" +
                "v1.4.0 — Selectable visual \"themes\" inspired by Windows Vista/7/8/10-11 (corners, shadows, glossy " +
                "buttons, Windows 8 Metro tiles) in the Appearance menu.\n" +
                "v1.3.0 — Real window-level transparency (no longer on panels/buttons) with a custom title bar, a startup " +
                "splash that shrinks into the title's position, custom vector icons on category tabs (instead of emoji), " +
                "a central AI agent picker (beyond Claude) with real chat for ChatGPT/Mistral/DeepSeek/Grok/" +
                "Perplexity/Gemini, adjustable transparency levels & animated background options, full catalog " +
                "translations in French/German.\n" +
                "v1.2.0 — Grouped settings menu (Agent, Appearance, Project, Info), a globe icon for language selection " +
                "(more universal than a flag), a corrected UK flag, a white backing plate behind every logo for contrast " +
                "in dark mode, an Aero visual style (transparency/shadow/animated background), whole-folder project " +
                "import, chat export to file, real usage tracking for ElevenLabs, settings persisted across restarts.\n" +
                "v1.1.0 — Multilingual UI in 4 languages (Greek/English/French/German) with a flag picker, per-tool logos " +
                "(with a fix for Microsoft Copilot), this help/version panel, project import from file, sidebar position " +
                "setting, animated title icon & title font, general contrast/rendering fixes.\n" +
                "v1.0.0 — First complete release: confirmed build/run on Windows, app icon, category tabs with real content " +
                "switching, agent/login menu on \"C\" with per-tool key storage.",
            Lang.FR =>
                "v4.6.0 — Passerelle Kombos (demande explicite après la v4.5.0 « finale » - une exception, " +
                "pas un dérapage de périmètre) : une nouvelle passerelle HTTP locale, inspirée du projet " +
                "open-source OmniRoute (github.com/diegosouzapw/OmniRoute, MIT) - un endpoint compatible " +
                "OpenAI (<code>http://127.0.0.1:PORT/v1/chat/completions</code>) que d'autres outils (VS " +
                "Code, Cursor, Claude Code, etc.) peuvent utiliser au lieu de parler directement à un " +
                "fournisseur. Route automatiquement vers l'agent central de Kombos, avec <b>repli " +
                "automatique</b> vers l'agent configuré suivant en cas d'échec (limite de débit/quota/" +
                "réseau) - le même esprit que le « repli automatique sur 359 fournisseurs » d'OmniRoute, " +
                "mais configuré entièrement via l'interface déjà conviviale de Kombos (Réglages → " +
                "Comportement : une case à cocher + un port), sans fichiers .env/CLI/Docker/tableau de bord " +
                "séparé - c'est l'« OmniRoute plus convivial » qui était demandé. Sécurisé par conception : " +
                "n'écoute QUE sur 127.0.0.1, jamais sur le réseau. Aucun code copié d'OmniRoute (langage/" +
                "architecture différents - Node.js là-bas, C#/.NET ici) - seule l'idée d'une passerelle " +
                "locale unique avec repli automatique a été construite de zéro, utilisant les clés déjà " +
                "saisies dans Kombos.\n" +
                "v4.5.0 — VERSION FINALE : Kombos est désormais complet en tant que multi-outil - aucune " +
                "version n'est prévue après celle-ci. Une grande vague finale : <b>🔄 Vérifier les mises à " +
                "jour</b> (Réglages → À propos, uniquement manuel, jamais de téléchargement/installation " +
                "silencieux) · <b>Importer depuis ChatGPT/Claude.ai</b> (leurs fichiers d'export " +
                "conversations.json) · <b>Kombos Anywhere</b> (Ctrl+Alt+Espace ouvre une mini-barre de " +
                "capture rapide depuis n'importe où, avec option d'ajouter la réponse à la conversation " +
                "principale) · <b>Multi-fenêtres</b> (détacher n'importe quelle conversation dans sa propre " +
                "fenêtre, avec « Copier tout en Markdown ») · <b>/image</b> (génération réelle d'image dans " +
                "la conversation via l'API OpenAI Images, avec la clé ChatGPT déjà enregistrée) · " +
                "<b>Collaboration en temps réel</b> (deux ordinateurs pointant vers le même dossier de " +
                "synchronisation voient automatiquement les changements de l'autre, via surveillance de " +
                "fichiers) · <b>Tableau de bord analytique local</b> (graphique de tokens par session plus " +
                "une répartition par outil, aucune télémétrie) · <b>Plugin SDK v2</b> (un assistant « Nouveau " +
                "plugin » génère un projet prêt à compiler) · <b>Nexus Marketplace</b> (installer un plugin " +
                "depuis un dossier ou une URL Git) · <b>Aperçu en direct des artefacts</b> (vrai rendu " +
                "HTML/SVG/Markdown dans sa propre fenêtre, pas juste une liste de code) · un passage " +
                "d'accessibilité (AutomationProperties.Name sur les icônes clés/contrôles de fenêtre). Sur " +
                "« vitesse/coût de réponse » : le system prompt cache-friendly permanent (depuis v2.8.0) " +
                "s'applique déjà à chaque agent ; un vrai routage automatique vers un outil moins cher ou du " +
                "batching ont été évalués et délibérément NON construits (cela signifierait que Kombos " +
                "outrepasse silencieusement le choix d'agent de l'utilisateur, ou ne convient pas au chat en " +
                "temps réel) - un choix honnête, pas un oubli.\n" +
                "v4.3.0 — Recherche locale de projet + actions de message (deuxième vague vers la v4.5.0 " +
                "finale) : nouvelle commande <code>/search &lt;mots-clés&gt;</code> - recherche dans tout le " +
                "contenu du projet actif (pas seulement l'arborescence comme /readfile), localement, avec un " +
                "score de fréquence de terme, coût en tokens nul (un « local RAG-lite » sans embeddings/base " +
                "vectorielle). Le clic droit sur un message propose désormais « Copier en Markdown », « " +
                "Copier en texte brut » et « Citer dans un nouveau message ».\n" +
                "v4.2.0 — Voix + Presets + Mode focus (première vague vers la v4.5.0 finale) : nouveau " +
                "bouton 🔊 « Lire à voix haute » sur chaque réponse d'agent (synthèse vocale locale et " +
                "gratuite via System.Speech, sans cloud/clé API), et nouveau bouton 🎤 de saisie vocale à " +
                "côté du champ de message (push-to-talk : appuie, parle une fois, réappuie - pas d'écoute " +
                "continue). Nouvelle section « Presets » dans les réglages Agent : enregistre la combinaison " +
                "agent + instructions actuelle sous un nom, réapplique-la plus tard en un clic au lieu de " +
                "retaper les mêmes instructions. Nouveau « Mode focus » (F11, ou le menu Affichage sous " +
                "Windows Classic) : masque barre latérale/menu/en-tête/catalogue d'outils/barre d'état, ne " +
                "laissant que la conversation - Échap en sort. La v4.5.0 a été fixée comme version FINALE " +
                "de Kombos - tout ce qui suit construit vers ce point ; voir le roadmap complet pour le reste.\n" +
                "v4.1.0 — Vraie correction Clair/Sombre + dérivation de conversation + Bibliothèque de " +
                "prompts : le mode sombre \"ne fonctionnait\" visiblement pas sur le skin Windows Classic - la " +
                "vraie cause a été trouvée (la palette de Classic ignorait totalement le mode sombre) et une " +
                "vraie variante sombre a été ajoutée. Nouvelle option \"Thème\" dans Apparence avec 3 états : " +
                "Clair / Sombre / Système (suit automatiquement et en direct le thème de Windows). Correction " +
                "aussi de la position des popups langue/Artifacts sous Windows Classic (correctement ancrées " +
                "mais avec le mauvais Placement, sortant de la fenêtre) et du champ de titre trop étroit dans " +
                "« + Nouveau Project ». Une recherche sur des applications open-source similaires (LibreChat, " +
                "NextChat, Cherry Studio) a donné deux nouvelles implémentations originales : « 🔀 Dériver » " +
                "une conversation depuis n'importe quel message (nouvelle conversation indépendante avec " +
                "l'historique jusqu'à ce point, sans altérer l'originale), et une « 📚 Bibliothèque de prompts » " +
                "(10 prompts de départ originaux, en 5 catégories - coût nul en tokens jusqu'à l'envoi). La " +
                "recherche dans TOUTES les conversations (titre + contenu) était déjà implémentée.\n" +
                "v4.0.0 — Étape majeure : corrections skins/fenêtre + Comptes + pagination du catalogue. Le " +
                "skin Windows Classic avait des popups oubliées (langue, Artifacts, Projets/historique) qui " +
                "restaient arrondies et apparaissaient à la position (désormais cachée) de la barre latérale " +
                "- entièrement corrigé, avec une cause plus profonde : un MenuItem de la barre de menu " +
                "classique n'est pas un ancrage sûr une fois son menu fermé, donc les popups échouaient " +
                "silencieusement à s'ouvrir. Le skin Nexus en mode sombre utilise désormais une palette " +
                "anthracite/graphite neutre (style Adobe Acrobat Pro en mode sombre) au lieu de la précédente, " +
                "très sombre. La maximisation respecte désormais exactement les limites de l'écran (barre des " +
                "tâches/configurations multi-écrans) au lieu de déborder. Le catalogue d'outils se feuillette " +
                "désormais avec des flèches latérales au lieu d'une barre de défilement. Nouvelle fenêtre " +
                "« Comptes » (depuis l'icône de la barre des tâches) montrant quels outils ont des identifiants " +
                "enregistrés et ouvrant la vraie page de connexion/inscription dans ton navigateur - Kombos ne " +
                "se connecte ni ne s'inscrit jamais à ta place.\n" +
                "v3.9.0 — Projets (style Claude Desktop) + corrections : nouvelle section « PROJECTS » " +
                "au-dessus de la liste des conversations avec un bouton « + Nouveau projet » - regroupe les " +
                "conversations sous un nom, les nouvelles discussions démarrées dans un projet lui sont " +
                "automatiquement assignées. Le bouton de pièce jointe accepte désormais TOUS les types de " +
                "fichiers (pas seulement les images) - .md/.txt/.json/code sont ajoutés comme contexte texte " +
                "dans la conversation. Nouveaux champs optionnels nom d'utilisateur/mot de passe par agent " +
                "dans les paramètres (chiffrés avec DPAPI, uniquement pour ta propre référence - ne remplit " +
                "jamais automatiquement un formulaire de connexion). Nouvel outil du catalogue « OmniRoute » " +
                "(passerelle IA locale tierce, projet communautaire - à démarrer toi-même). Le connecteur MCP " +
                "GitHub est pré-rempli à titre d'exemple dans la liste des serveurs MCP (nécessite une " +
                "authentification OAuth pas encore automatisée). Corrections : le fond animé « Orbites » " +
                "affiche désormais la ligne de la trajectoire, et l'engrenage le plus à gauche dans « Engrenages » " +
                "ne chevauche plus l'icône animée du titre.\n" +
                "v3.8.0 — Palette de commandes + boîte à outils entreprise + projets partagés : nouvelle " +
                "fenêtre de commande rapide Ctrl+K recherchant dans les outils/commandes/paramètres. Nouvelle " +
                "boîte à outils entreprise (extensions locales, coût en tokens nul) : /vat, /loan, /percent, " +
                "/workdays (jours ouvrés tenant compte des jours fériés grecs), /iban, /invoice-number. " +
                "Nouveaux « Projets partagés » : publie un projet Project Memory sous forme de ZIP dans le " +
                "dossier de synchronisation - chaque ordinateur avec le même dossier voit la liste avec un " +
                "statut (En attente/En cours/Terminé) et peut télécharger le ZIP (il n'existe pas d'API " +
                "publique pour les Projects de claude.ai, donc voici la véritable alternative). Corrections : " +
                "le skin Windows Classic a désormais le bon fond gris/barre de titre navy partout, et les " +
                "étiquettes du rail vertical du skin Nexus ne sont plus tronquées.\n" +
                "v3.7.0 — Mise à niveau visuelle + boîte à outils créative : deux nouveaux skins (Windows " +
                "Classic — coins carrés partout, tuiles en relief 3D, barre de titre navy, et une BARRE DE " +
                "MENU CLASSIQUE — Fichier/Édition/Affichage/Outils/Aide — remplaçant entièrement la barre " +
                "latérale d'icônes ; Nexus — branding orange original, rail d'onglets vertical dans les " +
                "paramètres). Chaque skin a désormais une couleur d'accent authentique de son époque (bleu " +
                "Aero, bleu Metro, bleu Fluent, navy Classic). Nouvelle boîte à outils créative (extensions " +
                "locales, coût en tokens nul) : /gradient, /aspect-ratio, /palette, /contrast (vérificateur de " +
                "contraste WCAG), /favicon-checklist.\n" +
                "v3.6.0 — Grande mise à jour « multi-outils » : nouvelle galerie « Télécharger et installer » " +
                "dans Paramètres → Extensions pour installer/désinstaller en un clic les extensions officielles " +
                "(boîte à outils développeur avec /json /base64 /hash /uuid /color /timestamp /regex /git /http " +
                "/cron, boîte à outils bureautique avec /wordcount /lorem /password /diff /case, une extension " +
                "météo) - tout en local, coût en tokens nul. Nouvelle « mémoire collective » (Hive Memory) : ce " +
                "qu'apprend un agent (/remember, /hive) est désormais connu de tous les agents. Nouveau pipeline " +
                "Handoff (bouton 🔗 à côté du champ de message) : une réponse initiale d'un agent, améliorée par " +
                "un second. Nouvelle « mémoire de projet » : importer un projet sous forme d'index léger " +
                "(arborescence de fichiers) au lieu du contenu complet, récupérer un fichier précis à la demande " +
                "avec /readfile, et lancer une vérification locale de compatibilité avec /verify (exécute le " +
                "build/lint du projet, sans appel IA). Un indicateur de taux de succès du cache dans la barre " +
                "d'état, le JSON du catalogue est désormais construit une seule fois au lieu de chaque message, " +
                "et les anciens messages sont automatiquement tronqués dans les très longues conversations pour " +
                "économiser des tokens.\n" +
                "v3.0.0 — Première version complète et « livrée » : un installateur (KombosSetup.exe), sans " +
                "droits administrateur, avec raccourci bureau optionnel et démarrage avec Windows pendant " +
                "l'installation. Système d'extensions complet, mise en cache des prompts + serveurs MCP pour " +
                "Claude, suppression de message et duplication de conversation sécurisées, un panneau Artifacts, " +
                "un dossier de synchronisation des conversations entre ordinateurs, un guide de première " +
                "utilisation, des tests automatisés, et d'importantes corrections de consommation mémoire/CPU - " +
                "tout ce qui a été construit depuis la v2.7.0, réuni en une version stable et testée.\n" +
                "v2.9.9 — Consommation mémoire/CPU réduite : les logos des outils sont désormais mis en cache " +
                "au lieu d'être retéléchargés à chaque changement de thème/langue/recherche, la recherche " +
                "d'outils ne reconstruit plus tout le catalogue à chaque frappe, et les fonds animés s'arrêtent " +
                "complètement pendant que Kombos tourne caché dans la zone de notification au lieu de " +
                "continuer indéfiniment en arrière-plan.\n" +
                "v2.9.5 — Séparation claire des points d'entrée des paramètres : l'engrenage ouvre les " +
                "paramètres généraux de l'application (Comportement), l'avatar « C » ouvre les paramètres " +
                "personnalisés de l'agent central et la collaboration avec les autres agents (MCP) - toujours " +
                "les deux mêmes, et seuls, points d'entrée vers les paramètres ; une nouvelle icône " +
                "« Conversations » dans la barre latérale pour un accès rapide aux discussions actuelles et " +
                "anciennes.\n" +
                "v2.9.0 — Un guide de démarrage avec des pages tutoriel, suppression sûre d'un seul message, " +
                "dupliquer une conversation, un panneau « Artifacts » dans la barre latérale listant tous les " +
                "blocs de code de chaque conversation (copier/enregistrer), un dossier de synchronisation des " +
                "conversations (OneDrive/Dropbox/réseau) pour partager entre ordinateurs, et un correctif pour " +
                "un bug où « Régénérer » sur une bulle d'erreur pouvait effacer un historique réel.\n" +
                "v2.8.0 — Prompt caching pour Claude (échanges moins chers tant que le prompt système/catalogue " +
                "ne change pas), prise en charge des serveurs MCP distants dans Paramètres → Agent (Claude " +
                "uniquement), et une règle de coût minimal en tokens appliquée à tous les agents sans " +
                "exception (documentée dans le code par fournisseur).\n" +
                "v2.7.0 — Système d'extensions : un nouvel onglet « Extensions » dans les Paramètres, un " +
                "dossier %AppData%\\Kombos\\plugins\\ pour les extensions tierces, des commandes « /truc » " +
                "dans le chat enregistrées par les extensions (exemple fourni : /weather <ville>).\n" +
                "v2.6.0 — Une recherche d'outils à côté des onglets de catégories - cherche dans toutes les " +
                "catégories à la fois.\n" +
                "v2.5.0 — Épingler tes outils favoris (⭐ sur chaque carte et dans le panneau de détails - les " +
                "épinglés s'affichent en premier dans leur catégorie), un nouvel onglet « Comportement » dans " +
                "les Paramètres (toujours au premier plan, démarrer avec Windows, démarrer réduit, son de " +
                "notification), export/import d'une sauvegarde JSON complète de tes conversations, raccourcis " +
                "clavier (Ctrl+N nouvelle conversation, Ctrl+, paramètres, Échap pour fermer), et un bouton " +
                "« aller en bas » quand tu remontes vers d'anciens messages.\n" +
                "v2.4.0 — Notification dans la zone de notification quand une réponse arrive alors que la " +
                "fenêtre n'est pas au premier plan, la recherche dans les conversations enregistrées porte " +
                "maintenant aussi sur le contenu (pas seulement le titre), un brouillon non envoyé est conservé " +
                "quand tu changes de conversation, taille du texte du chat réglable (Petit/Normal/Grand, dans " +
                "Apparence) pour l'accessibilité, et un bouton « supprimer toutes les conversations enregistrées » " +
                "(avec confirmation) dans les Paramètres.\n" +
                "v2.3.0 — Les clés API sont désormais chiffrées avec le DPAPI de Windows (lié à ton compte " +
                "Windows) avant d'être écrites sur le disque, au lieu du texte brut.\n" +
                "v2.2.0 — Rendu markdown dans les réponses (gras/italique/code en ligne, blocs de code avec leur " +
                "propre bouton copier), modifier ton propre message avant de le renvoyer (✏ sur chaque bulle " +
                "utilisateur), recherche dans les conversations enregistrées, et plusieurs correctifs de " +
                "stabilité : « Régénérer » remplace désormais correctement l'ancienne réponse au lieu d'en " +
                "ajouter une seconde en dessous et n'efface plus l'historique réel en cas d'échec ; les clés API " +
                "sont désormais conservées entre les redémarrages ; correction d'un possible mélange de " +
                "conversations si une réponse tardive arrivait après un changement de conversation.\n" +
                "v2.1.0 — Renommer les conversations enregistrées (✏ dans la liste d'historique), instructions " +
                "personnalisées pour l'agent central (Paramètres → Agent), boutons plus grands et plus visibles à " +
                "côté du champ de message, Aide et Historique des versions complètement séparés en sections " +
                "distinctes à en-tête en gras.\n" +
                "v2.0.0 — Régénérer une réponse, comparer les réponses de plusieurs agents à la fois (⚖ à côté du " +
                "champ de message), plusieurs conversations enregistrées avec leur propre historique (🕘) au lieu " +
                "d'un seul chat continu, une icône dans la zone de notification pour que Kombos continue de " +
                "fonctionner en arrière-plan à la fermeture de la fenêtre, et un raccourci global (Ctrl+Alt+K) " +
                "pour l'ouvrir depuis n'importe où sous Windows.\n" +
                "v1.9.0 — Vraie recherche web (activable, 🌐 à côté du champ de message) via l'outil officiel de " +
                "recherche web côté serveur de Claude — il décide lui-même quand chercher, et les sources trouvées " +
                "s'affichent en liens cliquables sous la réponse.\n" +
                "v1.8.0 — Coller une image du presse-papiers avec Ctrl+V (ex. capture d'écran) directement dans le " +
                "chat, glisser-déposer une image/un fichier/un dossier sur le panneau de discussion (détection " +
                "automatique : pièce jointe image ou contexte à importer), meilleur contraste sur tous les fonds " +
                "animés pour qu'ils restent lisibles sur tous les thèmes/skins.\n" +
                "v1.7.0 — 10 fonds animés sélectionnables pour l'en-tête (Engrenages/Orbite/Vagues/Grille pulsée " +
                "inspirés de Windows Optimizer, plus Réseau de nœuds/Bokeh/Aurore/Ciel étoilé), un bouton \"Nouvelle " +
                "conversation\", un bouton copier sur chaque réponse, la possibilité d'arrêter une réponse en cours, " +
                "un champ de saisie multiligne (Maj+Entrée insère désormais une vraie nouvelle ligne), correction de " +
                "l'icône palette dans la fenêtre Apparence.\n" +
                "v1.6.0 — Pièces jointes image dans le chat (Claude les \"voit\" réellement via l'API vision), un " +
                "panneau de raisonnement visible (extended thinking) activable au-dessus des réponses, un nouveau " +
                "bouton engrenage dédié aux réglages généraux (Agent/Projet/Infos) tandis que le bouton palette " +
                "ouvre désormais sa propre fenêtre plus petite, uniquement pour Apparence/Thèmes/Skins.\n" +
                "v1.5.0 — Les Paramètres d'apparence sont maintenant une fenêtre séparée à onglets (Agent/Apparence/Projet/Infos) " +
                "au lieu d'un menu popup qui se fermait tout seul dès qu'une autre fenêtre prenait le focus ; correction d'un bug " +
                "où les logos des outils affichaient une simple lettre au lieu de la vraie icône après un changement de thème Windows.\n" +
                "v1.4.0 — \"Thèmes\" visuels sélectionnables inspirés de Windows Vista/7/8/10-11 (coins, ombres, boutons " +
                "brillants, tuiles Metro sous Windows 8) dans le menu Apparence.\n" +
                "v1.3.0 — Transparence réelle au niveau de la fenêtre (plus sur les panneaux/boutons) avec une barre de " +
                "titre personnalisée, un écran de démarrage qui se réduit vers la position du titre, des icônes " +
                "vectorielles personnalisées sur les onglets de catégorie (au lieu d'émojis), un sélecteur d'agent IA " +
                "central (au-delà de Claude) avec un vrai chat pour ChatGPT/Mistral/DeepSeek/Grok/Perplexity/Gemini, " +
                "niveaux de transparence réglables & options de fond animé, traductions complètes du catalogue en " +
                "français/allemand.\n" +
                "v1.2.0 — Menu de paramètres regroupé (Agent, Apparence, Projet, Infos), icône de globe pour choisir la " +
                "langue (plus universelle qu'un drapeau), drapeau du Royaume-Uni corrigé, plaque blanche derrière chaque " +
                "logo pour le contraste en mode sombre, style visuel Aero (transparence/ombre/fond animé), importation " +
                "d'un dossier entier comme projet, export du chat en fichier, suivi d'usage réel pour ElevenLabs, " +
                "paramètres conservés entre les redémarrages.\n" +
                "v1.1.0 — Interface multilingue en 4 langues (grec/anglais/français/allemand) avec sélecteur de drapeaux, " +
                "logos par outil (avec correction pour Microsoft Copilot), ce panneau d'aide/historique, importation de " +
                "projet depuis un fichier, réglage de la position de la barre latérale, icône de titre animée & police du " +
                "titre, corrections générales de contraste/rendu.\n" +
                "v1.0.0 — Première version complète : build/exécution confirmés sous Windows, icône de l'application, onglets de " +
                "catégorie avec changement de contenu réel, menu agents/connexion sur « C » avec clés par outil.",
            _ =>
                "v4.6.0 — Kombos-Gateway (eine ausdrückliche Anfrage nach der \"finalen\" v4.5.0 - eine " +
                "Ausnahme, kein Scope Creep): ein neues lokales HTTP-Gateway, inspiriert vom Open-Source-" +
                "Projekt OmniRoute (github.com/diegosouzapw/OmniRoute, MIT) - ein OpenAI-kompatibler " +
                "Endpunkt (<code>http://127.0.0.1:PORT/v1/chat/completions</code>), an den andere Tools " +
                "(VS Code, Cursor, Claude Code usw.) sich wenden können, statt direkt mit einem Anbieter zu " +
                "sprechen. Leitet automatisch an Kombos' zentralen Agent weiter, mit automatischem " +
                "<b>Fallback</b> auf den nächsten konfigurierten Agent bei Fehlschlag (Rate-Limit/Kontingent/" +
                "Netzwerk) - im selben Geist wie OmniRoutes \"automatischer Fallback über 359 Anbieter\", " +
                "aber komplett über Kombos' bereits freundliche Oberfläche konfiguriert (Einstellungen → " +
                "Verhalten: eine Checkbox + ein Port), keine .env-Dateien/CLI/Docker/separates Dashboard - " +
                "das ist das \"freundlichere OmniRoute\", das gewünscht wurde. Sicher per Design: lauscht NUR " +
                "auf 127.0.0.1, nie im Netzwerk. Kein Code von OmniRoute übernommen (andere Sprache/" +
                "Architektur - dort Node.js, hier C#/.NET) - nur die Idee eines einzigen lokalen Gateways mit " +
                "automatischem Fallback wurde von Grund auf gebaut, unter Verwendung der bereits in Kombos " +
                "eingegebenen Schlüssel.\n" +
                "v4.5.0 — FINALE VERSION: Kombos ist jetzt als Multi-Tool vollständig - danach ist keine " +
                "weitere Version geplant. Eine große letzte Welle: <b>🔄 Nach Updates suchen</b> " +
                "(Einstellungen → Info, nur manuell, nie stilles Herunterladen/Installieren) · <b>Import von " +
                "ChatGPT/Claude.ai</b> (deren conversations.json-Exportdateien) · <b>Kombos Anywhere</b> " +
                "(Strg+Alt+Leertaste öffnet von überall eine Mini-Schnellerfassungsleiste, mit Option, die " +
                "Antwort zur Hauptunterhaltung hinzuzufügen) · <b>Mehrfenster</b> (jede Unterhaltung in ein " +
                "eigenes Fenster lösen, mit \"Alles als Markdown kopieren\") · <b>/image</b> (echte " +
                "Bilderzeugung in der Unterhaltung über die OpenAI Images API, mit dem bereits gespeicherten " +
                "ChatGPT-Schlüssel) · <b>Echtzeit-Zusammenarbeit</b> (zwei Computer mit demselben " +
                "Sync-Ordner sehen die Änderungen des anderen automatisch, via Dateiüberwachung) · " +
                "<b>Lokales Analytics-Dashboard</b> (Token-Diagramm pro Sitzung plus Aufschlüsselung pro " +
                "Tool, keine Telemetrie) · <b>Plugin SDK v2</b> (ein \"Neues Plugin\"-Assistent erzeugt ein " +
                "build-fertiges Projekt) · <b>Nexus Marketplace</b> (Plugin aus einem Ordner oder einer " +
                "Git-URL installieren) · <b>Artifact-Live-Vorschau</b> (echtes HTML/SVG/Markdown-Rendering " +
                "im eigenen Fenster, nicht nur eine Codeliste) · ein Zugänglichkeits-Durchgang " +
                "(AutomationProperties.Name auf wichtigen Symbolen/Fenstersteuerelementen). Zu " +
                "\"Antwortgeschwindigkeit/-kosten\": der ständige cache-freundliche System-Prompt (seit " +
                "v2.8.0) gilt bereits für jeden Agent; echtes automatisches Routing zu einem günstigeren " +
                "Tool oder Batching wurden geprüft und bewusst NICHT gebaut (das würde bedeuten, dass Kombos " +
                "die eigene Agent-Wahl des Nutzers stillschweigend übergeht, oder es passt nicht zu " +
                "Echtzeit-Chat) - eine ehrliche Entscheidung, kein Versehen.\n" +
                "v4.3.0 — Lokale Projektsuche + Nachrichtenaktionen (zweite Welle zur finalen v4.5.0): neuer " +
                "Befehl <code>/search &lt;Stichwörter&gt;</code> - durchsucht den gesamten Inhalt des aktiven " +
                "Projekts (nicht nur den Dateibaum wie /readfile), lokal, mit Termhäufigkeits-Bewertung, " +
                "keine Tokenkosten (ein \"lokales RAG-lite\" ohne Embeddings/Vektordatenbank). Rechtsklick auf " +
                "eine Nachricht bietet jetzt \"Als Markdown kopieren\", \"Als reinen Text kopieren\" und \"In " +
                "neuer Nachricht zitieren\".\n" +
                "v4.2.0 — Sprache + Presets + Fokusmodus (erste Welle zur finalen v4.5.0): neuer 🔊 " +
                "\"Vorlesen\"-Button bei jeder Agent-Antwort (lokale, kostenlose Sprachausgabe via " +
                "System.Speech, keine Cloud/kein API-Key), und ein neuer 🎤 Spracheingabe-Button neben dem " +
                "Nachrichtenfeld (push-to-talk: drücken, einmal sprechen, erneut drücken - kein " +
                "Dauerzuhören). Neuer Bereich \"Presets\" in den Agent-Einstellungen: aktuelle Kombination " +
                "aus Agent + Anweisungen unter einem Namen speichern, später mit einem Klick wieder " +
                "anwenden, statt dieselben Anweisungen erneut einzutippen. Neuer \"Fokusmodus\" (F11, oder " +
                "das Ansicht-Menü unter Windows Classic): blendet Seitenleiste/Menüleiste/Kopfzeile/" +
                "Werkzeugkatalog/Statusleiste aus, nur die Unterhaltung bleibt - Esc beendet ihn. v4.5.0 " +
                "wurde als FINALE Version von Kombos festgelegt - alles Weitere baut auf dieses Ziel hin; " +
                "die vollständige Roadmap zeigt, was noch fehlt.\n" +
                "v4.1.0 — Echte Light/Dark-Korrektur + Unterhaltungs-Abzweigung + Prompt-Bibliothek: der " +
                "Dark Mode \"funktionierte\" sichtbar nicht im Windows-Classic-Skin - die echte Ursache wurde " +
                "gefunden (die Classic-Palette ignorierte den Dark Mode komplett) und eine echte dunkle " +
                "Variante wurde hinzugefügt. Neue \"Design\"-Option in Erscheinungsbild mit 3 Zuständen: Hell " +
                "/ Dunkel / System (folgt automatisch und live dem Windows-Design). Auch die Position der " +
                "Sprach-/Artifacts-Popups unter Windows Classic wurde korrigiert (korrekt verankert, aber mit " +
                "falscher Placement, landeten außerhalb des Fensters), ebenso das zu schmale Titelfeld bei " +
                "\"+ Neues Project\". Recherche zu ähnlichen Open-Source-Apps (LibreChat, NextChat, Cherry " +
                "Studio) ergab zwei neue, eigene Implementierungen: \"🔀 Abzweigen\" einer Unterhaltung ab " +
                "einer beliebigen Nachricht (neue unabhängige Unterhaltung mit dem Verlauf bis dahin, ohne " +
                "das Original zu verändern), und eine \"📚 Prompt-Bibliothek\" (10 eigene, originale " +
                "Startprompts in 5 Kategorien - keine Tokenkosten, bis einer gesendet wird). Die Suche über " +
                "ALLE Unterhaltungen (Titel + Inhalt) war bereits vorher implementiert.\n" +
                "v4.0.0 — Meilenstein: Skin-/Fenster-Korrekturen + Konten + Katalog-Seitenumbruch. Der Windows-" +
                "Classic-Skin hatte vergessene Popups (Sprache, Artifacts, Projekte/Verlauf), die abgerundet " +
                "blieben und an der jetzt versteckten Position der Seitenleiste erschienen - vollständig " +
                "behoben, samt einer tieferen Ursache: ein MenuItem der klassischen Menüleiste ist kein " +
                "sicherer Anker, sobald sein Menü schließt, wodurch Popups sich lautlos nicht öffneten. Der " +
                "Nexus-Skin im Dunkelmodus verwendet jetzt eine neutrale Anthrazit-/Graphit-Palette (im Stil " +
                "des Adobe-Acrobat-Pro-Dunkelmodus) statt der vorherigen, sehr dunklen. Das Maximieren " +
                "respektiert jetzt exakt die Bildschirmgrenzen (Taskleiste/Mehrbildschirm-Setups), statt " +
                "überzulaufen. Der Tool-Katalog blättert jetzt mit seitlichen Pfeilen statt mit einer " +
                "Bildlaufleiste durch die Kacheln. Neues „Konten“-Fenster (über das Taskleisten-Symbol), das " +
                "zeigt, welche Tools gespeicherte Daten haben, und die echte Anmelde-/Registrierungsseite des " +
                "Tools in deinem Browser öffnet - Kombos meldet sich niemals in deinem Namen an oder " +
                "registriert dich.\n" +
                "v3.9.0 — Projekte (im Claude-Desktop-Stil) + Korrekturen: neuer Bereich „PROJECTS“ über der " +
                "Unterhaltungsliste mit einer Schaltfläche „+ Neues Projekt“ - gruppiere Unterhaltungen unter " +
                "einem Namen, neue Chats, die innerhalb eines Projekts gestartet werden, werden ihm automatisch " +
                "zugeordnet. Die Anhang-Schaltfläche akzeptiert jetzt JEDEN Dateityp (nicht nur Bilder) - " +
                ".md/.txt/.json/Code werden als Textkontext zum Chat hinzugefügt. Neue optionale Benutzername-/" +
                "Passwortfelder pro Agent in den Einstellungen (DPAPI-verschlüsselt, nur zu deiner eigenen " +
                "Referenz - füllt nie automatisch ein Website-Login aus). Neues Katalog-Tool „OmniRoute“ " +
                "(lokales KI-Gateway eines Drittanbieters, Community-Projekt - musst du selbst starten). Der " +
                "GitHub-MCP-Connector ist als Beispiel in der MCP-Serverliste vorausgefüllt (benötigt OAuth, " +
                "das noch nicht automatisiert ist). Korrekturen: der animierte Hintergrund „Orbits“ zeigt jetzt " +
                "die Umlaufbahnlinie, und das linke Zahnrad bei „Zahnräder“ überlappt nicht mehr mit dem " +
                "animierten Titelsymbol.\n" +
                "v3.8.0 — Befehlspalette + Business-Toolkit + Geteilte Projekte: neues Strg+K-Schnellbefehl-" +
                "Fenster, das Tools/Befehle/Einstellungen durchsucht. Neues Business-Toolkit (lokale Plugins, " +
                "keine Token-Kosten): /vat, /loan, /percent, /workdays (Arbeitstage unter Berücksichtigung " +
                "griechischer Feiertage), /iban, /invoice-number. Neue „Geteilte Projekte“: ein Project-Memory-" +
                "Projekt als ZIP im Sync-Ordner veröffentlichen - jeder Computer mit demselben Ordner sieht die " +
                "Liste mit Status (Ausstehend/In Bearbeitung/Abgeschlossen) und kann das ZIP herunterladen (es " +
                "gibt keine öffentliche API für die Projects von claude.ai, das hier ist die echte Alternative). " +
                "Korrekturen: der Windows-Classic-Skin hat jetzt überall den korrekten grauen Hintergrund/die " +
                "navyfarbene Titelleiste, und die Beschriftungen der vertikalen Tab-Schiene im Nexus-Skin werden " +
                "nicht mehr abgeschnitten.\n" +
                "v3.7.0 — Visuelles Upgrade + Kreativ-Toolkit: zwei neue Skins (Windows Classic — überall " +
                "eckige Ecken, erhabene 3D-Bevel-Kacheln, navyfarbene Titelleiste, und eine KLASSISCHE " +
                "MENÜLEISTE — Datei/Bearbeiten/Ansicht/Extras/Hilfe — die die Icon-Seitenleiste komplett " +
                "ersetzt; Nexus — originales oranges Branding, vertikale Tab-Schiene in den Einstellungen). " +
                "Jeder Skin hat jetzt eine authentische Akzentfarbe seiner Ära (Aero-Blau, Metro-Blau, " +
                "Fluent-Blau, Classic-Navy). Neues Kreativ-Toolkit (lokale Plugins, keine Token-Kosten): " +
                "/gradient, /aspect-ratio, /palette, /contrast (WCAG-Kontrastprüfung), /favicon-checklist.\n" +
                "v3.6.0 — Großes \"Multi-Tool\"-Update: eine neue \"Herunterladen & Installieren\"-Galerie unter " +
                "Einstellungen → Erweiterungen für die 1-Klick-Installation/Deinstallation offizieller Plugins " +
                "(Entwickler-Toolkit mit /json /base64 /hash /uuid /color /timestamp /regex /git /http /cron, " +
                "Office-Toolkit mit /wordcount /lorem /password /diff /case, ein Wetter-Plugin) - alles lokal, " +
                "keine Token-Kosten. Neues \"Kollektivgedächtnis\" (Hive Memory): was ein Agent lernt " +
                "(/remember, /hive), wissen jetzt alle Agenten. Neue Handoff-Pipeline (🔗-Schaltfläche neben dem " +
                "Nachrichtenfeld): eine Entwurfsantwort eines Agenten, verfeinert von einem zweiten. Neues " +
                "\"Projektgedächtnis\": ein Projekt als schlanken Index (Dateibaum) statt vollständigem Inhalt " +
                "importieren, eine bestimmte Datei bei Bedarf mit /readfile abrufen, und eine lokale " +
                "Kompatibilitätsprüfung mit /verify ausführen (führt den eigenen Build/Lint des Projekts aus, " +
                "ohne KI-Aufruf). Eine Prompt-Cache-Trefferquote-Anzeige in der Statusleiste, das Katalog-JSON " +
                "wird jetzt einmal statt bei jeder Nachricht neu aufgebaut, und ältere Nachrichten werden in sehr " +
                "langen Unterhaltungen automatisch gekürzt, um Tokens zu sparen.\n" +
                "v3.0.0 — Erste vollständige, \"ausgelieferte\" Version: ein Installationsprogramm " +
                "(KombosSetup.exe), keine Administratorrechte nötig, mit optionaler Desktop-Verknüpfung und " +
                "Start mit Windows während der Installation. Vollständiges Plugin-System, Prompt-Caching + " +
                "MCP-Server für Claude, sicheres Löschen einzelner Nachrichten/Duplizieren von Unterhaltungen, " +
                "ein Artifacts-Panel, ein Synchronisationsordner für Unterhaltungen zwischen Computern, eine " +
                "Erstnutzungs-Anleitung, automatisierte Tests, sowie wichtige Speicher-/CPU-Korrekturen - alles, " +
                "was seit v2.7.0 gebaut wurde, gebündelt in einer stabilen, getesteten Version.\n" +
                "v2.9.9 — Geringerer Speicher-/CPU-Verbrauch: Tool-Logos werden jetzt zwischengespeichert " +
                "statt bei jedem Design-/Sprach-/Suchwechsel erneut aus dem Web geladen zu werden, die " +
                "Tool-Suche baut den gesamten Katalog nicht mehr bei jedem Tastendruck neu auf, und animierte " +
                "Hintergründe stoppen jetzt vollständig, während Kombos versteckt im Infobereich läuft, statt " +
                "unbegrenzt im Hintergrund weiterzulaufen.\n" +
                "v2.9.5 — Klare Trennung der Einstellungs-Einstiegspunkte: das Zahnrad öffnet die allgemeinen " +
                "App-Einstellungen (Verhalten), der „C“-Avatar öffnet die eigenen personalisierten " +
                "Einstellungen des zentralen Agenten und die Zusammenarbeit mit anderen Agenten (MCP) - " +
                "weiterhin dieselben zwei, und einzigen, Einstiegspunkte in die Einstellungen; ein neues " +
                "„Chats“-Symbol in der Seitenleiste für schnellen Zugriff auf aktuelle und ältere Chats.\n" +
                "v2.9.0 — Eine Einführungstour mit Tutorial-Seiten beim ersten Start, sicheres Löschen einer " +
                "einzelnen Nachricht, Chat duplizieren, ein „Artifacts“-Panel in der Seitenleiste mit allen " +
                "Codeblöcken aus jedem Chat (kopieren/speichern), ein Sync-Ordner für Chats (OneDrive/Dropbox/" +
                "Netzwerk) zum Teilen zwischen Computern, und ein Fix für einen Bug, bei dem „Neu generieren“ " +
                "auf einer Fehlerblase echten Verlauf löschen konnte.\n" +
                "v2.8.0 — Prompt-Caching für Claude (günstigere Antworten, solange sich System-Prompt/Katalog " +
                "nicht ändern), Unterstützung für externe MCP-Server in Einstellungen → Agent (nur Claude), " +
                "und eine Regel für minimale Token-Kosten, die ausnahmslos für jeden Agenten gilt (im Code pro " +
                "Anbieter dokumentiert).\n" +
                "v2.7.0 — Plugin-System: ein neuer „Plugins“-Tab in den Einstellungen, ein Ordner " +
                "%AppData%\\Kombos\\plugins\\ für Erweiterungen von Drittanbietern, „/etwas“-Chatbefehle, die " +
                "von Plugins registriert werden (Beispiel dabei: /weather <Stadt>).\n" +
                "v2.6.0 — Eine Tool-Suche neben den Kategorie-Tabs - durchsucht alle Kategorien gleichzeitig.\n" +
                "v2.5.0 — Lieblings-Tools anheften (⭐ auf jeder Kachel und im Detailpanel - angeheftete werden " +
                "zuerst in ihrer Kategorie angezeigt), ein neuer Tab „Verhalten“ in den Einstellungen (immer im " +
                "Vordergrund, mit Windows starten, minimiert starten, Benachrichtigungston), Export/Import eines " +
                "vollständigen JSON-Backups deiner Chats, Tastenkombinationen (Strg+N neuer Chat, Strg+, " +
                "Einstellungen, Esc zum Schließen), und ein „Zum Ende springen“-Button beim Hochscrollen zu " +
                "älteren Nachrichten.\n" +
                "v2.4.0 — Benachrichtigung im Infobereich, wenn eine Antwort eintrifft, während das Fenster " +
                "nicht im Vordergrund ist; die Suche in gespeicherten Chats durchsucht jetzt auch den Inhalt " +
                "(nicht nur den Titel); ein ungesendeter Entwurf bleibt beim Wechseln des Chats erhalten; " +
                "einstellbare Textgröße im Chat (Klein/Normal/Groß, unter Darstellung) für Barrierefreiheit; " +
                "und ein Button zum Löschen aller gespeicherten Chats auf einmal (mit Bestätigung) in den " +
                "Einstellungen.\n" +
                "v2.3.0 — API-Schlüssel werden jetzt vor dem Speichern mit Windows-DPAPI verschlüsselt (an dein " +
                "Windows-Konto gebunden) statt im Klartext.\n" +
                "v2.2.0 — Markdown-Rendering in Antworten (fett/kursiv/Inline-Code, Codeblöcke mit eigenem " +
                "Kopieren-Button), eigene Nachricht vor dem erneuten Senden bearbeiten (✏ bei jeder eigenen " +
                "Chatblase), Suche innerhalb gespeicherter Chats, sowie mehrere Stabilitätskorrekturen: " +
                "„Neu generieren“ ersetzt die alte Antwort jetzt korrekt, statt eine zweite darunter " +
                "hinzuzufügen, und löscht bei einem Fehler keinen echten Verlauf mehr; API-Schlüssel bleiben " +
                "jetzt über Neustarts erhalten; ein möglicher Chat-Mix behoben, wenn eine verspätete Antwort " +
                "nach dem Wechsel zu einem anderen Chat eintraf.\n" +
                "v2.1.0 — Gespeicherte Chats umbenennen (✏ in der Verlaufsliste), benutzerdefinierte Anweisungen " +
                "für den zentralen Agenten (Einstellungen → Agent), größere, besser erkennbare Schaltflächen neben " +
                "dem Nachrichtenfeld, Hilfe und Versionsverlauf vollständig in getrennte Abschnitte mit fett " +
                "hervorgehobenen Überschriften aufgeteilt.\n" +
                "v2.0.0 — Antwort neu generieren, Antworten mehrerer Agenten gleichzeitig vergleichen (⚖ neben dem " +
                "Nachrichtenfeld), mehrere gespeicherte Chats mit eigenem Verlauf (🕘) statt eines einzigen " +
                "durchgehenden Chats, ein Symbol im Infobereich, damit Kombos beim Schließen des Fensters im " +
                "Hintergrund weiterläuft, und ein globaler Hotkey (Strg+Alt+K), um es von überall in Windows zu " +
                "öffnen.\n" +
                "v1.9.0 — Echte Websuche (umschaltbar, 🌐 neben dem Nachrichtenfeld) über Claudes offizielles " +
                "serverseitiges Websuche-Tool — es entscheidet selbst, wann eine Suche nötig ist, und die " +
                "gefundenen Quellen erscheinen als anklickbare Links unter der Antwort.\n" +
                "v1.8.0 — Bild aus der Zwischenablage mit Strg+V einfügen (z. B. ein Screenshot) direkt in den " +
                "Chat, Bild/Datei/Ordner per Drag & Drop auf das Chat-Panel ziehen (erkennt automatisch, ob es " +
                "ein Bildanhang oder zu importierender Kontext ist), verbesserter Kontrast bei allen animierten " +
                "Hintergründen, damit sie in jedem Design/Skin klar erkennbar sind.\n" +
                "v1.7.0 — 10 auswählbare animierte Kopfzeilen-Hintergründe (Zahnräder/Umlaufbahn/Wellen/Rasterpuls " +
                "inspiriert vom Windows Optimizer, plus Knotennetzwerk/Bokeh/Polarlicht/Sternenhimmel), ein " +
                "\"Neuer Chat\"-Button, ein Kopieren-Button bei jeder Antwort, die Möglichkeit, eine laufende " +
                "Antwort abzubrechen, ein mehrzeiliges Eingabefeld (Umschalt+Eingabe fügt jetzt wirklich eine neue " +
                "Zeile ein), ein behobenes Paletten-Symbol im Darstellungsfenster.\n" +
                "v1.6.0 — Bildanhänge im Chat (Claude \"sieht\" sie wirklich über die Vision-API), ein umschaltbares, " +
                "sichtbares Denkprozess-Panel (extended thinking) über den Antworten, ein neuer eigener " +
                "Zahnrad-Button für die allgemeinen App-Einstellungen (Agent/Projekt/Info), während der " +
                "Paletten-Button jetzt ein eigenes, kleineres Fenster nur für Darstellung/Designs/Skins öffnet.\n" +
                "v1.5.0 — Die Darstellungseinstellungen sind jetzt ein eigenes Fenster mit Tabs (Agent/Darstellung/Projekt/Info) " +
                "statt eines Popup-Menüs, das sich von selbst schloss, sobald ein anderes Fenster den Fokus erhielt; ein Fehler " +
                "wurde behoben, bei dem Tool-Logos nach einem Wechsel des Windows-Designs nur noch als einfacher Buchstabe " +
                "statt als echtes Symbol angezeigt wurden.\n" +
                "v1.4.0 — Auswählbare visuelle „Designs“ inspiriert von Windows Vista/7/8/10-11 (Ecken, Schatten, " +
                "glänzende Schaltflächen, Metro-Kacheln unter Windows 8) im Darstellungsmenü.\n" +
                "v1.3.0 — Echte Transparenz auf Fensterebene (nicht mehr auf Panels/Schaltflächen) mit einer " +
                "benutzerdefinierten Titelleiste, einem Start-Splashscreen, der sich zur Position des Titels " +
                "verkleinert, benutzerdefinierten Vektorsymbolen auf den Kategorie-Tabs (statt Emojis), einer " +
                "Auswahl des zentralen KI-Agenten (über Claude hinaus) mit echtem Chat für " +
                "ChatGPT/Mistral/DeepSeek/Grok/Perplexity/Gemini, einstellbaren Transparenzstufen & animierten " +
                "Hintergrundoptionen, vollständigen Katalogübersetzungen auf Französisch/Deutsch.\n" +
                "v1.2.0 — Gruppiertes Einstellungsmenü (Agent, Darstellung, Projekt, Infos), Globus-Symbol für die " +
                "Sprachauswahl (universeller als eine Flagge), korrigierte britische Flagge, weiße Hintergrundplatte " +
                "hinter jedem Logo für Kontrast im Dunkelmodus, Aero-Stil (Transparenz/Schatten/animierter Hintergrund), " +
                "Import ganzer Ordner als Projekt, Chat-Export in Datei, echtes Nutzungs-Tracking für ElevenLabs, " +
                "Einstellungen bleiben über Neustarts erhalten.\n" +
                "v1.1.0 — Mehrsprachige Oberfläche in 4 Sprachen (Griechisch/Englisch/Französisch/Deutsch) mit " +
                "Flaggenauswahl, Logos pro Tool (mit Korrektur für Microsoft Copilot), dieses Hilfe-/Versionspanel, " +
                "Projektimport aus Datei, Einstellung der Seitenleistenposition, animiertes Titelsymbol & Titelschrift, " +
                "allgemeine Kontrast-/Rendering-Korrekturen.\n" +
                "v1.0.0 — Erste vollständige Version: Build/Start unter Windows bestätigt, App-Symbol, Kategorie-Tabs mit echtem " +
                "Inhaltswechsel, Agenten-/Login-Menü unter „C“ mit Schlüsselspeicherung pro Tool."
        };

        public static string AppearanceLiveNote => Current switch
        {
            Lang.EL => "Όλες οι παραπάνω ρυθμίσεις εφαρμόζονται αμέσως, χωρίς επανεκκίνηση.",
            Lang.EN => "All settings above apply immediately, no restart needed.",
            Lang.FR => "Tous les réglages ci-dessus s'appliquent immédiatement, sans redémarrage.",
            _ => "Alle obigen Einstellungen werden sofort angewendet, kein Neustart nötig."
        };

        public static string SkinLabel => Current switch
        {
            Lang.EL => "Θέμα Windows:",
            Lang.EN => "Windows theme:",
            Lang.FR => "Thème Windows :",
            _ => "Windows-Design:"
        };

        public static string SkinName(WindowsSkin skin) => (Current, skin) switch
        {
            (Lang.EL, WindowsSkin.Modern) => "Σύγχρονο (προεπιλογή)",
            (Lang.EL, WindowsSkin.Vista) => "Windows Vista (γυαλί)",
            (Lang.EL, WindowsSkin.Win7) => "Windows 7 (Aero)",
            (Lang.EL, WindowsSkin.Win8) => "Windows 8 (Metro / πλακίδια)",
            (Lang.EL, WindowsSkin.Classic) => "Windows Classic (95/98/2000)",
            (Lang.EL, WindowsSkin.Nexus) => "Nexus (πρωτότυπο Kombos)",
            (Lang.EL, _) => "Windows 10/11 (Fluent)",
            (Lang.EN, WindowsSkin.Modern) => "Modern (default)",
            (Lang.EN, WindowsSkin.Vista) => "Windows Vista (glass)",
            (Lang.EN, WindowsSkin.Win7) => "Windows 7 (Aero)",
            (Lang.EN, WindowsSkin.Win8) => "Windows 8 (Metro tiles)",
            (Lang.EN, WindowsSkin.Classic) => "Windows Classic (95/98/2000)",
            (Lang.EN, WindowsSkin.Nexus) => "Nexus (original Kombos)",
            (Lang.EN, _) => "Windows 10/11 (Fluent)",
            (Lang.FR, WindowsSkin.Modern) => "Moderne (par défaut)",
            (Lang.FR, WindowsSkin.Vista) => "Windows Vista (verre)",
            (Lang.FR, WindowsSkin.Win7) => "Windows 7 (Aero)",
            (Lang.FR, WindowsSkin.Win8) => "Windows 8 (tuiles Metro)",
            (Lang.FR, WindowsSkin.Classic) => "Windows Classic (95/98/2000)",
            (Lang.FR, WindowsSkin.Nexus) => "Nexus (original Kombos)",
            (Lang.FR, _) => "Windows 10/11 (Fluent)",
            (_, WindowsSkin.Modern) => "Modern (Standard)",
            (_, WindowsSkin.Vista) => "Windows Vista (Glas)",
            (_, WindowsSkin.Win7) => "Windows 7 (Aero)",
            (_, WindowsSkin.Win8) => "Windows 8 (Metro-Kacheln)",
            (_, WindowsSkin.Classic) => "Windows Classic (95/98/2000)",
            (_, WindowsSkin.Nexus) => "Nexus (original von Kombos)",
            (_, _) => "Windows 10/11 (Fluent)"
        };

        public static string SectionAgent => Current switch
        {
            Lang.EL => "AGENT & ΣΥΝΔΕΣΗ",
            Lang.EN => "AGENT & CONNECTION",
            Lang.FR => "AGENT & CONNEXION",
            _ => "AGENT & VERBINDUNG"
        };

        public static string SectionAppearance => Current switch
        {
            Lang.EL => "ΕΜΦΑΝΙΣΗ",
            Lang.EN => "APPEARANCE",
            Lang.FR => "APPARENCE",
            _ => "DARSTELLUNG"
        };

        public static string SectionProject => Current switch
        {
            Lang.EL => "PROJECT & ΣΥΖΗΤΗΣΗ",
            Lang.EN => "PROJECT & CHAT",
            Lang.FR => "PROJET & CHAT",
            _ => "PROJEKT & CHAT"
        };

        public static string SectionAbout => Current switch
        {
            Lang.EL => "ΠΛΗΡΟΦΟΡΙΕΣ",
            Lang.EN => "INFO",
            Lang.FR => "INFOS",
            _ => "INFOS"
        };

        public static string SectionBehavior => Current switch
        {
            Lang.EL => "ΣΥΜΠΕΡΙΦΟΡΑ",
            Lang.EN => "BEHAVIOR",
            Lang.FR => "COMPORTEMENT",
            _ => "VERHALTEN"
        };

        public static string SectionPlugins => Current switch
        {
            Lang.EL => "ΕΠΕΚΤΑΣΕΙΣ",
            Lang.EN => "PLUGINS",
            Lang.FR => "EXTENSIONS",
            _ => "PLUGINS"
        };

        public static string PluginsFolderHint => Current switch
        {
            Lang.EL => "Βάλε τον φάκελο ενός plugin (με το .dll του) μέσα σε αυτόν τον φάκελο, μετά κάνε επανεκκίνηση του Kombos.",
            Lang.EN => "Drop a plugin's folder (with its .dll) inside this folder, then restart Kombos.",
            Lang.FR => "Dépose le dossier d'une extension (avec son .dll) dans ce dossier, puis redémarre Kombos.",
            _ => "Lege den Ordner eines Plugins (mit seiner .dll) in diesen Ordner und starte Kombos neu."
        };

        public static string OpenPluginsFolderButton => Current switch
        {
            Lang.EL => "📂 Άνοιγμα φακέλου plugins",
            Lang.EN => "📂 Open plugins folder",
            Lang.FR => "📂 Ouvrir le dossier des extensions",
            _ => "📂 Plugins-Ordner öffnen"
        };

        public static string PluginsRestartHint => Current switch
        {
            Lang.EL => "Η ενεργοποίηση/απενεργοποίηση ενός plugin εφαρμόζεται μετά από επανεκκίνηση του Kombos.",
            Lang.EN => "Enabling or disabling a plugin takes effect after restarting Kombos.",
            Lang.FR => "Activer ou désactiver une extension prend effet après le redémarrage de Kombos.",
            _ => "Das Aktivieren/Deaktivieren eines Plugins wirkt sich erst nach einem Neustart von Kombos aus."
        };

        public static string PluginsEmpty => Current switch
        {
            Lang.EL => "Δεν υπάρχουν ακόμα plugins. Άνοιξε τον φάκελο παρακάτω για να προσθέσεις ένα.",
            Lang.EN => "No plugins yet. Open the folder below to add one.",
            Lang.FR => "Aucune extension pour l'instant. Ouvre le dossier ci-dessous pour en ajouter une.",
            _ => "Noch keine Plugins. Öffne den Ordner unten, um eins hinzuzufügen."
        };

        public static string PluginBroken => Current switch
        {
            Lang.EL => "απέτυχε η φόρτωση",
            Lang.EN => "failed to load",
            Lang.FR => "échec du chargement",
            _ => "Laden fehlgeschlagen"
        };

        public static string TrimmedTurnSuffix => Current switch
        {
            Lang.EL => " […παλαιότερο μήνυμα, περικομμένο για εξοικονόμηση τόκενς]",
            Lang.EN => " […older message, trimmed to save tokens]",
            Lang.FR => " […message plus ancien, tronqué pour économiser des tokens]",
            _ => " […ältere Nachricht, zur Token-Einsparung gekürzt]"
        };

        public static string HandoffTitle => Current switch
        {
            Lang.EL => "🔗 Handoff: 2 agents σε αλυσίδα",
            Lang.EN => "🔗 Handoff: 2 agents in a chain",
            Lang.FR => "🔗 Handoff : 2 agents en chaîne",
            _ => "🔗 Handoff: 2 Agenten in einer Kette"
        };

        public static string HandoffStepALabel => Current switch
        {
            Lang.EL => "Στάδιο 1 - πρόχειρη απάντηση:",
            Lang.EN => "Step 1 - draft reply:",
            Lang.FR => "Étape 1 - réponse initiale :",
            _ => "Schritt 1 - Entwurfsantwort:"
        };

        public static string HandoffStepBLabel => Current switch
        {
            Lang.EL => "Στάδιο 2 - βελτίωση:",
            Lang.EN => "Step 2 - refinement:",
            Lang.FR => "Étape 2 - amélioration :",
            _ => "Schritt 2 - Verfeinerung:"
        };

        public static string HandoffHint => Current switch
        {
            Lang.EL => "Το κείμενο στο πεδίο μηνύματος στέλνεται στον πρώτο agent· η απάντησή του στέλνεται στον δεύτερο για βελτίωση/κριτική. Δεν επηρεάζει την κύρια συζήτηση.",
            Lang.EN => "The text in the message box is sent to the first agent; its reply is sent to the second for refinement/critique. Doesn't affect the main conversation.",
            Lang.FR => "Le texte du champ de message est envoyé au premier agent ; sa réponse est envoyée au second pour amélioration/critique. N'affecte pas la conversation principale.",
            _ => "Der Text im Nachrichtenfeld wird an den ersten Agenten gesendet; seine Antwort geht zur Verfeinerung/Kritik an den zweiten. Beeinflusst nicht die Hauptunterhaltung."
        };

        public static string HandoffRunButton => Current switch
        {
            Lang.EL => "Εκτέλεση",
            Lang.EN => "Run",
            Lang.FR => "Exécuter",
            _ => "Ausführen"
        };

        public static string HandoffStageLabel(string agentName, int stage) => Current switch
        {
            Lang.EL => $"{agentName} (στάδιο {stage})",
            Lang.EN => $"{agentName} (step {stage})",
            Lang.FR => $"{agentName} (étape {stage})",
            _ => $"{agentName} (Schritt {stage})"
        };

        public static string HandoffStageBPrompt(string originalRequest, string draftReply) => Current switch
        {
            Lang.EL => $"Αρχικό αίτημα του χρήστη: {originalRequest}\n\nΠρόχειρη απάντηση από άλλον agent: {draftReply}\n\nΒελτίωσε, διόρθωσε ή συμπλήρωσε την παραπάνω πρόχειρη απάντηση, όσο πιο χρήσιμα γίνεται.",
            Lang.EN => $"User's original request: {originalRequest}\n\nDraft reply from another agent: {draftReply}\n\nRefine, correct, or complete the draft reply above, as usefully as possible.",
            Lang.FR => $"Demande initiale de l'utilisateur : {originalRequest}\n\nRéponse initiale d'un autre agent : {draftReply}\n\nAméliore, corrige ou complète la réponse ci-dessus, aussi utilement que possible.",
            _ => $"Ursprüngliche Anfrage des Nutzers: {originalRequest}\n\nEntwurfsantwort eines anderen Agenten: {draftReply}\n\nVerfeinere, korrigiere oder vervollständige die obige Entwurfsantwort, so nützlich wie möglich."
        };

        public static string AccountsTitle => Current switch
        {
            Lang.EL => "Λογαριασμοί",
            Lang.EN => "Accounts",
            Lang.FR => "Comptes",
            _ => "Konten"
        };

        public static string AccountsHint => Current switch
        {
            Lang.EL => "Το Kombos δεν κάνει ποτέ σύνδεση ή εγγραφή για λογαριασμό σου - κάθε κουμπί ανοίγει την πραγματική σελίδα του εργαλείου στον browser σου, όπου συνδέεσαι ή εγγράφεσαι εσύ ο ίδιος.",
            Lang.EN => "Kombos never logs in or signs up on your behalf - each button opens the tool's real page in your browser, where you log in or sign up yourself.",
            Lang.FR => "Kombos ne se connecte ni ne s'inscrit jamais à ta place - chaque bouton ouvre la vraie page de l'outil dans ton navigateur, où tu te connectes ou t'inscris toi-même.",
            _ => "Kombos meldet sich niemals in deinem Namen an oder registriert dich - jede Schaltfläche öffnet die echte Seite des Tools in deinem Browser, wo du dich selbst anmeldest oder registrierst."
        };

        public static string AccountsStatusSaved => Current switch
        {
            Lang.EL => "✓ Έχεις αποθηκευμένα στοιχεία αναφοράς",
            Lang.EN => "✓ You have saved reference details",
            Lang.FR => "✓ Tu as des identifiants de référence enregistrés",
            _ => "✓ Du hast gespeicherte Referenzdaten"
        };

        public static string AccountsStatusNone => Current switch
        {
            Lang.EL => "Χωρίς αποθηκευμένα στοιχεία",
            Lang.EN => "No saved details",
            Lang.FR => "Aucun identifiant enregistré",
            _ => "Keine gespeicherten Daten"
        };

        /// <summary>"Συνδεδεμένος" = υπάρχει αποθηκευμένο API key για το εργαλείο, το ίδιο κριτήριο που
        /// χρησιμοποιεί η γραμμή κατάστασης (UsageRow.IsConnected) - όχι πραγματικό δικτυακό ping.</summary>
        public static string AccountsStatusConnected => Current switch
        {
            Lang.EL => "🟢 Συνδεδεμένος",
            Lang.EN => "🟢 Connected",
            Lang.FR => "🟢 Connecté",
            _ => "🟢 Verbunden"
        };

        public static string AccountsStatusDisconnected => Current switch
        {
            Lang.EL => "⚪ Μη συνδεδεμένος",
            Lang.EN => "⚪ Not connected",
            Lang.FR => "⚪ Non connecté",
            _ => "⚪ Nicht verbunden"
        };

        public static string AccountsOpenLoginButton => Current switch
        {
            Lang.EL => "Άνοιγμα σύνδεσης ↗",
            Lang.EN => "Open login ↗",
            Lang.FR => "Ouvrir la connexion ↗",
            _ => "Anmeldung öffnen ↗"
        };

        public static string AccountsSignUpButton => Current switch
        {
            Lang.EL => "Σύνδεση / Εγγραφή ↗",
            Lang.EN => "Log in / Sign up ↗",
            Lang.FR => "Connexion / Inscription ↗",
            _ => "Anmelden / Registrieren ↗"
        };

        public static string LoginCredentialsLabel => Current switch
        {
            Lang.EL => "Στοιχεία σύνδεσης (username / password)",
            Lang.EN => "Login details (username / password)",
            Lang.FR => "Identifiants de connexion (nom d'utilisateur / mot de passe)",
            _ => "Anmeldedaten (Benutzername / Passwort)"
        };

        public static string LoginCredentialsHint => Current switch
        {
            Lang.EL => "Προαιρετικό, μόνο για δική σου αναφορά όταν ένα εργαλείο απαιτεί σύνδεση μέσω browser αντί για API key. Αποθηκεύεται τοπικά, κρυπτογραφημένο με DPAPI (δεμένο στον λογαριασμό σου Windows) - το Kombos δεν το στέλνει πουθενά και δεν συμπληρώνει καμία φόρμα ιστοσελίδας αυτόματα.",
            Lang.EN => "Optional, just for your own reference when a tool requires browser login instead of an API key. Stored locally, DPAPI-encrypted (tied to your Windows account) - Kombos never sends it anywhere and never auto-fills any website form.",
            Lang.FR => "Facultatif, uniquement pour ta propre référence quand un outil nécessite une connexion via navigateur au lieu d'une clé API. Stocké localement, chiffré avec DPAPI (lié à ton compte Windows) - Kombos ne l'envoie jamais nulle part et ne remplit automatiquement aucun formulaire de site web.",
            _ => "Optional, nur zu deiner eigenen Referenz, wenn ein Tool eine Browser-Anmeldung statt eines API-Schlüssels erfordert. Lokal gespeichert, DPAPI-verschlüsselt (an dein Windows-Konto gebunden) - Kombos sendet es nirgendwohin und füllt damit nie automatisch ein Website-Formular aus."
        };

        public static string HiveMemoryLabel => Current switch
        {
            Lang.EL => "🐝 Συλλογική μνήμη (Hive Memory)",
            Lang.EN => "🐝 Hive Memory",
            Lang.FR => "🐝 Mémoire collective (Hive Memory)",
            _ => "🐝 Kollektivgedächtnis (Hive Memory)"
        };

        public static string HiveMemoryHint => Current switch
        {
            Lang.EL => "Γεγονότα/προτιμήσεις που θυμούνται ΟΛΟΙ οι agents (Claude και κάθε άλλο εργαλείο). Πρόσθεσέ τα εδώ ή γράψε /remember <κάτι> στη συνομιλία.",
            Lang.EN => "Facts/preferences remembered by EVERY agent (Claude and every other tool). Add them here or type /remember <something> in chat.",
            Lang.FR => "Faits/préférences dont se souviennent TOUS les agents (Claude et chaque autre outil). Ajoute-les ici ou tape /remember <quelque chose> dans le chat.",
            _ => "Fakten/Vorlieben, an die sich JEDER Agent erinnert (Claude und jedes andere Tool). Füge sie hier hinzu oder tippe /remember <etwas> im Chat."
        };

        public static string HiveMemoryClearButton => Current switch
        {
            Lang.EL => "Εκκαθάριση συλλογικής μνήμης",
            Lang.EN => "Clear hive memory",
            Lang.FR => "Effacer la mémoire collective",
            _ => "Kollektivgedächtnis löschen"
        };

        public static string HiveMemoryPromptPrefix => Current switch
        {
            Lang.EL => "Συλλογική μνήμη του Κόμβου (γεγονότα/προτιμήσεις που έμαθε ο χρήστης σε ΚΑΘΕ agent, λάβε τα υπόψη): ",
            Lang.EN => "Kombos hive memory (facts/preferences the user has taught every agent — take these into account): ",
            Lang.FR => "Mémoire collective de Kombos (faits/préférences enseignés à chaque agent — à prendre en compte) : ",
            _ => "Kombos-Kollektivgedächtnis (Fakten/Vorlieben, die der Nutzer jedem Agenten beigebracht hat — berücksichtige sie): "
        };

        public static string HiveMemoryRememberUsage => Current switch
        {
            Lang.EL => "Χρήση: /remember <κάτι που θέλεις να θυμούνται όλοι οι agents>",
            Lang.EN => "Usage: /remember <something you want every agent to remember>",
            Lang.FR => "Utilisation : /remember <quelque chose que tous les agents doivent retenir>",
            _ => "Verwendung: /remember <etwas, das sich jeder Agent merken soll>"
        };

        public static string HiveMemoryRemembered(string fact) => Current switch
        {
            Lang.EL => $"✅ Το θυμάμαι πλέον όλοι οι agents: «{fact}»",
            Lang.EN => $"✅ Every agent will now remember: \"{fact}\"",
            Lang.FR => $"✅ Chaque agent s'en souviendra désormais : « {fact} »",
            _ => $"✅ Jeder Agent merkt sich das jetzt: „{fact}“"
        };

        public static string HiveMemoryEmpty => Current switch
        {
            Lang.EL => "Η συλλογική μνήμη είναι άδεια. Χρησιμοποίησε /remember <κάτι> για να προσθέσεις.",
            Lang.EN => "Hive memory is empty. Use /remember <something> to add a fact.",
            Lang.FR => "La mémoire collective est vide. Utilise /remember <quelque chose> pour ajouter un fait.",
            _ => "Das Kollektivgedächtnis ist leer. Verwende /remember <etwas>, um eine Tatsache hinzuzufügen."
        };

        public static string CacheHitIndicator(int hitRatePercent) => Current switch
        {
            Lang.EL => $"⚡ {hitRatePercent}% από cache",
            Lang.EN => $"⚡ {hitRatePercent}% from cache",
            Lang.FR => $"⚡ {hitRatePercent}% depuis le cache",
            _ => $"⚡ {hitRatePercent}% aus Cache"
        };

        public static string GalleryHeader => Current switch
        {
            Lang.EL => "📦 Λήψη & Ενσωμάτωση",
            Lang.EN => "📦 Download & Install",
            Lang.FR => "📦 Télécharger et installer",
            _ => "📦 Herunterladen & Installieren"
        };

        public static string GalleryHint => Current switch
        {
            Lang.EL => "Επίσημα plugins που ήρθαν μαζί με την εγκατάσταση. Η εγκατάσταση αντιγράφει τοπικά το plugin, χωρίς δίκτυο. Χρειάζεται επανεκκίνηση για ενεργοποίηση.",
            Lang.EN => "Official plugins bundled with the install. Installing copies the plugin locally, no network involved. Restart to activate.",
            Lang.FR => "Extensions officielles fournies avec l'installation. L'installation copie l'extension localement, sans réseau. Redémarrage nécessaire pour l'activer.",
            _ => "Offizielle Plugins, die mit der Installation geliefert werden. Die Installation kopiert das Plugin lokal, ohne Netzwerk. Neustart zur Aktivierung erforderlich."
        };

        public static string GalleryEmpty => Current switch
        {
            Lang.EL => "Δεν υπάρχουν διαθέσιμα plugins στη γκαλερί αυτή τη στιγμή.",
            Lang.EN => "No plugins available in the gallery right now.",
            Lang.FR => "Aucune extension disponible dans la galerie pour le moment.",
            _ => "Derzeit sind keine Plugins in der Galerie verfügbar."
        };

        public static string GalleryInstalledLabel => Current switch
        {
            Lang.EL => "εγκατεστημένο",
            Lang.EN => "installed",
            Lang.FR => "installée",
            _ => "installiert"
        };

        public static string GalleryInstallButton => Current switch
        {
            Lang.EL => "Εγκατάσταση",
            Lang.EN => "Install",
            Lang.FR => "Installer",
            _ => "Installieren"
        };

        public static string GalleryUninstallButton => Current switch
        {
            Lang.EL => "Αφαίρεση",
            Lang.EN => "Uninstall",
            Lang.FR => "Désinstaller",
            _ => "Deinstallieren"
        };

        public static string AlwaysOnTopLabel => Current switch
        {
            Lang.EL => "Πάντα σε πρώτο πλάνο",
            Lang.EN => "Always on top",
            Lang.FR => "Toujours au premier plan",
            _ => "Immer im Vordergrund"
        };

        public static string StartWithWindowsLabel => Current switch
        {
            Lang.EL => "Εκκίνηση με τα Windows",
            Lang.EN => "Start with Windows",
            Lang.FR => "Démarrer avec Windows",
            _ => "Mit Windows starten"
        };

        public static string StartMinimizedLabel => Current switch
        {
            Lang.EL => "Εκκίνηση ελαχιστοποιημένο (στην περιοχή ειδοποιήσεων)",
            Lang.EN => "Start minimized (to the system tray)",
            Lang.FR => "Démarrer réduit (dans la zone de notification)",
            _ => "Minimiert starten (im Infobereich)"
        };

        public static string StartMinimizedHint => Current switch
        {
            Lang.EL => "Χρήσιμο μαζί με την \"Εκκίνηση με τα Windows\" - το Kombos θα είναι έτοιμο στο παρασκήνιο χωρίς να ανοίγει παράθυρο.",
            Lang.EN => "Useful together with \"Start with Windows\" - Kombos will be ready in the background without opening a window.",
            Lang.FR => "Utile avec « Démarrer avec Windows » - Kombos sera prêt en arrière-plan sans ouvrir de fenêtre.",
            _ => "Nützlich zusammen mit „Mit Windows starten“ - Kombos ist im Hintergrund bereit, ohne ein Fenster zu öffnen."
        };

        public static string SoundOnReplyLabel => Current switch
        {
            Lang.EL => "Ήχος ειδοποίησης όταν φτάνει απάντηση στο παρασκήνιο",
            Lang.EN => "Play a sound when a reply arrives in the background",
            Lang.FR => "Jouer un son quand une réponse arrive en arrière-plan",
            _ => "Ton abspielen, wenn eine Antwort im Hintergrund eintrifft"
        };

        public static string HotkeyInfo => Current switch
        {
            Lang.EL => "Καθολική συντόμευση: Ctrl+Alt+K φέρνει το Kombos μπροστά από οπουδήποτε στα Windows (αν ο συνδυασμός δεν είναι ήδη δεσμευμένος από άλλη εφαρμογή).",
            Lang.EN => "Global shortcut: Ctrl+Alt+K brings Kombos to the front from anywhere in Windows (if the combo isn't already taken by another app).",
            Lang.FR => "Raccourci global : Ctrl+Alt+K ramène Kombos au premier plan depuis n'importe où sous Windows (si la combinaison n'est pas déjà prise par une autre application).",
            _ => "Globaler Shortcut: Strg+Alt+K holt Kombos von überall in Windows in den Vordergrund (sofern die Kombination nicht bereits von einer anderen App belegt ist)."
        };

        public static string ThemeModeLabel => Current switch
        {
            Lang.EL => "Θέμα:",
            Lang.EN => "Theme:",
            Lang.FR => "Thème :",
            _ => "Design:"
        };

        public static string ThemeModeButtonText(AppThemeMode mode) => (Current, mode) switch
        {
            (Lang.EL, AppThemeMode.Light) => "☀ Φωτεινό (πάτησε για Σκοτεινό)",
            (Lang.EL, AppThemeMode.Dark) => "🌙 Σκοτεινό (πάτησε για Σύστημα)",
            (Lang.EL, _) => "🖥 Σύστημα - ακολουθεί Windows (πάτησε για Φωτεινό)",
            (Lang.EN, AppThemeMode.Light) => "☀ Light (click for Dark)",
            (Lang.EN, AppThemeMode.Dark) => "🌙 Dark (click for System)",
            (Lang.EN, _) => "🖥 System - follows Windows (click for Light)",
            (Lang.FR, AppThemeMode.Light) => "☀ Clair (clique pour Sombre)",
            (Lang.FR, AppThemeMode.Dark) => "🌙 Sombre (clique pour Système)",
            (Lang.FR, _) => "🖥 Système - suit Windows (clique pour Clair)",
            (_, AppThemeMode.Light) => "☀ Hell (klicken für Dunkel)",
            (_, AppThemeMode.Dark) => "🌙 Dunkel (klicken für System)",
            (_, _) => "🖥 System - folgt Windows (klicken für Hell)"
        };

        public static string TransparencyLabel => Current switch
        {
            Lang.EL => "Διαφάνεια:",
            Lang.EN => "Transparency:",
            Lang.FR => "Transparence :",
            _ => "Transparenz:"
        };

        public static string TransparencyButtonText(TransparencyLevel level) => (Current, level) switch
        {
            (Lang.EL, TransparencyLevel.None) => "◻ Καθόλου (πάτησε για Λίγη)",
            (Lang.EL, TransparencyLevel.Low) => "◧ Λίγη (πάτησε για Μέση)",
            (Lang.EL, _) => "◇ Μέση (πάτησε για Καθόλου)",
            (Lang.EN, TransparencyLevel.None) => "◻ None (click for Low)",
            (Lang.EN, TransparencyLevel.Low) => "◧ Low (click for Medium)",
            (Lang.EN, _) => "◇ Medium (click for None)",
            (Lang.FR, TransparencyLevel.None) => "◻ Aucune (clique pour Faible)",
            (Lang.FR, TransparencyLevel.Low) => "◧ Faible (clique pour Moyenne)",
            (Lang.FR, _) => "◇ Moyenne (clique pour Aucune)",
            (_, TransparencyLevel.None) => "◻ Keine (klicken für Niedrig)",
            (_, TransparencyLevel.Low) => "◧ Niedrig (klicken für Mittel)",
            (_, _) => "◇ Mittel (klicken für Keine)"
        };

        public static string BgStyleLabel => Current switch
        {
            Lang.EL => "Κινούμενο φόντο:",
            Lang.EN => "Animated background:",
            Lang.FR => "Fond animé :",
            _ => "Animierter Hintergrund:"
        };

        public static string BgStyleName(BgAnimStyle style) => (Current, style) switch
        {
            (Lang.EL, BgAnimStyle.None) => "Κανένα",
            (Lang.EL, BgAnimStyle.Glow) => "Λάμψη",
            (Lang.EL, BgAnimStyle.Particles) => "Σωματίδια",
            (Lang.EL, BgAnimStyle.Gears) => "Γρανάζια",
            (Lang.EL, BgAnimStyle.Orbit) => "Τροχιές",
            (Lang.EL, BgAnimStyle.Waves) => "Κύματα",
            (Lang.EL, BgAnimStyle.GridPulse) => "Πλέγμα",
            (Lang.EL, BgAnimStyle.Network) => "Δίκτυο κόμβων",
            (Lang.EL, BgAnimStyle.Bokeh) => "Φυσαλίδες (Bokeh)",
            (Lang.EL, BgAnimStyle.Aurora) => "Βόρειο σέλας",
            (Lang.EL, _) => "Έναστρος ουρανός",

            (Lang.EN, BgAnimStyle.None) => "None",
            (Lang.EN, BgAnimStyle.Glow) => "Glow",
            (Lang.EN, BgAnimStyle.Particles) => "Particles",
            (Lang.EN, BgAnimStyle.Gears) => "Gears",
            (Lang.EN, BgAnimStyle.Orbit) => "Orbit",
            (Lang.EN, BgAnimStyle.Waves) => "Waves",
            (Lang.EN, BgAnimStyle.GridPulse) => "Grid Pulse",
            (Lang.EN, BgAnimStyle.Network) => "Node Network",
            (Lang.EN, BgAnimStyle.Bokeh) => "Bokeh",
            (Lang.EN, BgAnimStyle.Aurora) => "Aurora",
            (Lang.EN, _) => "Starfield",

            (Lang.FR, BgAnimStyle.None) => "Aucun",
            (Lang.FR, BgAnimStyle.Glow) => "Lueur",
            (Lang.FR, BgAnimStyle.Particles) => "Particules",
            (Lang.FR, BgAnimStyle.Gears) => "Engrenages",
            (Lang.FR, BgAnimStyle.Orbit) => "Orbite",
            (Lang.FR, BgAnimStyle.Waves) => "Vagues",
            (Lang.FR, BgAnimStyle.GridPulse) => "Grille pulsée",
            (Lang.FR, BgAnimStyle.Network) => "Réseau de nœuds",
            (Lang.FR, BgAnimStyle.Bokeh) => "Bokeh",
            (Lang.FR, BgAnimStyle.Aurora) => "Aurore boréale",
            (Lang.FR, _) => "Ciel étoilé",

            (_, BgAnimStyle.None) => "Keiner",
            (_, BgAnimStyle.Glow) => "Leuchten",
            (_, BgAnimStyle.Particles) => "Partikel",
            (_, BgAnimStyle.Gears) => "Zahnräder",
            (_, BgAnimStyle.Orbit) => "Umlaufbahn",
            (_, BgAnimStyle.Waves) => "Wellen",
            (_, BgAnimStyle.GridPulse) => "Rasterpuls",
            (_, BgAnimStyle.Network) => "Knotennetzwerk",
            (_, BgAnimStyle.Bokeh) => "Bokeh",
            (_, BgAnimStyle.Aurora) => "Polarlicht",
            (_, _) => "Sternenhimmel"
        };

        public static string SidebarPosLabel => Current switch
        {
            Lang.EL => "Πλευρική μπάρα:",
            Lang.EN => "Sidebar:",
            Lang.FR => "Barre latérale :",
            _ => "Seitenleiste:"
        };

        public static string SidebarPosButtonText(bool right) => (Current, right) switch
        {
            (Lang.EL, false) => "⇄ Αριστερά (πάτησε για Δεξιά)",
            (Lang.EL, true) => "⇄ Δεξιά (πάτησε για Αριστερά)",
            (Lang.EN, false) => "⇄ Left (click for Right)",
            (Lang.EN, true) => "⇄ Right (click for Left)",
            (Lang.FR, false) => "⇄ Gauche (clique pour Droite)",
            (Lang.FR, true) => "⇄ Droite (clique pour Gauche)",
            (_, false) => "⇄ Links (klicken für Rechts)",
            (_, true) => "⇄ Rechts (klicken für Links)"
        };

        public static string ChatTextScaleLabel => Current switch
        {
            Lang.EL => "Μέγεθος κειμένου συνομιλίας:",
            Lang.EN => "Chat text size:",
            Lang.FR => "Taille du texte du chat :",
            _ => "Textgröße im Chat:"
        };

        public static string ChatTextScaleButtonText(ChatTextScale scale) => (Current, scale) switch
        {
            (Lang.EL, Kombos.Models.ChatTextScale.Small) => "Αα Μικρό (πάτησε για Κανονικό)",
            (Lang.EL, Kombos.Models.ChatTextScale.Large) => "Αα Μεγάλο (πάτησε για Μικρό)",
            (Lang.EL, _) => "Αα Κανονικό (πάτησε για Μεγάλο)",
            (Lang.EN, Kombos.Models.ChatTextScale.Small) => "Aa Small (click for Normal)",
            (Lang.EN, Kombos.Models.ChatTextScale.Large) => "Aa Large (click for Small)",
            (Lang.EN, _) => "Aa Normal (click for Large)",
            (Lang.FR, Kombos.Models.ChatTextScale.Small) => "Aa Petit (clique pour Normal)",
            (Lang.FR, Kombos.Models.ChatTextScale.Large) => "Aa Grand (clique pour Petit)",
            (Lang.FR, _) => "Aa Normal (clique pour Grand)",
            (_, Kombos.Models.ChatTextScale.Small) => "Aa Klein (klicken für Normal)",
            (_, Kombos.Models.ChatTextScale.Large) => "Aa Groß (klicken für Klein)",
            (_, _) => "Aa Normal (klicken für Groß)"
        };

        public static string ImportProjectButton => Current switch
        {
            Lang.EL => "📂 Εισαγωγή αρχείου ↗",
            Lang.EN => "📂 Import file ↗",
            Lang.FR => "📂 Importer un fichier ↗",
            _ => "📂 Datei importieren ↗"
        };

        public static string ImportFolderButton => Current switch
        {
            Lang.EL => "📁 Εισαγωγή φακέλου (project) ↗",
            Lang.EN => "📁 Import folder (project) ↗",
            Lang.FR => "📁 Importer un dossier (projet) ↗",
            _ => "📁 Ordner importieren (Projekt) ↗"
        };

        public static string ImportProjectHint => Current switch
        {
            Lang.EL => "Δεν υπάρχει δημόσιο API για \"Claude Projects\" — εισάγει τοπικό αρχείο ή ολόκληρο φάκελο ως πλαίσιο στη συζήτηση, χωρίς όριο αριθμού αρχείων (με ένα εύλογο όριο συνολικού μεγέθους).",
            Lang.EN => "There's no public API for \"Claude Projects\" — this imports a local file or a whole folder as context into the chat, with no file-count limit (a reasonable total size cap still applies).",
            Lang.FR => "Il n'existe pas d'API publique pour « Claude Projects » — ceci importe un fichier local ou un dossier entier comme contexte dans le chat, sans limite de nombre de fichiers (une limite de taille totale raisonnable s'applique).",
            _ => "Es gibt keine öffentliche API für „Claude Projects“ — dies importiert eine lokale Datei oder einen ganzen Ordner als Kontext in den Chat, ohne Dateianzahl-Limit (eine angemessene Gesamtgrößenbegrenzung gilt weiterhin)."
        };

        public static string ImportedFolderPrefix(string folderName, int fileCount) => Current switch
        {
            Lang.EL => $"[Εισαγωγή φακέλου: {folderName} — {fileCount} αρχεία]\n\n",
            Lang.EN => $"[Imported folder: {folderName} — {fileCount} files]\n\n",
            Lang.FR => $"[Dossier importé : {folderName} — {fileCount} fichiers]\n\n",
            _ => $"[Importierter Ordner: {folderName} — {fileCount} Dateien]\n\n"
        };

        public static string ImportFolderEmpty => Current switch
        {
            Lang.EL => "Δεν βρέθηκαν αναγνώσιμα αρχεία κειμένου/κώδικα σε αυτόν τον φάκελο.",
            Lang.EN => "No readable text/code files were found in that folder.",
            Lang.FR => "Aucun fichier texte/code lisible n'a été trouvé dans ce dossier.",
            _ => "In diesem Ordner wurden keine lesbaren Text-/Code-Dateien gefunden."
        };

        public static string ProjectMemoryLabel => Current switch
        {
            Lang.EL => "🧠 Μνήμη Project (v3.6.0)",
            Lang.EN => "🧠 Project Memory (v3.6.0)",
            Lang.FR => "🧠 Mémoire de projet (v3.6.0)",
            _ => "🧠 Projektgedächtnis (v3.6.0)"
        };

        public static string ProjectMemoryHint => Current switch
        {
            Lang.EL => "Αντί να στέλνεται ολόκληρο το περιεχόμενο κάθε αρχείου, κρατάει τοπικά ένα ευρετήριο (δέντρο + hash ανά αρχείο) και φέρνει στοχευμένα μόνο ό,τι χρειάζεται με /readfile. Το /verify τρέχει τοπικά το build/lint του project, χωρίς κόστος tokens.",
            Lang.EN => "Instead of sending every file's full content, keeps a local index (tree + per-file hash) and pulls in only what's needed via /readfile. /verify runs the project's build/lint locally, at zero token cost.",
            Lang.FR => "Au lieu d'envoyer le contenu complet de chaque fichier, conserve un index local (arborescence + hash par fichier) et ne récupère que ce qui est nécessaire via /readfile. /verify exécute le build/lint du projet localement, sans coût en tokens.",
            _ => "Anstatt den vollständigen Inhalt jeder Datei zu senden, wird ein lokaler Index (Baum + Hash pro Datei) geführt und nur das Nötige per /readfile geholt. /verify führt den Build/Lint des Projekts lokal aus, ohne Token-Kosten."
        };

        public static string ImportProjectIndexButton => Current switch
        {
            Lang.EL => "🧠 Εισαγωγή project (ευρετήριο, όχι πλήρες περιεχόμενο)",
            Lang.EN => "🧠 Import project (index, not full content)",
            Lang.FR => "🧠 Importer un projet (index, pas le contenu complet)",
            _ => "🧠 Projekt importieren (Index, nicht vollständiger Inhalt)"
        };

        public static string VerifyProjectButton => Current switch
        {
            Lang.EL => "✅ Τοπική επαλήθευση συμβατότητας (/verify)",
            Lang.EN => "✅ Local compatibility check (/verify)",
            Lang.FR => "✅ Vérification locale de compatibilité (/verify)",
            _ => "✅ Lokale Kompatibilitätsprüfung (/verify)"
        };

        public static string ActiveProjectStatus(string path) => Current switch
        {
            Lang.EL => $"Ενεργό project: {path}",
            Lang.EN => $"Active project: {path}",
            Lang.FR => $"Projet actif : {path}",
            _ => $"Aktives Projekt: {path}"
        };

        public static string ProjectsSectionLabel => Current switch
        {
            Lang.EL => "📁 PROJECTS",
            Lang.EN => "📁 PROJECTS",
            Lang.FR => "📁 PROJETS",
            _ => "📁 PROJEKTE"
        };

        public static string ProjectsAllChip => Current switch
        {
            Lang.EL => "Όλες",
            Lang.EN => "All",
            Lang.FR => "Toutes",
            _ => "Alle"
        };

        public static string ProjectsNewButton => Current switch
        {
            Lang.EL => "+ Νέο Project",
            Lang.EN => "+ New Project",
            Lang.FR => "+ Nouveau projet",
            _ => "+ Neues Projekt"
        };

        public static string SharedProjectsLabel => Current switch
        {
            Lang.EL => "🗂 Κοινόχρηστα Projects (μεταξύ υπολογιστών)",
            Lang.EN => "🗂 Shared Projects (across computers)",
            Lang.FR => "🗂 Projets partagés (entre ordinateurs)",
            _ => "🗂 Geteilte Projekte (computerübergreifend)"
        };

        public static string SharedProjectsHint => Current switch
        {
            Lang.EL => "Δεν υπάρχει δημόσιο API για τα \"Projects\" του claude.ai - αυτό εδώ είναι κάτι πραγματικό: τα δικά σου Project Memory projects, δημοσιευμένα σαν ZIP μέσα στον φάκελο συγχρονισμού από πάνω. Κάθε υπολογιστής με τον ίδιο φάκελο βλέπει τη λίστα και μπορεί να κατεβάσει το ZIP.",
            Lang.EN => "There's no public API for claude.ai's \"Projects\" - this is something real instead: your own Project Memory projects, published as a ZIP inside the sync folder above. Every computer with the same folder sees the list and can download the ZIP.",
            Lang.FR => "Il n'existe pas d'API publique pour les « Projects » de claude.ai - voici quelque chose de réel à la place : tes propres projets Project Memory, publiés sous forme de ZIP dans le dossier de synchronisation ci-dessus. Chaque ordinateur avec le même dossier voit la liste et peut télécharger le ZIP.",
            _ => "Es gibt keine öffentliche API für die „Projects“ von claude.ai - hier ist stattdessen etwas Reales: deine eigenen Project-Memory-Projekte, veröffentlicht als ZIP im obigen Sync-Ordner. Jeder Computer mit demselben Ordner sieht die Liste und kann das ZIP herunterladen."
        };

        public static string PublishActiveProjectButton => Current switch
        {
            Lang.EL => "📤 Δημοσίευση ενεργού project",
            Lang.EN => "📤 Publish active project",
            Lang.FR => "📤 Publier le projet actif",
            _ => "📤 Aktives Projekt veröffentlichen"
        };

        public static string SharedProjectsNoSyncFolder => Current switch
        {
            Lang.EL => "Όρισε πρώτα φάκελο συγχρονισμού από πάνω για να χρησιμοποιήσεις τα κοινόχρηστα projects.",
            Lang.EN => "Set a sync folder above first to use shared projects.",
            Lang.FR => "Définis d'abord un dossier de synchronisation ci-dessus pour utiliser les projets partagés.",
            _ => "Lege oben zuerst einen Sync-Ordner fest, um geteilte Projekte zu nutzen."
        };

        public static string SharedProjectsEmpty => Current switch
        {
            Lang.EL => "Δεν υπάρχουν ακόμα κοινόχρηστα projects. Πάτησε \"Δημοσίευση ενεργού project\" για να προσθέσεις το τρέχον.",
            Lang.EN => "No shared projects yet. Click \"Publish active project\" to add the current one.",
            Lang.FR => "Aucun projet partagé pour l'instant. Clique sur « Publier le projet actif » pour ajouter le projet actuel.",
            _ => "Noch keine geteilten Projekte. Klicke auf „Aktives Projekt veröffentlichen“, um das aktuelle hinzuzufügen."
        };

        public static string SharedProjectStatusLabel(string status) => (Current, status) switch
        {
            (Lang.EL, "Completed") => "Ολοκληρώθηκε",
            (Lang.EL, "InProgress") => "Σε εξέλιξη",
            (Lang.EL, _) => "Εκκρεμεί",
            (Lang.EN, "Completed") => "Completed",
            (Lang.EN, "InProgress") => "In progress",
            (Lang.EN, _) => "Pending",
            (Lang.FR, "Completed") => "Terminé",
            (Lang.FR, "InProgress") => "En cours",
            (Lang.FR, _) => "En attente",
            (_, "Completed") => "Abgeschlossen",
            (_, "InProgress") => "In Bearbeitung",
            (_, _) => "Ausstehend"
        };

        public static string SharedProjectMeta(string machine, int fileCount, DateTime lastUpdated) => Current switch
        {
            Lang.EL => $"Από: {machine} · {fileCount} αρχεία · {lastUpdated:dd/MM/yyyy HH:mm}",
            Lang.EN => $"From: {machine} · {fileCount} files · {lastUpdated:yyyy-MM-dd HH:mm}",
            Lang.FR => $"De : {machine} · {fileCount} fichiers · {lastUpdated:dd/MM/yyyy HH:mm}",
            _ => $"Von: {machine} · {fileCount} Dateien · {lastUpdated:dd.MM.yyyy HH:mm}"
        };

        public static string SharedProjectDownloadButton => Current switch
        {
            Lang.EL => "📥 Λήψη ZIP",
            Lang.EN => "📥 Download ZIP",
            Lang.FR => "📥 Télécharger le ZIP",
            _ => "📥 ZIP herunterladen"
        };

        public static string SharedProjectDownloaded(string path) => Current switch
        {
            Lang.EL => $"✅ Αποθηκεύτηκε: {path}",
            Lang.EN => $"✅ Saved: {path}",
            Lang.FR => $"✅ Enregistré : {path}",
            _ => $"✅ Gespeichert: {path}"
        };

        public static string ImportedProjectIndexPrefix(string folderName, int fileCount) => Current switch
        {
            Lang.EL => $"[Ενεργό project: {folderName} — ευρετήριο {fileCount} αρχείων (μόνο δέντρο, όχι περιεχόμενο). Ζήτησε /readfile <σχετική διαδρομή> για συγκεκριμένο αρχείο.]\n\n",
            Lang.EN => $"[Active project: {folderName} — index of {fileCount} files (tree only, no content). Ask for /readfile <relative path> for a specific file.]\n\n",
            Lang.FR => $"[Projet actif : {folderName} — index de {fileCount} fichiers (arborescence seulement, pas de contenu). Demande /readfile <chemin relatif> pour un fichier précis.]\n\n",
            _ => $"[Aktives Projekt: {folderName} — Index von {fileCount} Dateien (nur Baum, kein Inhalt). Verwende /readfile <relativer Pfad> für eine bestimmte Datei.]\n\n"
        };

        public static string NoActiveProject => Current switch
        {
            Lang.EL => "Δεν υπάρχει ενεργό project. Κάνε πρώτα εισαγωγή φακέλου project από Ρυθμίσεις → Project.",
            Lang.EN => "No active project. First import a project folder from Settings → Project.",
            Lang.FR => "Aucun projet actif. Importe d'abord un dossier de projet depuis Paramètres → Projet.",
            _ => "Kein aktives Projekt. Importiere zuerst einen Projektordner unter Einstellungen → Projekt."
        };

        /// <summary>/search (v4.5.0) - τοπική αναζήτηση λέξεων-κλειδιών σε ΟΛΟ το περιεχόμενο του ενεργού
        /// project ("local RAG-lite", βλ. MainWindow.SearchProjectAsync) - χωρίς embeddings/vector DB, χωρίς
        /// καμία κλήση AI για την ίδια την αναζήτηση.</summary>
        public static string SearchUsage => Current switch
        {
            Lang.EL => "Χρήση: /search <λέξεις-κλειδιά> - ψάχνει ΟΛΟ το περιεχόμενο του ενεργού project τοπικά, μηδενικό κόστος tokens.",
            Lang.EN => "Usage: /search <keywords> - searches the active project's full content locally, zero token cost.",
            Lang.FR => "Utilisation : /search <mots-clés> - recherche dans tout le contenu du projet actif localement, coût en tokens nul.",
            _ => "Verwendung: /search <Stichwörter> - durchsucht den gesamten Inhalt des aktiven Projekts lokal, keine Tokenkosten."
        };

        public static string SearchNoResults(string query) => Current switch
        {
            Lang.EL => $"Καμία αντιστοιχία για «{query}» στο ενεργό project.",
            Lang.EN => $"No matches for \"{query}\" in the active project.",
            Lang.FR => $"Aucune correspondance pour « {query} » dans le projet actif.",
            _ => $"Keine Treffer für \"{query}\" im aktiven Projekt."
        };

        public static string SearchResultsHeader(string query) => Current switch
        {
            Lang.EL => $"🔎 Αποτελέσματα για «{query}» (τοπική αναζήτηση, μηδενικό κόστος tokens):",
            Lang.EN => $"🔎 Results for \"{query}\" (local search, zero token cost):",
            Lang.FR => $"🔎 Résultats pour « {query} » (recherche locale, coût en tokens nul) :",
            _ => $"🔎 Ergebnisse für \"{query}\" (lokale Suche, keine Tokenkosten):"
        };

        public static string SearchHint => Current switch
        {
            Lang.EL => "Χρησιμοποίησε /readfile <διαδρομή> για να φέρεις ολόκληρο ένα από αυτά τα αρχεία στη συζήτηση.",
            Lang.EN => "Use /readfile <path> to bring one of these files fully into the conversation.",
            Lang.FR => "Utilise /readfile <chemin> pour apporter l'un de ces fichiers en entier dans la conversation.",
            _ => "Verwende /readfile <Pfad>, um eine dieser Dateien vollständig in die Unterhaltung zu holen."
        };

        /// <summary>/image (v4.5.0) - πραγματική δημιουργία εικόνας μέσα στη συνομιλία (OpenAI Images API),
        /// βλ. MainWindow.GenerateImageAsync.</summary>
        public static string ImageUsage => Current switch
        {
            Lang.EL => "Χρήση: /image <περιγραφή εικόνας> - δημιουργεί πραγματική εικόνα μέσα στη συνομιλία (μέσω OpenAI, χρειάζεται κλειδί ChatGPT).",
            Lang.EN => "Usage: /image <image description> - generates a real image inside the conversation (via OpenAI, needs a ChatGPT key).",
            Lang.FR => "Utilisation : /image <description de l'image> - génère une vraie image dans la conversation (via OpenAI, nécessite une clé ChatGPT).",
            _ => "Verwendung: /image <Bildbeschreibung> - erzeugt ein echtes Bild in der Unterhaltung (via OpenAI, benötigt einen ChatGPT-Schlüssel)."
        };

        public static string ImageNoKey => Current switch
        {
            Lang.EL => "Χρειάζεται κλειδί API για το ChatGPT (Ρυθμίσεις Agent → ChatGPT) για να δημιουργηθούν εικόνες.",
            Lang.EN => "Needs a ChatGPT API key (Agent settings → ChatGPT) to generate images.",
            Lang.FR => "Nécessite une clé API ChatGPT (Réglages Agent → ChatGPT) pour générer des images.",
            _ => "Benötigt einen ChatGPT-API-Schlüssel (Agent-Einstellungen → ChatGPT), um Bilder zu erzeugen."
        };

        public static string ReadFileUsage => Current switch
        {
            Lang.EL => "Χρήση: /readfile <σχετική διαδρομή μέσα στο ενεργό project>",
            Lang.EN => "Usage: /readfile <relative path inside the active project>",
            Lang.FR => "Utilisation : /readfile <chemin relatif dans le projet actif>",
            _ => "Verwendung: /readfile <relativer Pfad im aktiven Projekt>"
        };

        public static string ReadFileOutsideProject => Current switch
        {
            Lang.EL => "❌ Η διαδρομή είναι εκτός του ενεργού project.",
            Lang.EN => "❌ That path is outside the active project.",
            Lang.FR => "❌ Ce chemin est en dehors du projet actif.",
            _ => "❌ Dieser Pfad liegt außerhalb des aktiven Projekts."
        };

        public static string ReadFileAdded(string relativePath) => Current switch
        {
            Lang.EL => $"✅ Προστέθηκε στη συζήτηση: {relativePath}",
            Lang.EN => $"✅ Added to the conversation: {relativePath}",
            Lang.FR => $"✅ Ajouté à la conversation : {relativePath}",
            _ => $"✅ Zur Unterhaltung hinzugefügt: {relativePath}"
        };

        public static string VerifyCouldNotRun(string reason) => Current switch
        {
            Lang.EL => $"⚠️ Δεν έτρεξε ο τοπικός έλεγχος: {reason}",
            Lang.EN => $"⚠️ Local check didn't run: {reason}",
            Lang.FR => $"⚠️ La vérification locale n'a pas été exécutée : {reason}",
            _ => $"⚠️ Lokale Prüfung wurde nicht ausgeführt: {reason}"
        };

        public static string VerifySuccess(string command) => Current switch
        {
            Lang.EL => $"✅ Επιτυχής τοπικός έλεγχος ({command}) - μηδενικό κόστος tokens.",
            Lang.EN => $"✅ Local check passed ({command}) - zero token cost.",
            Lang.FR => $"✅ Vérification locale réussie ({command}) - coût en tokens nul.",
            _ => $"✅ Lokale Prüfung erfolgreich ({command}) - keine Token-Kosten."
        };

        public static string VerifyFailed(string command) => Current switch
        {
            Lang.EL => $"❌ Ο τοπικός έλεγχος απέτυχε ({command}):",
            Lang.EN => $"❌ Local check failed ({command}):",
            Lang.FR => $"❌ La vérification locale a échoué ({command}) :",
            _ => $"❌ Lokale Prüfung fehlgeschlagen ({command}):"
        };

        public static string ExportChatButton => Current switch
        {
            Lang.EL => "💾 Εξαγωγή συζήτησης ↗",
            Lang.EN => "💾 Export chat ↗",
            Lang.FR => "💾 Exporter la conversation ↗",
            _ => "💾 Chat exportieren ↗"
        };

        public static string ExportBackupButton => Current switch
        {
            Lang.EL => "📤 Εξαγωγή αντιγράφου ασφαλείας (JSON)",
            Lang.EN => "📤 Export backup (JSON)",
            Lang.FR => "📤 Exporter une sauvegarde (JSON)",
            _ => "📤 Backup exportieren (JSON)"
        };

        public static string ImportBackupButton => Current switch
        {
            Lang.EL => "📥 Εισαγωγή αντιγράφου ασφαλείας (JSON)",
            Lang.EN => "📥 Import backup (JSON)",
            Lang.FR => "📥 Importer une sauvegarde (JSON)",
            _ => "📥 Backup importieren (JSON)"
        };

        public static string ImportBackupEmpty => Current switch
        {
            Lang.EL => "Το αρχείο δεν περιέχει έγκυρες συζητήσεις.",
            Lang.EN => "The file doesn't contain any valid conversations.",
            Lang.FR => "Le fichier ne contient aucune conversation valide.",
            _ => "Die Datei enthält keine gültigen Chats."
        };

        public static string ImportBackupSuccess(int count) => Current switch
        {
            Lang.EL => $"Εισήχθησαν {count} συζητήσεις με επιτυχία.",
            Lang.EN => $"Successfully imported {count} conversation(s).",
            Lang.FR => $"{count} conversation(s) importée(s) avec succès.",
            _ => $"{count} Chat(s) erfolgreich importiert."
        };

        /// <summary>v4.5.0 - εισαγωγή από αρχεία εξαγωγής ChatGPT/Claude.ai (conversations.json από το "Data
        /// export" της κάθε υπηρεσίας), όχι από το δικό μας formats του ImportBackupButton παραπάνω.</summary>
        public static string ImportChatExportButton => Current switch
        {
            Lang.EL => "📂 Εισαγωγή από ChatGPT / Claude.ai (export)",
            Lang.EN => "📂 Import from ChatGPT / Claude.ai (export)",
            Lang.FR => "📂 Importer depuis ChatGPT / Claude.ai (export)",
            _ => "📂 Import von ChatGPT / Claude.ai (Export)"
        };

        public static string ImportChatExportHint => Current switch
        {
            Lang.EL => "Ζήτησε \"Data export\" από τις ρυθμίσεις του ChatGPT ή του Claude.ai, βρες το conversations.json μέσα στο ZIP, και επίλεξέ το εδώ.",
            Lang.EN => "Request a \"Data export\" from ChatGPT's or Claude.ai's settings, find conversations.json inside the ZIP, and select it here.",
            Lang.FR => "Demande un « Data export » depuis les paramètres de ChatGPT ou Claude.ai, trouve conversations.json dans le ZIP, et sélectionne-le ici.",
            _ => "Fordere einen \"Data Export\" in den Einstellungen von ChatGPT oder Claude.ai an, finde conversations.json im ZIP und wähle sie hier aus."
        };

        public static string ImportChatExportEmpty => Current switch
        {
            Lang.EL => "Δεν βρέθηκαν αναγνωρίσιμες συζητήσεις σε αυτό το αρχείο (αναμένεται conversations.json από ChatGPT ή Claude.ai).",
            Lang.EN => "No recognizable conversations found in this file (expected a conversations.json from ChatGPT or Claude.ai).",
            Lang.FR => "Aucune conversation reconnaissable trouvée dans ce fichier (un conversations.json de ChatGPT ou Claude.ai est attendu).",
            _ => "Keine erkennbaren Chats in dieser Datei gefunden (erwartet wird eine conversations.json von ChatGPT oder Claude.ai)."
        };

        public static string CommandPaletteHint => Current switch
        {
            Lang.EL => "↑↓ πλοήγηση · Enter επιλογή · Esc κλείσιμο",
            Lang.EN => "↑↓ navigate · Enter select · Esc close",
            Lang.FR => "↑↓ naviguer · Entrée sélectionner · Échap fermer",
            _ => "↑↓ navigieren · Enter auswählen · Esc schließen"
        };

        public static string CommandPaletteCategoryTool => Current switch
        {
            Lang.EL => "Εργαλείο",
            Lang.EN => "Tool",
            Lang.FR => "Outil",
            _ => "Tool"
        };

        public static string CommandPaletteCategoryCommand => Current switch
        {
            Lang.EL => "Εντολή",
            Lang.EN => "Command",
            Lang.FR => "Commande",
            _ => "Befehl"
        };

        public static string CommandPaletteCategoryAction => Current switch
        {
            Lang.EL => "Ενέργεια",
            Lang.EN => "Action",
            Lang.FR => "Action",
            _ => "Aktion"
        };

        /// <summary>Σύντομες ετικέτες πλευρικού μενού (v4.7.0) - ορατές μόνο όταν το sidebar είναι
        /// "αναπτυγμένο" (κείμενο + εικονίδιο), κρύβονται στην "διπλωμένη" κατάσταση (μόνο εικονίδιο) - βλ.
        /// MainWindow.SidebarCollapseButton_Click.</summary>
        public static string SidebarThemeLabel => Current switch
        {
            Lang.EL => "Θέμα",
            Lang.EN => "Theme",
            Lang.FR => "Thème",
            _ => "Design"
        };

        public static string SidebarLangLabel => Current switch
        {
            Lang.EL => "Γλώσσα",
            Lang.EN => "Language",
            Lang.FR => "Langue",
            _ => "Sprache"
        };

        public static string SidebarAppearanceLabel => Current switch
        {
            Lang.EL => "Εμφάνιση",
            Lang.EN => "Appearance",
            Lang.FR => "Apparence",
            _ => "Aussehen"
        };

        public static string SidebarArtifactsLabel => Current switch
        {
            Lang.EL => "Artifacts",
            Lang.EN => "Artifacts",
            Lang.FR => "Artefacts",
            _ => "Artifacts"
        };

        public static string SidebarProjectsLabel => Current switch
        {
            Lang.EL => "Projects",
            Lang.EN => "Projects",
            Lang.FR => "Projets",
            _ => "Projekte"
        };

        public static string SidebarSettingsLabel => Current switch
        {
            Lang.EL => "Ρυθμίσεις",
            Lang.EN => "Settings",
            Lang.FR => "Réglages",
            _ => "Einstellungen"
        };

        public static string ThemeToggleLabel => Current switch
        {
            Lang.EL => "Εναλλαγή Light / Dark mode",
            Lang.EN => "Toggle Light / Dark mode",
            Lang.FR => "Basculer mode clair / sombre",
            _ => "Hell-/Dunkelmodus umschalten"
        };

        public static string ArtifactsTitle => Current switch
        {
            Lang.EL => "Artifacts (μπλοκ κώδικα)",
            Lang.EN => "Artifacts (code blocks)",
            Lang.FR => "Artifacts (blocs de code)",
            _ => "Artifacts (Codeblöcke)"
        };

        public static string ArtifactsEmpty => Current switch
        {
            Lang.EL => "Δεν υπάρχουν ακόμα μπλοκ κώδικα σε καμία συζήτηση.",
            Lang.EN => "No code blocks in any conversation yet.",
            Lang.FR => "Aucun bloc de code dans une conversation pour l'instant.",
            _ => "Noch keine Codeblöcke in irgendeinem Chat."
        };

        public static string ArtifactCurrentChat => Current switch
        {
            Lang.EL => "Τρέχουσα συζήτηση",
            Lang.EN => "Current chat",
            Lang.FR => "Conversation actuelle",
            _ => "Aktueller Chat"
        };

        public static string ArtifactSaveAs => Current switch
        {
            Lang.EL => "💾 Αποθήκευση ως",
            Lang.EN => "💾 Save as",
            Lang.FR => "💾 Enregistrer sous",
            _ => "💾 Speichern unter"
        };

        public static string SyncFolderLabel => Current switch
        {
            Lang.EL => "Φάκελος συγχρονισμού συζητήσεων (προαιρετικό):",
            Lang.EN => "Conversations sync folder (optional):",
            Lang.FR => "Dossier de synchronisation des conversations (facultatif) :",
            _ => "Sync-Ordner für Chats (optional):"
        };

        public static string SyncFolderHint => Current switch
        {
            Lang.EL => "Διάλεξε έναν φάκελο OneDrive/Dropbox/δικτύου για να μοιράζεσαι τις αποθηκευμένες συζητήσεις σου ανάμεσα σε υπολογιστές - χωρίς merge, το τελευταίο αποθηκευμένο κερδίζει.",
            Lang.EN => "Pick a OneDrive/Dropbox/network folder to share your saved conversations across computers - no merging, the last save wins.",
            Lang.FR => "Choisis un dossier OneDrive/Dropbox/réseau pour partager tes conversations enregistrées entre ordinateurs - pas de fusion, la dernière sauvegarde l'emporte.",
            _ => "Wähle einen OneDrive-/Dropbox-/Netzwerkordner, um deine gespeicherten Chats zwischen Computern zu teilen - kein Zusammenführen, der letzte Speicherstand gewinnt."
        };

        public static string SyncFolderDefault => Current switch
        {
            Lang.EL => "(προεπιλογή - τοπικά σε αυτόν τον υπολογιστή)",
            Lang.EN => "(default - local to this computer)",
            Lang.FR => "(par défaut - local à cet ordinateur)",
            _ => "(Standard - lokal auf diesem Computer)"
        };

        public static string ClearAllConversationsButton => Current switch
        {
            Lang.EL => "🗑 Διαγραφή όλων των αποθηκευμένων συζητήσεων",
            Lang.EN => "🗑 Delete all saved conversations",
            Lang.FR => "🗑 Supprimer toutes les conversations enregistrées",
            _ => "🗑 Alle gespeicherten Chats löschen"
        };

        public static string ClearAllConfirmTitle => Current switch
        {
            Lang.EL => "Διαγραφή όλων των συζητήσεων",
            Lang.EN => "Delete all conversations",
            Lang.FR => "Supprimer toutes les conversations",
            _ => "Alle Chats löschen"
        };

        public static string ClearAllConfirmMessage => Current switch
        {
            Lang.EL => "Θα διαγραφούν ΟΡΙΣΤΙΚΑ όλες οι αποθηκευμένες συζητήσεις. Η τρέχουσα ανοιχτή συζήτηση δεν επηρεάζεται. Θέλεις σίγουρα να συνεχίσεις;",
            Lang.EN => "This will PERMANENTLY delete every saved conversation. The conversation currently open is not affected. Are you sure you want to continue?",
            Lang.FR => "Toutes les conversations enregistrées seront supprimées DÉFINITIVEMENT. La conversation actuellement ouverte n'est pas affectée. Veux-tu vraiment continuer ?",
            _ => "Dadurch werden ALLE gespeicherten Chats endgültig gelöscht. Der aktuell geöffnete Chat ist davon nicht betroffen. Möchtest du wirklich fortfahren?"
        };

        public static string ExportChatEmpty => Current switch
        {
            Lang.EL => "Δεν υπάρχει ακόμα συζήτηση για εξαγωγή.",
            Lang.EN => "There's no conversation yet to export.",
            Lang.FR => "Il n'y a pas encore de conversation à exporter.",
            _ => "Es gibt noch kein Gespräch zum Exportieren."
        };

        public static string ImportedFilePrefix(string fileName) => Current switch
        {
            Lang.EL => $"[Εισαγωγή από αρχείο: {fileName}]\n\n",
            Lang.EN => $"[Imported from file: {fileName}]\n\n",
            Lang.FR => $"[Importé depuis le fichier : {fileName}]\n\n",
            _ => $"[Importiert aus Datei: {fileName}]\n\n"
        };

        public static string ClaudeTokens(long used, long budget) => $"Claude: {used:N0} / {budget:N0} tokens";

        public static string CombinedTokens(long combined) => Current switch
        {
            Lang.EL => $"Σύνολο (όλα τα εργαλεία): {combined:N0} tokens",
            Lang.EN => $"Total (all tools): {combined:N0} tokens",
            Lang.FR => $"Total (tous les outils) : {combined:N0} tokens",
            _ => $"Gesamt (alle Tools): {combined:N0} Tokens"
        };

        public static string SomethingWentWrong(string msg) => Current switch
        {
            Lang.EL => $"Κάτι πήγε στραβά: {msg}",
            Lang.EN => $"Something went wrong: {msg}",
            Lang.FR => $"Un problème est survenu : {msg}",
            _ => $"Etwas ist schiefgelaufen: {msg}"
        };

        public static string ToolsCountSuffix(int n) => Current switch
        {
            Lang.EL => $"  ·  {n} εργαλεία",
            Lang.EN => $"  ·  {n} tools",
            Lang.FR => $"  ·  {n} outils",
            _ => $"  ·  {n} Tools"
        };

        public static string SystemPrompt(string catalogJson, string agentName, string customInstructions = "")
        {
            var (intro, jsonNote) = Current switch
            {
                Lang.EL => (
                    $"Είσαι ο {agentName}, ο κεντρικός AI agent αυτής της desktop εφαρμογής (Κόμβος). Μιλάς φυσικά, σύντομα και " +
                    "βοηθητικά στα ελληνικά. Έχεις μια ομάδα εξειδικευμένων εργαλείων/agents (catalog παρακάτω, JSON) — " +
                    "όταν ένα task χρειάζεται κάτι εξειδικευμένο, τα φέρνεις μέσα στην ίδια απάντηση. Το πεδίο mcp:true " +
                    "σημαίνει ότι το εργαλείο έχει επίσημο MCP server και δουλεύει απευθείας μέσα στο Claude Desktop.",
                    "Απάντα ΠΑΝΤΑ ΜΟΝΟ με JSON object, χωρίς markdown, ακριβώς της μορφής:\n" +
                    "{\"reply\": \"η απάντησή σου στα ελληνικά, 1-4 προτάσεις\", \"tools\": [{\"tool\": \"Ακριβές όνομα από catalog\", \"why\": \"1 πρόταση\"}]}\n" +
                    "Το tools array μπορεί να είναι κενό []. Το πολύ 5 εργαλεία, μόνο από το catalog."),
                Lang.EN => (
                    $"You are {agentName}, the central AI agent of this desktop app (Kombos). Speak naturally, briefly and helpfully in English. " +
                    "You have a team of specialized tools/agents (catalog below, JSON) — when a task needs something specialized, bring them into the same reply. " +
                    "The field mcp:true means the tool has an official MCP server and works directly inside Claude Desktop.",
                    "ALWAYS reply ONLY with a JSON object, no markdown, in exactly this shape:\n" +
                    "{\"reply\": \"your reply in English, 1-4 sentences\", \"tools\": [{\"tool\": \"Exact name from catalog\", \"why\": \"1 sentence\"}]}\n" +
                    "The tools array can be empty []. At most 5 tools, only from the catalog."),
                Lang.FR => (
                    $"Tu es {agentName}, l'agent IA central de cette application de bureau (Kombos). Parle naturellement, brièvement et " +
                    "utilement en français. Tu as une équipe d'outils/agents spécialisés (catalogue ci-dessous, JSON) — quand une tâche " +
                    "nécessite quelque chose de spécialisé, intègre-les dans la même réponse. Le champ mcp:true signifie que l'outil a un " +
                    "serveur MCP officiel et fonctionne directement dans Claude Desktop.",
                    "Réponds TOUJOURS UNIQUEMENT avec un objet JSON, sans markdown, exactement sous cette forme :\n" +
                    "{\"reply\": \"ta réponse en français, 1-4 phrases\", \"tools\": [{\"tool\": \"Nom exact du catalogue\", \"why\": \"1 phrase\"}]}\n" +
                    "Le tableau tools peut être vide []. Au plus 5 outils, uniquement du catalogue."),
                _ => (
                    $"Du bist {agentName}, der zentrale KI-Agent dieser Desktop-App (Kombos). Sprich natürlich, kurz und hilfreich auf Deutsch. " +
                    "Du hast ein Team spezialisierter Tools/Agenten (Katalog unten, JSON) — wenn eine Aufgabe etwas Spezielles erfordert, " +
                    "bringe sie in derselben Antwort ein. Das Feld mcp:true bedeutet, dass das Tool einen offiziellen MCP-Server hat und " +
                    "direkt in Claude Desktop funktioniert.",
                    "Antworte IMMER NUR mit einem JSON-Objekt, ohne Markdown, genau in dieser Form:\n" +
                    "{\"reply\": \"deine Antwort auf Deutsch, 1-4 Sätze\", \"tools\": [{\"tool\": \"Exakter Name aus dem Katalog\", \"why\": \"1 Satz\"}]}\n" +
                    "Das tools-Array kann leer sein []. Höchstens 5 Tools, nur aus dem Katalog.")
            };

            var customBlock = string.IsNullOrWhiteSpace(customInstructions) ? "" : Current switch
            {
                Lang.EL => $"\n\nΠρόσθετες οδηγίες από τον χρήστη (ακολούθησέ τες όσο δεν έρχονται σε αντίθεση με τα παραπάνω): {customInstructions}",
                Lang.EN => $"\n\nAdditional instructions from the user (follow them as long as they don't conflict with the above): {customInstructions}",
                Lang.FR => $"\n\nInstructions supplémentaires de l'utilisateur (suis-les tant qu'elles ne contredisent pas ce qui précède) : {customInstructions}",
                _ => $"\n\nZusätzliche Anweisungen des Nutzers (befolge sie, solange sie dem oben Genannten nicht widersprechen): {customInstructions}"
            };

            return intro + customBlock + "\n\ncatalog: " + catalogJson + "\n\n" + jsonNote;
        }
    }
}
