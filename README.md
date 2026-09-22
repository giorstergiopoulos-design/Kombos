# Κόμβος (Kombos)

**A local Windows hub for AI tools, with Claude as the agent that ties them together.**
WPF desktop app (.NET 8) — chat with Claude (or another central agent), browse a curated catalog of 39+ AI tools by category, and extend the app itself with plugins. Greek-first UI with full English/French/German translations.

---

## Τι είναι το Κόμβος

Το Κόμβος είναι ένας τοπικός, desktop κόμβος (hub) για εργαλεία τεχνητής νοημοσύνης. Στο κέντρο του βρίσκεται μια συνομιλία με τον Claude (ή όποιον άλλο κεντρικό agent προτιμήσεις) που ξέρει τον πλήρη κατάλογο 39+ εξειδικευμένων εργαλείων της εφαρμογής και σου προτείνει τα κατάλληλα ανάλογα με το task σου. Γύρω από αυτό τον πυρήνα, το Κόμβος έχει εξελιχθεί σε πλήρη εφαρμογή chat με πραγματική ενσωμάτωση στο Anthropic API, σύστημα plugins, και μια μεγάλη γκάμα ρυθμίσεων εμφάνισης/συμπεριφοράς.

## Βασικές λειτουργίες

**Συνομιλία & agents**
- Πραγματική σύνδεση με το Anthropic Messages API (Claude) — ή με οποιοδήποτε άλλο εργαλείο του καταλόγου έχει OpenAI-compatible endpoint (ChatGPT, Mistral, DeepSeek, Grok, Gemini) ως εναλλακτικό κεντρικό agent.
- Ορατό extended thinking, πραγματική αναζήτηση στο διαδίκτυο (web search tool), επισύναψη εικόνων (vision), υποστήριξη απομακρυσμένων MCP servers.
- **Σύγκριση agents** (⚖): στέλνει το ίδιο ερώτημα σε όλους τους agents με αποθηκευμένο κλειδί ταυτόχρονα.
- Markdown rendering στις απαντήσεις (έντονα/πλάγια/inline code, μπλοκ κώδικα με κουμπί αντιγραφής).
- Επανάληψη (regenerate) απάντησης, επεξεργασία δικού σου μηνύματος πριν το ξαναστείλεις, ασφαλής διαγραφή μεμονωμένου μηνύματος.
- Πολλαπλές αποθηκευμένες συζητήσεις: μετονομασία, διπλότυπο, διαγραφή, αναζήτηση (τίτλο + περιεχόμενο), εξαγωγή/εισαγωγή πλήρους αντιγράφου ασφαλείας σε JSON.
- Prompt caching για τον Claude και γενικός κανόνας ελάχιστου κόστους tokens για κάθε agent χωρίς εξαίρεση.

**Κατάλογος εργαλείων**
- 39+ εργαλεία σε 8 κατηγορίες (Κείμενο, Εικόνα, Βίντεο, Ήχος, Κώδικας, Agents & Αυτοματισμοί, Έρευνα, Σχεδιασμός), με πραγματικά λογότυπα, περιγραφές, link δωρεάν/paid πλάνου.
- Καρφίτσωμα αγαπημένων εργαλείων (⭐) — εμφανίζονται πάντα πρώτα στην κατηγορία τους.
- Αναζήτηση εργαλείων σε όλες τις κατηγορίες ταυτόχρονα.
- Λεπτομέρειες κάθε εργαλείου σε πλαϊνό πάνελ, με σύνδεσμο "Πρόσθεσε στο Claude Desktop (MCP)" όπου υπάρχει επίσημος MCP server.

**Επεκτασιμότητα**
- Πραγματικό σύστημα **plugins** (`AssemblyLoadContext` isolation): οποιοσδήποτε μπορεί να γράψει ένα .NET plugin που καταχωρεί νέες εντολές `/κάτι` στο chat ή προσθέτει εργαλεία στον κατάλογο. Δείγμα plugin (`/weather <πόλη>`) συμπεριλαμβάνεται.
- Απομακρυσμένοι MCP servers για τον Claude, χωρίς να χρειάζεται τοπικός MCP client.

**Εμφάνιση & συμπεριφορά**
- Light/Dark mode, 5 οπτικά "θέματα" εμπνευσμένα από Windows Vista/7/8/10-11, 10 κινούμενα φόντα κεφαλίδας, ρυθμιζόμενη διαφάνεια, μέγεθος κειμένου συνομιλίας (προσβασιμότητα).
- Πάντα σε πρώτο πλάνο, εκκίνηση με τα Windows, εκκίνηση ελαχιστοποιημένο στην περιοχή ειδοποιήσεων (system tray), καθολική συντόμευση Ctrl+Alt+K, ήχος ειδοποίησης όταν φτάνει απάντηση στο παρασκήνιο.
- Οδηγός πρώτης εκκίνησης (onboarding) με tutorial σελίδες, επαναδιαθέσιμος από τις Ρυθμίσεις.
- Πλήρης μετάφραση σε 4 γλώσσες: Ελληνικά, English, Français, Deutsch.

**Ασφάλεια**
- Τα κλειδιά API κρυπτογραφούνται με Windows DPAPI (δεμένα στον λογαριασμό χρήστη) πριν αποθηκευτούν στο δίσκο.

## Πώς το χτίζεις

1. Εγκατέστησε το [.NET 8 SDK](https://dotnet.microsoft.com/download) σε Windows.
2. Άνοιξε τον φάκελο με Visual Studio 2022 (`Kombos.csproj`) **ή** από τερματικό:
   ```
   dotnet build
   dotnet run
   ```
3. Το εκτελέσιμο βγαίνει στο `bin/Debug/net8.0-windows/Kombos.exe`.
4. Χρειάζεσαι δικό σου κλειδί API από [console.anthropic.com](https://console.anthropic.com) για τον Claude — μπαίνει στις Ρυθμίσεις → Agent, κρυπτογραφημένο τοπικά, ποτέ σε καθαρό κείμενο.

## Δομή

```
Kombos.csproj                   Κύριο project (.NET 8, WPF + WinForms για το tray icon)
App.xaml / App.xaml.cs
MainWindow.xaml(.cs)            UI + κύρια λογική chat/κατάλογος
SettingsWindow.xaml(.cs)        Ρυθμίσεις (Agent/Project/Συμπεριφορά/Επεκτάσεις/Πληροφορίες)
AppearanceWindow.xaml(.cs)      Εμφάνιση/Θέματα/Skins
Themes/Light.xaml, Dark.xaml    Χρωματικά θέματα
Models/Models.cs                ToolItem, CategoryDef, ChatTurn, Conversation, ...
Data/Catalog.cs                 Ο πλήρης κατάλογος εργαλείων
Services/                       ClaudeClient, OpenAiCompatibleClient, SettingsStore,
                                 ConversationStore, DpapiProtector, PluginLoader, KombosHost,
                                 TrayIconService
Localization/L.cs               Όλα τα strings σε 4 γλώσσες
Plugins/IKombosPlugin.cs        Contract για plugins τρίτων
plugins-src/KombosWeather/      Δείγμα plugin (ξεχωριστό .csproj)
installer/                      Script εγκατάστασης (Inno Setup)
```

## Plugins

Βάλε τον φάκελο ενός plugin (με το .dll του) μέσα στο `%AppData%\Kombos\plugins\`, μετά κάνε επανεκκίνηση. Δες το `plugins-src/KombosWeather` για ένα πλήρες παράδειγμα (contract `IKombosPlugin`, καταχώρηση εντολής chat).

## Άδεια / αποποίηση ευθύνης

Τα κλειδιά API που εισάγεις χρησιμοποιούνται με δική σου ευθύνη· κάθε χρέωση γίνεται στον δικό σου λογαριασμό στον αντίστοιχο πάροχο. Τα εργαλεία/agents τρίτων που εμφανίζονται στον κατάλογο δεν ανήκουν ούτε ελέγχονται από αυτή την εφαρμογή.
