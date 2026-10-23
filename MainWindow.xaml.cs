using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

using Kombos.Data;
using Kombos.Localization;
using Kombos.Models;
using Kombos.Services;

namespace Kombos
{
    public partial class MainWindow : Window
    {
        internal readonly ClaudeClient _claude = new();
        internal readonly List<ChatTurn> _history = new();
        internal List<string> _hiveMemory = new();
        internal List<Conversation> _conversations = new();
        private string _currentConversationId = Guid.NewGuid().ToString("N");
        /// <summary>Ονομασμένα Projects (v3.9.0, στυλ Claude Desktop) - ομαδοποιούν συζητήσεις. null στο
        /// φίλτρο σημαίνει "Όλες" (χωρίς φιλτράρισμα ανά project).</summary>
        internal List<KombosProject> _projects = new();
        internal List<AgentPreset> _presets = new();
        internal string _updateCheckRepo = "";
        internal bool _gatewayEnabled = false;
        internal int _gatewayPort = 8420;
        internal GatewayServer? _gatewayServer;
        internal readonly List<GatewayRequestLogEntry> _gatewayLog = new();
        internal string? _gatewayStartError;
        internal bool _autoContinueOnClaudeLimit = true;
        internal string _rememberedFallbackAgent = "";
        internal string? _activeProjectFilterId;
        private string? _pendingNewChatProjectId;
        internal readonly List<UsageRow> _usageRows = new();

        private const long ClaudeSessionBudget = 200_000;
        private long _claudeUsed = 0;
        private long _cacheReadTotal = 0;
        private long _cacheCreationTotal = 0;

        private bool _isDark = false;
        internal AppThemeMode _themeMode = AppThemeMode.System;
        private bool _sending = false;
        private string? _pendingImagePath;
        private bool _thinkingEnabled = false;
        private bool _webSearchEnabled = false;
        private bool _compareModeEnabled = false;
        private CancellationTokenSource? _sendCts;
        private Button? _lastRegenerateButton;
        private StackPanel? _lastAssistantRow;
        /// <summary>Αυξάνεται σε κάθε New Chat / Load Conversation - ένα in-flight αίτημα που ολοκληρώνεται
        /// ΜΕΤΑ την αλλαγή συνομιλίας ελέγχει αυτό πριν αγγίξει _history/UI, ώστε μια καθυστερημένη απάντηση
        /// να μην "μολύνει" τη νέα/άλλη συνομιλία (η ακύρωση του CancellationTokenSource δεν εγγυάται ότι το
        /// ήδη απεσταλμένο HTTP request δεν θα προλάβει να απαντήσει).</summary>
        private int _conversationEpoch;
        /// <summary>Μη αποθηκευμένο κείμενο εισαγωγής ανά συνομιλία (μόνο στη μνήμη, χάνεται στο κλείσιμο της
        /// εφαρμογής) - ώστε η εναλλαγή σε άλλη αποθηκευμένη συζήτηση να μη σβήνει κάτι μισο-γραμμένο.</summary>
        private readonly Dictionary<string, string> _drafts = new();
        private StackPanel? _typingRow;
        private ToolItem? _currentTool;
        private string? _activeCategoryId;
        private readonly Dictionary<string, Border> _categoryFrames = new();
        private readonly Dictionary<string, (Border Tab, TextBlock Label, Canvas Icon)> _categoryTabs = new();
        internal readonly Dictionary<string, string> _apiKeys = new();
        internal readonly Dictionary<string, string> _agentUsernames = new();
        internal readonly Dictionary<string, string> _agentPasswords = new();
        internal readonly HashSet<string> _pinnedTools = new();
        internal readonly HashSet<string> _disabledPlugins = new();
        internal readonly KombosHost _pluginHost = new();
        internal List<LoadedPlugin> _loadedPlugins = new();
        internal bool _sidebarRight = false;
        internal string _centralAgentName = "Claude";
        internal TransparencyLevel _transparency = TransparencyLevel.None;
        internal BgAnimStyle _bgStyle = BgAnimStyle.None;
        internal WindowsSkin _skin = WindowsSkin.Modern;
        internal ChatTextScale _chatTextScale = ChatTextScale.Normal;
        internal bool _alwaysOnTop = false;
        internal bool _startWithWindows = false;
        internal bool _startMinimized = false;
        internal bool _soundOnReply = false;
        internal bool _onboardingSeen = false;
        internal string _customInstructions = "";
        internal string _mcpServersRaw = "";
        internal string _conversationsSyncFolder = "";
        internal string _activeProjectPath = "";
        private bool IsElevated => _transparency != TransparencyLevel.None;
        private double WindowOpacity => _transparency switch
        {
            TransparencyLevel.Low => 0.95,
            TransparencyLevel.Medium => 0.85,
            _ => 1.0
        };

        // ---------- Windows-era skin: γωνίες/σκιάσεις/glossy κουμπιά/Metro πλακίδια ανά εποχή ----------
        private double SkinShellRadius => _skin switch { WindowsSkin.Vista => 14, WindowsSkin.Win7 => 10, WindowsSkin.Win8 => 0, WindowsSkin.Win10 => 8, WindowsSkin.Classic => 0, WindowsSkin.Nexus => 18, _ => 8 };
        private double SkinFrameRadius => _skin switch { WindowsSkin.Vista => 16, WindowsSkin.Win7 => 10, WindowsSkin.Win8 => 0, WindowsSkin.Win10 => 6, WindowsSkin.Classic => 0, WindowsSkin.Nexus => 16, _ => (IsElevated ? 12 : 5) };
        private double SkinTileRadius => _skin switch { WindowsSkin.Vista => 10, WindowsSkin.Win7 => 6, WindowsSkin.Win8 => 0, WindowsSkin.Win10 => 4, WindowsSkin.Classic => 0, WindowsSkin.Nexus => 10, _ => (IsElevated ? 8 : 3) };
        private double SkinTabRadius => _skin switch { WindowsSkin.Win8 => 0, WindowsSkin.Classic => 0, _ => 16 };
        private bool SkinUsesMetroTiles => _skin == WindowsSkin.Win8;
        private bool SkinUsesGlossyButtons => _skin is WindowsSkin.Vista or WindowsSkin.Win7;
        private bool SkinUsesGlassTint => _skin is WindowsSkin.Vista or WindowsSkin.Win7;
        /// <summary>Classic = επίπεδο, ανάγλυφο 3D πλαίσιο στυλ Windows 95/98/2000, όχι μαλακή σκιά.</summary>
        private bool SkinUsesBeveled => _skin == WindowsSkin.Classic;

        /// <summary>Σηματοδοτικό χρώμα κάθε skin, κοντά στο αυθεντικό της αντίστοιχης εποχής (αντί το skin να
        /// αλλάζει μόνο γωνίες/σκιές, χρωματίζει και το ίδιο το accent brush που ήδη χρησιμοποιείται παντού
        /// στην εφαρμογή - κουμπί αποστολής, ενεργή καρτέλα, avatar): Vista/Win7 = γυάλινο Aero μπλε, Win8 =
        /// επίπεδο Metro μπλε, Win10 = Fluent μπλε, Classic = navy τίτλου παραθύρου Windows 95/98/2000, Nexus =
        /// το πρωτότυπο πορτοκαλί branding του ίδιου του Kombos.</summary>
        private Color SkinAccentColor => _skin switch
        {
            WindowsSkin.Vista => Color.FromRgb(0x3D, 0x95, 0xE0),
            WindowsSkin.Win7 => Color.FromRgb(0x2A, 0x8A, 0xD4),
            WindowsSkin.Win8 => Color.FromRgb(0x00, 0xAE, 0xEF),
            WindowsSkin.Win10 => Color.FromRgb(0x00, 0x78, 0xD7),
            WindowsSkin.Classic => Color.FromRgb(0x0A, 0x24, 0x6A),
            WindowsSkin.Nexus => Color.FromRgb(0xFF, 0x7A, 0x45),
            _ => Color.FromRgb(0xE2, 0x6D, 0x3C)
        };

        /// <summary>Εφαρμόζει το SkinAccentColor πάνω στο ήδη υπάρχον "ClaudeBrush" resource - το ίδιο brush
        /// που χρησιμοποιείται ήδη σε δεκάδες σημεία (κουμπί αποστολής, ενεργές καρτέλες, avatar, ενδείξεις
        /// cache) οπότε ΚΑΘΕ skin αποκτά αυθεντικό χρώμα εποχής χωρίς να χρειαστεί να αλλάξουν όλα τα σημεία
        /// binding ένα-ένα. Το "Modern" (προεπιλογή) κρατάει το αρχικό branding χρώμα του Kombos.</summary>
        private void ApplySkinAccent()
        {
            if (_skin == WindowsSkin.Modern)
                Application.Current.Resources.Remove("ClaudeBrush"); // επιστροφή στο αρχικό branding χρώμα του τρέχοντος θέματος
            else
                Application.Current.Resources["ClaudeBrush"] = new SolidColorBrush(SkinAccentColor);
        }

        /// <summary>Το "Windows Classic" δεν έχει light/dark mode - είναι πάντα το ίδιο γκρι με navy τίτλο. Το
        /// "Nexus" σε dark mode παίρνει ουδέτερη ανθρακί/γραφίτη παλέτα (στυλ Adobe Acrobat Pro dark mode)
        /// αντί για το γενικό ζεστό σχεδόν-μαύρο του Dark.xaml, που αναφέρθηκε ως πολύ σκοτεινό/χαμηλής
        /// αντίθεσης. Και οι δύο περιπτώσεις υπερκαλύπτουν global τα ίδια resource keys (PaperBrush/PanelBrush/
        /// LineBrush/InkBrush/InkSoftBrush/SidebarBrush) που ήδη χρησιμοποιεί ΚΑΘΕ παράθυρο μέσω DynamicResource
        /// - έτσι όλα τα παράθυρα παίρνουν αυτόματα το σωστό χρώμα, χωρίς ξεχωριστό κώδικα ανά παράθυρο.</summary>
        private void ApplySkinPalette()
        {
            if (_skin == WindowsSkin.Classic)
            {
                // ΔΙΟΡΘΩΣΗ: πριν αγνοούσε εντελώς το _isDark και έμενε πάντα στην ίδια ανοιχτόχρωμη γκρι
                // παλέτα - η εναλλαγή Light/Dark φαινόταν "χαλασμένη" σε αυτό το skin. Η σκοτεινή παραλλαγή
                // εδώ είναι το πραγματικό ιστορικό αντίστοιχο: το "High Contrast Black" θέμα των Windows.
                if (_isDark)
                {
                    Application.Current.Resources["PaperBrush"] = new SolidColorBrush(Color.FromRgb(0x30, 0x30, 0x30));
                    Application.Current.Resources["PanelBrush"] = new SolidColorBrush(Color.FromRgb(0x40, 0x40, 0x40));
                    Application.Current.Resources["LineBrush"] = new SolidColorBrush(Color.FromRgb(0x60, 0x60, 0x60));
                    Application.Current.Resources["InkBrush"] = new SolidColorBrush(Colors.White);
                    Application.Current.Resources["InkSoftBrush"] = new SolidColorBrush(Color.FromRgb(0xC0, 0xC0, 0xC0));
                    Application.Current.Resources["SidebarBrush"] = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20));
                }
                else
                {
                    Application.Current.Resources["PaperBrush"] = new SolidColorBrush(Color.FromRgb(0xC0, 0xC0, 0xC0));
                    Application.Current.Resources["PanelBrush"] = new SolidColorBrush(Color.FromRgb(0xD4, 0xD0, 0xC8));
                    Application.Current.Resources["LineBrush"] = new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80));
                    Application.Current.Resources["InkBrush"] = new SolidColorBrush(Colors.Black);
                    Application.Current.Resources["InkSoftBrush"] = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44));
                    Application.Current.Resources["SidebarBrush"] = new SolidColorBrush(SkinAccentColor);
                }
                return;
            }

            if (_skin == WindowsSkin.Nexus && _isDark)
            {
                // Ουδέτερο ανθρακί/γραφίτη (όχι ζεστό καφέ-μαύρο) - το πορτοκαλί accent του Nexus ξεχωρίζει
                // περισσότερο πάνω σε ουδέτερο γκρι παρά σε σχεδόν-μαύρο.
                Application.Current.Resources["PaperBrush"] = new SolidColorBrush(Color.FromRgb(0x2E, 0x2E, 0x2E));
                Application.Current.Resources["PanelBrush"] = new SolidColorBrush(Color.FromRgb(0x3A, 0x3A, 0x3A));
                Application.Current.Resources["LineBrush"] = new SolidColorBrush(Color.FromRgb(0x4D, 0x4D, 0x4D));
                Application.Current.Resources["InkBrush"] = new SolidColorBrush(Color.FromRgb(0xEC, 0xEC, 0xEC));
                Application.Current.Resources["InkSoftBrush"] = new SolidColorBrush(Color.FromRgb(0xA6, 0xA6, 0xA6));
                Application.Current.Resources["SidebarBrush"] = new SolidColorBrush(Color.FromRgb(0x23, 0x23, 0x23));
                return;
            }

            Application.Current.Resources.Remove("PaperBrush");
            Application.Current.Resources.Remove("PanelBrush");
            Application.Current.Resources.Remove("LineBrush");
            Application.Current.Resources.Remove("InkBrush");
            Application.Current.Resources.Remove("InkSoftBrush");
            Application.Current.Resources.Remove("SidebarBrush");
        }

        /// <summary>Το skin "Windows Classic" αντικαθιστά την πλευρική μπάρα εικονιδίων με ένα παραδοσιακό
        /// οριζόντιο μενού File/Edit/View/Tools/Help - ζητήθηκε ρητά ώστε η εμπειρία να είναι "ακριβώς όπως
        /// ήταν τότε" (τα Windows της εποχής δεν είχαν κάθετη μπάρα εικονιδίων, είχαν menu bar).</summary>
        /// <summary>Αναδίπλωση πλευρικού μενού (v4.6.0, ρητό αίτημα χρήστη) - όχι μόνιμη ρύθμιση, μόνο
        /// κατάσταση της τρέχουσας συνεδρίας (δεν αποθηκεύεται) - ίδιο πνεύμα με μικρό, γρήγορο toggle που
        /// βρίσκεις σε πολλά σύγχρονα εργαλεία με πλευρική μπάρα εικονιδίων.</summary>
        /// <summary>false = "αναπτυγμένο" (προεπιλογή): κάθε κουμπί δείχνει εικονίδιο+ετικέτα, όπως στο
        /// MotionDeskTop (ρητό αίτημα χρήστη, v4.8.0). true = "διπλωμένο": μόνο εικονίδιο, στενή 56px στήλη
        /// (η παλιά όψη). Δεν κρύβεται πλέον εντελώς (0px) - πάντα υπάρχει τουλάχιστον η στενή στήλη.</summary>
        private bool _sidebarCollapsed = false;
        private const double SidebarExpandedWidth = 190;
        private const double SidebarCollapsedWidth = 56;

        /// <summary>Εφαρμόζει το πλάτος της στήλης που φιλοξενεί την πλευρική μπάρα - Col0 ή Col1, ανάλογα
        /// με το αν η μπάρα είναι αριστερά ή δεξιά (_sidebarRight, βλ. ApplySidebarPosition). Κεντρικό σημείο
        /// ώστε το Classic-skin (μηδενικό πλάτος) και το collapse-toggle να μη γράφουν ασυνεπώς τη λάθος στήλη.</summary>
        private void ApplySidebarWidth()
        {
            bool classic = _skin == WindowsSkin.Classic;
            double width = classic ? 0 : (_sidebarCollapsed ? SidebarCollapsedWidth : SidebarExpandedWidth);
            if (_sidebarRight) Col1.Width = new GridLength(width);
            else Col0.Width = new GridLength(width);
        }

        private void SidebarCollapseButton_Click(object sender, RoutedEventArgs e)
        {
            _sidebarCollapsed = !_sidebarCollapsed;
            ApplySidebarWidth();
            ((RotateTransform)SidebarCollapseIcon.RenderTransform).Angle = _sidebarCollapsed ? 90 : -90;
            var labelVisibility = _sidebarCollapsed ? Visibility.Collapsed : Visibility.Visible;
            ThemeToggleLabelText.Visibility = labelVisibility;
            LangToggleLabelText.Visibility = labelVisibility;
            AppearanceLabelText.Visibility = labelVisibility;
            ArtifactsLabelText.Visibility = labelVisibility;
            ProjectsSidebarLabelText.Visibility = labelVisibility;
            SettingsGearLabelText.Visibility = labelVisibility;
        }

        private void ApplySkinChrome()
        {
            bool classic = _skin == WindowsSkin.Classic;
            ClassicMenuBar.Visibility = classic ? Visibility.Visible : Visibility.Collapsed;
            SidebarBorder.Visibility = classic ? Visibility.Collapsed : Visibility.Visible;
            ApplySidebarWidth();
            // "Αναδίπλωση πλευρικού μενού" (v4.6.0) δεν έχει νόημα στο Classic - εκεί δεν υπάρχει καθόλου
            // κάθετη μπάρα εικονιδίων (menu bar αντ' αυτού μόνιμα).
            SidebarCollapseButton.Visibility = classic ? Visibility.Collapsed : Visibility.Visible;

            // "Windows Classic" = τετράγωνες γωνίες ΠΑΝΤΟΥ, όχι μόνο στα κύρια παράθυρα - τα popups (γλώσσα,
            // Artifacts, ανάλυση χρήσης, Handoff, ιστορικό/Projects) είχαν ξεχαστεί και έμεναν στρογγυλεμένα.
            LangPopupBorder.CornerRadius = new CornerRadius(classic ? 0 : 6);
            ArtifactsPopupBorder.CornerRadius = new CornerRadius(classic ? 0 : 6);
            BreakdownPopupBorder.CornerRadius = new CornerRadius(classic ? 0 : 4);
            HandoffPopupBorder.CornerRadius = new CornerRadius(classic ? 0 : 6);
            PromptLibraryPopupBorder.CornerRadius = new CornerRadius(classic ? 0 : 6);
            HistoryPopupBorder.CornerRadius = new CornerRadius(classic ? 0 : 6);
        }

        private void ClassicExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _reallyExiting = true;
            Close();
        }

        private void ClassicAgentSettingsMenuItem_Click(object sender, RoutedEventArgs e) => OpenAgentSettingsWindow();
        private void ClassicCommandPaletteMenuItem_Click(object sender, RoutedEventArgs e) => OpenCommandPalette();
        private void ClassicHelpMenuItem_Click(object sender, RoutedEventArgs e) => OpenSettingsWindow("about");
        private void ClassicFocusModeMenuItem_Click(object sender, RoutedEventArgs e) => ToggleFocusMode();
        private (double Blur, double Depth, double Opacity)? SkinShadow => _skin switch
        {
            WindowsSkin.Vista => (22, 5, 0.32),
            WindowsSkin.Win7 => (16, 3, 0.26),
            WindowsSkin.Win8 => null,
            WindowsSkin.Win10 => (10, 2, 0.16),
            WindowsSkin.Classic => null,
            WindowsSkin.Nexus => (18, 4, 0.28),
            _ => IsElevated ? (16.0, 3.0, 0.2) : null
        };
        /// <summary>Σκιά σε επίπεδο παραθύρου (πάντα ενεργή, ανεξάρτητη από τη ρύθμιση διαφάνειας) — μηδενική μόνο για Win8/Classic.</summary>
        private (double Blur, double Opacity)? SkinShellShadow => _skin switch
        {
            WindowsSkin.Vista => (28, 0.42),
            WindowsSkin.Win7 => (22, 0.36),
            WindowsSkin.Win8 => null,
            WindowsSkin.Win10 => (16, 0.3),
            WindowsSkin.Classic => null,
            WindowsSkin.Nexus => (24, 0.4),
            _ => (18, 0.35)
        };

        public MainWindow()
        {
            InitializeComponent();
            BuildUsageRows();
            LoadSettings();
            SyncUsageConnectionStatus();
            _conversations = ConversationStore.Load(_conversationsSyncFolder);
            _projects = ProjectsListStore.Load(_conversationsSyncFolder);
            _presets = PresetsStore.Load();
            SetupSyncFolderWatcher();
            ApplyGatewaySetting();
            _loadedPlugins = PluginLoader.LoadAll(_pluginHost, _disabledPlugins);
            _hiveMemory = HiveMemoryStore.Load();
            RegisterBuiltInSlashCommands();
            ApplyCentralAgent();
            ApplyLanguage();
            ApplySidebarPosition();
            ApplyAppearance();
            InitializeTrayIcon();
            Microsoft.Win32.SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        }

        // ---------- System tray: το Kombos "τρέχει στο παρασκήνιο" - κλείσιμο παραθύρου = απόκρυψη, όχι έξοδος.
        // Πραγματική έξοδος μόνο μέσω "Έξοδος" στο μενού του tray icon. ----------
        private TrayIconService? _trayIcon;
        private bool _reallyExiting;

        private void InitializeTrayIcon()
        {
            _trayIcon = new TrayIconService(L.WindowTitle, L.TrayOpen, L.TrayNewChat, L.TrayAccounts, L.TrayExit);
            _trayIcon.OpenRequested += () => Dispatcher.Invoke(ShowAndActivate);
            _trayIcon.NewChatRequested += () => Dispatcher.Invoke(() =>
            {
                ShowAndActivate();
                NewChatButton_Click(this, new RoutedEventArgs());
            });
            _trayIcon.AccountsRequested += () => Dispatcher.Invoke(OpenAccountsWindow);
            _trayIcon.ExitRequested += () => Dispatcher.Invoke(() =>
            {
                _reallyExiting = true;
                Close();
            });
        }

        private void NotifyReplyIfInBackground(string replyText)
        {
            if (IsActive) return;
            _trayIcon?.ShowNotification("Κόμβος", replyText);
            if (_soundOnReply)
            {
                try { System.Media.SystemSounds.Asterisk.Play(); }
                catch { /* όχι κρίσιμο αν αποτύχει */ }
            }
        }

        internal void ShowAndActivate()
        {
            Show();
            if (WindowState == WindowState.Minimized) WindowState = WindowState.Normal;
            Activate();
            // "Flash" topmost για να έρθει σίγουρα μπροστά ακόμα κι αν ένα άλλο παράθυρο έχει το focus, αλλά
            // επαναφέρουμε στην ΠΡΑΓΜΑΤΙΚΗ προτίμηση του χρήστη (_alwaysOnTop) αντί για πάντα false - αλλιώς
            // το άνοιγμα από tray/hotkey θα απενεργοποιούσε σιωπηλά το "πάντα σε πρώτο πλάνο".
            Topmost = true;
            Topmost = _alwaysOnTop;
            BuildAnimatedBackground(_bgStyle); // βλ. MainWindow_Closing - είχαν σταματήσει όσο ήταν κρυφό
            if (_bgStyle == BgAnimStyle.Glow)
                HeaderGlowShift.BeginAnimation(TranslateTransform.XProperty,
                    new DoubleAnimation(-60, 60, TimeSpan.FromSeconds(6)) { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever });
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            if (_reallyExiting)
            {
                _trayIcon?.Dispose();
                UnregisterGlobalHotkey();
                Microsoft.Win32.SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
                _syncFolderWatcher?.Dispose();
                _gatewayServer?.Dispose();
                LogUsageSessionIfAny();
                return;
            }
            e.Cancel = true;
            Hide();
            // ΔΙΟΡΘΩΣΗ κατανάλωσης πόρων: τα κινούμενα φόντα είναι RepeatBehavior.Forever animations - το WPF
            // ΔΕΝ τα σταματάει από μόνο του όταν το παράθυρο γίνεται Hide() (τρέχει επ' αόριστον στο
            // παρασκήνιο, όσο το Kombos μένει στο tray). Καθαρίζοντας το Canvas αποσυνδέουμε τα animation
            // clocks - ξαναχτίζονται στο ShowAndActivate() όταν ο χρήστης το ξανανοίξει.
            HeaderParticles.Children.Clear();
            HeaderGlowShift.BeginAnimation(TranslateTransform.XProperty, null); // σταματά και το ξεχωριστό Glow storyboard, αν ήταν ενεργό
        }

        // ---------- Global hotkey (Ctrl+Alt+K) - άνοιγμα/επαναφορά του Kombos από οπουδήποτε στα Windows ----------
        private const int HotkeyId = 0x4B4B; // αυθαίρετο, μοναδικό αναγνωριστικό
        private const int AnywhereHotkeyId = 0x4B4D; // Kombos Anywhere (v4.5.0) - ξεχωριστό αναγνωριστικό
        private const uint ModControl = 0x0002;
        private const uint ModAlt = 0x0001;
        private const uint VkK = 0x4B;
        private const uint VkSpace = 0x20;
        private HwndSource? _hwndSource;
        private bool _hotkeyRegistered;
        private bool _anywhereHotkeyRegistered;
        private AnywhereBarWindow? _anywhereBar;

        /// <summary>Προσθέτει/αφαιρεί την εφαρμογή από το registry Run key του τρέχοντος χρήστη - δεν
        /// χρειάζεται δικαιώματα διαχειριστή (HKCU, όχι HKLM) και επηρεάζει μόνο τον τρέχοντα λογαριασμό.</summary>
        internal static void SetStartWithWindows(bool enabled)
        {
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Run", writable: true);
                if (key == null) return;
                if (enabled)
                {
                    var exePath = Environment.ProcessPath;
                    if (!string.IsNullOrEmpty(exePath)) key.SetValue("Kombos", $"\"{exePath}\"");
                }
                else
                {
                    key.DeleteValue("Kombos", throwOnMissingValue: false);
                }
            }
            catch { /* π.χ. έλλειψη δικαιωμάτων εγγραφής στο registry - όχι κρίσιμο */ }
        }

        [DllImport("user32.dll")] private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // ---------- Σωστά περιθώρια οθόνης στη μεγιστοποίηση (WM_GETMINMAXINFO) ----------
        // ΔΙΟΡΘΩΣΗ: με WindowStyle="None" η προεπιλεγμένη μεγιστοποίηση των Windows γεμίζει ΟΛΗ την οθόνη
        // (κάτω από τη γραμμή εργασιών συμπεριλαμβανομένης), αγνοώντας το πραγματικό work area - και το
        // SystemParameters.WorkArea που χρησιμοποιούνταν πριν αντιπροσωπεύει ΜΟΝΟ την κύρια οθόνη, λάθος σε
        // πολυ-οθονικές διατάξεις ή όταν η γραμμή εργασιών είναι πάνω/στο πλάι. Το σωστό, καθιερωμένο fix για
        // custom-chrome WPF παράθυρα είναι να απαντήσεις στο WM_GETMINMAXINFO με το work area της ΣΥΓΚΕΚΡΙΜΕΝΗΣ
        // οθόνης πάνω στην οποία βρίσκεται αυτή τη στιγμή το παράθυρο.
        [DllImport("user32.dll")] private static extern IntPtr MonitorFromWindow(IntPtr handle, uint flags);
        [DllImport("user32.dll")] private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [StructLayout(LayoutKind.Sequential)] private struct RECT { public int Left, Top, Right, Bottom; }
        [StructLayout(LayoutKind.Sequential)] private struct POINT { public int X, Y; }
        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFO { public int cbSize; public RECT rcMonitor; public RECT rcWork; public uint dwFlags; }
        [StructLayout(LayoutKind.Sequential)]
        private struct MINMAXINFO { public POINT ptReserved, ptMaxSize, ptMaxPosition, ptMinTrackSize, ptMaxTrackSize; }

        private const uint MonitorDefaultToNearest = 2;

        private void HandleGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            var monitor = MonitorFromWindow(hwnd, MonitorDefaultToNearest);
            if (monitor == IntPtr.Zero) return;

            var info = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };
            if (!GetMonitorInfo(monitor, ref info)) return;

            var mmi = Marshal.PtrToStructure<MINMAXINFO>(lParam);
            var workArea = info.rcWork;
            var monitorArea = info.rcMonitor;
            // Θέση/μέγεθος σχετικά με την ίδια την οθόνη (όχι την κύρια) - έτσι η μεγιστοποίηση σταματά ακριβώς
            // στα όρια του work area, ό,τι θέση/προσανατολισμό κι αν έχει η γραμμή εργασιών.
            mmi.ptMaxPosition.X = workArea.Left - monitorArea.Left;
            mmi.ptMaxPosition.Y = workArea.Top - monitorArea.Top;
            mmi.ptMaxSize.X = workArea.Right - workArea.Left;
            mmi.ptMaxSize.Y = workArea.Bottom - workArea.Top;
            Marshal.StructureToPtr(mmi, lParam, true);
        }

        private void RegisterGlobalHotkey()
        {
            var handle = new WindowInteropHelper(this).Handle;
            _hwndSource = HwndSource.FromHwnd(handle);
            _hwndSource?.AddHook(HwndHook);
            // Το Ctrl+Alt+K μπορεί ήδη να είναι δεσμευμένο από άλλη εφαρμογή - σε αυτή την περίπτωση το
            // RegisterHotKey αποτυγχάνει σιωπηλά (επιστρέφει false)· καταγράφουμε την κατάσταση ώστε να μην
            // προσπαθήσουμε να το ξε-καταχωρίσουμε αργότερα (θα ήταν άσκοπη κλήση, όχι σφάλμα).
            _hotkeyRegistered = RegisterHotKey(handle, HotkeyId, ModControl | ModAlt, VkK);
            // Kombos Anywhere (v4.5.0) - Ctrl+Alt+Space, ξεχωριστό από το Ctrl+Alt+K (άνοιγμα/επαναφορά).
            _anywhereHotkeyRegistered = RegisterHotKey(handle, AnywhereHotkeyId, ModControl | ModAlt, VkSpace);
        }

        private void UnregisterGlobalHotkey()
        {
            if (_hwndSource == null) return;
            if (_hotkeyRegistered) UnregisterHotKey(_hwndSource.Handle, HotkeyId);
            if (_anywhereHotkeyRegistered) UnregisterHotKey(_hwndSource.Handle, AnywhereHotkeyId);
            _hwndSource.RemoveHook(HwndHook);
            _hwndSource = null;
        }

        /// <summary>Kombos Anywhere (v4.5.0) - μικρό, πάντα-από-πάνω quick-capture bar, ανοίγει από ΟΠΟΥΔΗΠΟΤΕ
        /// στα Windows με Ctrl+Alt+Space, χωρίς να χρειάζεται να είναι ήδη ανοιχτό το κύριο παράθυρο. Singleton
        /// - αν είναι ήδη ανοιχτό, απλά το φέρνει μπροστά αντί να ανοίξει δεύτερο.</summary>
        private void OpenAnywhereBar()
        {
            if (_anywhereBar != null) { _anywhereBar.Activate(); return; }
            _anywhereBar = new AnywhereBarWindow(this);
            _anywhereBar.Closed += (s, e) => _anywhereBar = null;
            _anywhereBar.Show();
            _anywhereBar.Activate();
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            const int WM_GETMINMAXINFO = 0x0024;
            if (msg == WM_HOTKEY && wParam.ToInt32() == HotkeyId)
            {
                ShowAndActivate();
                handled = true;
            }
            else if (msg == WM_HOTKEY && wParam.ToInt32() == AnywhereHotkeyId)
            {
                OpenAnywhereBar();
                handled = true;
            }
            else if (msg == WM_GETMINMAXINFO)
            {
                HandleGetMinMaxInfo(hwnd, lParam);
                handled = true;
            }
            return IntPtr.Zero;
        }

        // ---------- Custom window chrome (WindowStyle=None για διαφάνεια παραθύρου) ----------
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void MaximizeButton_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            // Το σωστό μέγεθος/θέση μεγιστοποίησης (ανά οθόνη, σεβόμενο το work area) το αναλαμβάνει πλέον
            // το WM_GETMINMAXINFO hook (βλ. HandleGetMinMaxInfo) - εδώ μένει μόνο η οπτική λεπτομέρεια των
            // γωνιών, που πρέπει να είναι τετράγωνες όσο το παράθυρο καλύπτει ολόκληρη την οθόνη.
            AppShellBorder.CornerRadius = new CornerRadius(WindowState == WindowState.Maximized ? 0 : SkinShellRadius);
        }

        // ---------- Splash "curtain" στην εκκίνηση ----------
        /// <summary>Γρήγορες συντομεύσεις πληκτρολογίου, ίδιο πνεύμα με άλλες desktop εφαρμογές συνομιλίας:
        /// Ctrl+N νέα συζήτηση, Ctrl+, ρυθμίσεις, Escape κλείνει το πρώτο ανοιχτό popup/panel που βρει.</summary>
        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.N)
            {
                e.Handled = true;
                NewChatButton_Click(sender, e);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.OemComma)
            {
                e.Handled = true;
                OpenSettingsWindow("behavior");
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.K)
            {
                e.Handled = true;
                OpenCommandPalette();
            }
            else if (e.Key == Key.F11)
            {
                e.Handled = true;
                ToggleFocusMode();
            }
            else if (e.Key == Key.Escape)
            {
                if (HistoryPopup.IsOpen) { HistoryPopup.IsOpen = false; e.Handled = true; }
                else if (BreakdownPopup.IsOpen) { BreakdownPopup.IsOpen = false; e.Handled = true; }
                else if (DetailsPanel.Visibility == Visibility.Visible) { DetailsPanel.Visibility = Visibility.Collapsed; e.Handled = true; }
                else if (_focusMode) { ToggleFocusMode(); e.Handled = true; }
            }
        }

        private bool _focusMode;

        /// <summary>Focus mode (v4.5.0) - F11 ή Esc κρύβει sidebar/menu bar/κεφαλίδα/κατάλογο εργαλείων/γραμμή
        /// κατάστασης, αφήνοντας μόνο τη συζήτηση, για ελεύθερη ανάγνωση/γράψιμο χωρίς περισπασμούς. Η σειρά
        /// της συζήτησης (<c>ChatRowDef</c>) παίρνει το χώρο που ελευθερώνεται από τον κατάλογο εργαλείων
        /// (<c>TilesRowDef</c>, που φτιάχτηκε 0 ύψος) αντί να μένει στο σταθερό της ύψος 260px.</summary>
        private void ToggleFocusMode()
        {
            _focusMode = !_focusMode;
            var visible = _focusMode ? Visibility.Collapsed : Visibility.Visible;

            HeaderRow.Visibility = visible;
            CategoryTabsRow.Visibility = visible;
            CategoryTilesRow.Visibility = visible;
            StatusBarRow.Visibility = visible;
            SidebarBorder.Visibility = _focusMode || _skin == WindowsSkin.Classic ? Visibility.Collapsed : Visibility.Visible;
            ClassicMenuBar.Visibility = !_focusMode && _skin == WindowsSkin.Classic ? Visibility.Visible : Visibility.Collapsed;

            ChatRowDef.Height = _focusMode ? new GridLength(1, GridUnitType.Star) : new GridLength(260);
            TilesRowDef.Height = _focusMode ? new GridLength(0) : new GridLength(1, GridUnitType.Star);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RegisterGlobalHotkey();

            // Ο οδηγός πρώτης χρήσης έχει προτεραιότητα έναντι του "εκκίνηση ελαχιστοποιημένο" - αλλιώς ένας
            // χρήστης που δεν έχει δει ποτέ τον οδηγό θα έμενε για πάντα χωρίς αυτόν (το Window_Loaded τρέχει
            // μία φορά ανά διεργασία, οπότε δεν θα ξαναπροσφερόταν ευκαιρία να εμφανιστεί).
            if (!_onboardingSeen)
            {
                ShowOnboarding();
                return; // το splash (και το "εκκίνηση ελαχιστοποιημένο", αν ισχύει) εφαρμόζονται μετά το κλείσιμο του οδηγού
            }

            if (_startMinimized)
            {
                SplashOverlay.Visibility = Visibility.Collapsed;
                Hide();
                return;
            }

            RunSplashAnimation();
        }

        private void RunSplashAnimation()
        {
            // Όπως στο GearWin: το εικονίδιο της κουρτίνας "προσγειώνεται" ακριβώς στη ΘΕΣΗ και το ΜΕΓΕΘΟΣ του
            // πραγματικού κινούμενου εικονιδίου της εφαρμογής (HeaderIcon, δίπλα στον τίτλο "Κόμβος") - κέντρο και διάσταση διαβάζονται από το
            // πραγματικό layout, σε συντεταγμένες της ίδιας της κουρτίνας. Ο τίτλος/έκδοση απλώς σβήνουν.
            SplashOverlay.UpdateLayout();
            var iconCenter = SplashIcon.TranslatePoint(new Point(SplashIcon.Width / 2, SplashIcon.Height / 2), SplashOverlay);
            var targetCenter = HeaderIcon.TranslatePoint(new Point(HeaderIcon.Width / 2, HeaderIcon.Height / 2), SplashOverlay);
            double targetScale = HeaderIcon.Width / SplashIcon.Width;
            var hold = TimeSpan.FromSeconds(0.9);
            var shrink = TimeSpan.FromSeconds(0.7);
            var ease = new CubicEase { EasingMode = EasingMode.EaseInOut };

            var scaleX = new DoubleAnimation(1, targetScale, shrink) { BeginTime = hold, EasingFunction = ease };
            var scaleY = new DoubleAnimation(1, targetScale, shrink) { BeginTime = hold, EasingFunction = ease };
            var transX = new DoubleAnimation(0, targetCenter.X - iconCenter.X, shrink) { BeginTime = hold, EasingFunction = ease };
            var transY = new DoubleAnimation(0, targetCenter.Y - iconCenter.Y, shrink) { BeginTime = hold, EasingFunction = ease };
            // Φόντο και κείμενα σβήνουν όσο το εικονίδιο πετάει, ώστε να αποκαλύπτεται η εφαρμογή από κάτω.
            var bgFade = new DoubleAnimation(1, 0, shrink) { BeginTime = hold };
            var textFade = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.35)) { BeginTime = hold };
            scaleY.Completed += (s, ev) => SplashOverlay.Visibility = Visibility.Collapsed;

            // Το BeginAnimation() απευθείας πάνω στο κάθε Freezable είναι αξιόπιστο (το Storyboard.SetTarget σε
            // εμφωλευμένα Transform μέσα σε TransformGroup αγνοούνταν σιωπηλά).
            SplashScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleX);
            SplashScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleY);
            SplashTranslate.BeginAnimation(TranslateTransform.XProperty, transX);
            SplashTranslate.BeginAnimation(TranslateTransform.YProperty, transY);
            SplashBackgroundBrush.BeginAnimation(SolidColorBrush.OpacityProperty, bgFade);
            SplashTitleText.BeginAnimation(UIElement.OpacityProperty, textFade);
            SplashVersionText.BeginAnimation(UIElement.OpacityProperty, textFade);
        }

        // ---------- Οδηγός πρώτης εκκίνησης (onboarding) ----------
        private int _onboardingPageIndex;
        /// <summary>True όταν ο οδηγός ανοίγει εκ νέου (Ρυθμίσεις → Βοήθεια) ΜΕΤΑ την πρώτη εκκίνηση - στην
        /// περίπτωση αυτή το κλείσιμο δεν πρέπει να ξανατρέξει το splash animation.</summary>
        private bool _onboardingIsReplay;

        internal void ShowOnboarding(bool isReplay = false)
        {
            _onboardingIsReplay = isReplay;
            _onboardingPageIndex = 0;
            OnboardingSkipButton.Content = L.OnboardingSkip;
            RenderOnboardingPage();
            OnboardingOverlay.Visibility = Visibility.Visible;
            // ΔΙΟΡΘΩΣΗ: το AppShellBorder.Opacity (ρύθμιση "Διαφάνεια") καλύπτει ΟΛΟ το παράθυρο, άρα και
            // αυτό το overlay - με Ελαφριά/Μέτρια διαφάνεια ενεργή, ο οδηγός έβγαινε ημιδιάφανος πάνω σε ό,τι
            // βρίσκεται πίσω από το παράθυρο στην οθόνη (αναφέρθηκε ως "σκοτεινό και θολό" σε dark mode). Ο
            // οδηγός πρέπει να είναι πάντα πλήρως αδιαφανής/ευανάγνωστος, ανεξάρτητα από αυτή τη ρύθμιση.
            AppShellBorder.Opacity = 1.0;
        }

        private void RenderOnboardingPage()
        {
            var pages = L.OnboardingPages;
            _onboardingPageIndex = Math.Clamp(_onboardingPageIndex, 0, pages.Length - 1);
            var page = pages[_onboardingPageIndex];
            OnboardingIconText.Text = page.Icon;
            OnboardingTitleText.Text = page.Title;
            OnboardingBodyText.Text = page.Body;
            BuildOnboardingDemo(_onboardingPageIndex);

            bool isFirst = _onboardingPageIndex == 0;
            bool isLast = _onboardingPageIndex == pages.Length - 1;
            OnboardingBackButton.Visibility = isFirst ? Visibility.Hidden : Visibility.Visible;
            OnboardingNextButton.Content = isLast ? L.OnboardingGetStarted : "→";

            OnboardingDotsPanel.Children.Clear();
            for (int i = 0; i < pages.Length; i++)
            {
                var dot = new Border
                {
                    Width = 7, Height = 7, CornerRadius = new CornerRadius(4), Margin = new Thickness(3, 0, 3, 0)
                };
                dot.SetResourceReference(Border.BackgroundProperty, i == _onboardingPageIndex ? "ClaudeBrush" : "LineBrush");
                OnboardingDotsPanel.Children.Add(dot);
            }
        }

        /// <summary>Μικρή, αυτο-επαναλαμβανόμενη "ζωντανή" επίδειξη ανά σελίδα του οδηγού - αντί για στατικό
        /// emoji, δείχνει σε κίνηση τι ακριβώς κάνει το κουμπί/λειτουργία που περιγράφει η σελίδα. Καθαρά
        /// vector σχήματα + Storyboards (ίδιο μοτίβο με τα κινούμενα φόντα BuildOrbit/BuildGears), όχι αρχεία
        /// GIF - μηδενικό πρόσθετο μέγεθος, καμία ανάγκη εγγραφής της πραγματικής οθόνης.</summary>
        private void BuildOnboardingDemo(int pageIndex)
        {
            OnboardingDemoCanvas.Children.Clear();
            switch (pageIndex)
            {
                case 0: BuildDemoHub(); break;
                case 1: BuildDemoChat(); break;
                case 2: BuildDemoCatalog(); break;
                case 3: BuildDemoPlugins(); break;
                case 4: BuildDemoSettings(); break;
                default: BuildDemoReady(); break;
            }
        }

        private Brush DemoAccent => (Brush)FindResource("ClaudeBrush");
        private Brush DemoLine => (Brush)FindResource("LineBrush");
        private Brush DemoInk => (Brush)FindResource("InkSoftBrush");

        private static DoubleAnimation DemoPulse(double from, double to, double seconds, double beginSeconds = 0) => new()
        {
            From = from, To = to, Duration = TimeSpan.FromSeconds(seconds),
            AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever, BeginTime = TimeSpan.FromSeconds(beginSeconds)
        };

        /// <summary>Σελίδα 1 - "Καλώς ήρθες": κεντρικός κόμβος με δορυφόρους γύρω του, ίδιο μοτίβο με το
        /// λογότυπο του Κόμβου - δείχνει την ιδέα "ένας κόμβος, πολλά εργαλεία".</summary>
        private void BuildDemoHub()
        {
            var center = new Point(160, 68);
            var satellites = new[] { new Point(70, 35), new Point(250, 35), new Point(70, 105), new Point(250, 105) };
            for (int i = 0; i < satellites.Length; i++)
            {
                var line = new Line { X1 = center.X, Y1 = center.Y, X2 = satellites[i].X, Y2 = satellites[i].Y, StrokeThickness = 1.5, Stroke = DemoLine, Opacity = 0.4 };
                line.BeginAnimation(Line.OpacityProperty, DemoPulse(0.25, 0.75, 1.1, i * 0.2));
                OnboardingDemoCanvas.Children.Add(line);

                var sat = new Ellipse { Width = 16, Height = 16, Fill = DemoAccent, Opacity = 0.85 };
                Canvas.SetLeft(sat, satellites[i].X - 8);
                Canvas.SetTop(sat, satellites[i].Y - 8);
                var scale = new ScaleTransform(1, 1);
                sat.RenderTransform = scale; sat.RenderTransformOrigin = new Point(0.5, 0.5);
                scale.BeginAnimation(ScaleTransform.ScaleXProperty, DemoPulse(0.85, 1.15, 1.1, i * 0.2));
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, DemoPulse(0.85, 1.15, 1.1, i * 0.2));
                OnboardingDemoCanvas.Children.Add(sat);
            }
            var hub = new Ellipse { Width = 30, Height = 30, Fill = DemoAccent };
            Canvas.SetLeft(hub, center.X - 15);
            Canvas.SetTop(hub, center.Y - 15);
            OnboardingDemoCanvas.Children.Add(hub);
            var label = new TextBlock { Text = "Κ", FontSize = 15, FontWeight = FontWeights.Bold, Foreground = Brushes.White };
            Canvas.SetLeft(label, center.X - 5);
            Canvas.SetTop(label, center.Y - 11);
            OnboardingDemoCanvas.Children.Add(label);
        }

        /// <summary>Σελίδα 2 - "Συνομιλία &amp; agents": μια γραμμή εισαγωγής που "γράφεται" και μια φούσκα
        /// απάντησης που εμφανίζεται, μαζί με δύο avatar που ανάβουν εναλλάξ (ιδέα σύγκρισης agents).</summary>
        private void BuildDemoChat()
        {
            var bubble = new Border { Width = 150, Height = 34, CornerRadius = new CornerRadius(8), Background = DemoLine, Opacity = 0 };
            Canvas.SetLeft(bubble, 20); Canvas.SetTop(bubble, 14);
            var bubbleFade = new DoubleAnimationUsingKeyFrames { RepeatBehavior = RepeatBehavior.Forever };
            bubbleFade.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            bubbleFade.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.6))));
            bubbleFade.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1.0))));
            bubbleFade.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1.8))));
            bubbleFade.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2.0))));
            bubble.BeginAnimation(UIElement.OpacityProperty, bubbleFade);
            OnboardingDemoCanvas.Children.Add(bubble);

            var inputBar = new Border { Width = 280, Height = 30, CornerRadius = new CornerRadius(15), Background = DemoLine };
            Canvas.SetLeft(inputBar, 20); Canvas.SetTop(inputBar, 96);
            OnboardingDemoCanvas.Children.Add(inputBar);

            var typedText = new Border { Height = 12, CornerRadius = new CornerRadius(2), Background = DemoAccent, Opacity = 0.7 };
            Canvas.SetLeft(typedText, 32); Canvas.SetTop(typedText, 105);
            var widthAnim = new DoubleAnimation { From = 0, To = 190, Duration = TimeSpan.FromSeconds(1.4), RepeatBehavior = RepeatBehavior.Forever };
            typedText.BeginAnimation(FrameworkElement.WidthProperty, widthAnim);
            OnboardingDemoCanvas.Children.Add(typedText);

            for (int i = 0; i < 2; i++)
            {
                var avatar = new Ellipse { Width = 22, Height = 22, Fill = i == 0 ? DemoAccent : DemoLine };
                Canvas.SetLeft(avatar, 260 + i * 28); Canvas.SetTop(avatar, 8);
                avatar.BeginAnimation(UIElement.OpacityProperty, DemoPulse(0.35, 1, 1.0, i * 1.0));
                OnboardingDemoCanvas.Children.Add(avatar);
            }
        }

        /// <summary>Σελίδα 3 - "Κατάλογος εργαλείων": έγχρωμες καρτέλες κατηγοριών με ένα ενεργό πλαίσιο που
        /// μετακινείται ανάμεσά τους, και ένα αστέρι που ανάβει/σβήνει (καρφίτσωμα).</summary>
        private void BuildDemoCatalog()
        {
            var colors = new[] { Color.FromRgb(0x4C, 0x8B, 0xC9), Color.FromRgb(0xB0, 0x4C, 0x8A), Color.FromRgb(0xC9, 0x8A, 0x33), Color.FromRgb(0x4C, 0xA3, 0x7A) };
            double x = 20;
            var highlight = new Border { Width = 62, Height = 26, CornerRadius = new CornerRadius(13), BorderBrush = DemoAccent, BorderThickness = new Thickness(2) };
            Canvas.SetTop(highlight, 15);
            for (int i = 0; i < colors.Length; i++)
            {
                var tab = new Border { Width = 62, Height = 26, CornerRadius = new CornerRadius(13), Background = new SolidColorBrush(colors[i]), Opacity = 0.85 };
                Canvas.SetLeft(tab, x); Canvas.SetTop(tab, 15);
                OnboardingDemoCanvas.Children.Add(tab);
                x += 74;
            }
            OnboardingDemoCanvas.Children.Add(highlight);
            var moveAnim = new DoubleAnimationUsingKeyFrames { RepeatBehavior = RepeatBehavior.Forever };
            for (int i = 0; i < colors.Length; i++)
                moveAnim.KeyFrames.Add(new DiscreteDoubleKeyFrame(20 + i * 74, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(i * 0.7))));
            moveAnim.KeyFrames.Add(new DiscreteDoubleKeyFrame(20, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(colors.Length * 0.7))));
            highlight.BeginAnimation(Canvas.LeftProperty, moveAnim);

            var star = new TextBlock { Text = "★", FontSize = 30, Foreground = DemoAccent };
            Canvas.SetLeft(star, 145); Canvas.SetTop(star, 68);
            star.BeginAnimation(UIElement.OpacityProperty, DemoPulse(0.25, 1, 0.9));
            OnboardingDemoCanvas.Children.Add(star);

            var tileHint = new Border { Width = 280, Height = 16, CornerRadius = new CornerRadius(3), Background = DemoLine };
            Canvas.SetLeft(tileHint, 20); Canvas.SetTop(tileHint, 108);
            OnboardingDemoCanvas.Children.Add(tileHint);
        }

        /// <summary>Σελίδα 4 - "Επεκτάσεις &amp; MCP": δύο κομμάτια-βύσματα γλιστράνε και ενώνονται στη μέση
        /// με μια μικρή "σπίθα" επαφής - η ιδέα της σύνδεσης ενός plugin/MCP server.</summary>
        private void BuildDemoPlugins()
        {
            var left = new Border { Width = 60, Height = 34, CornerRadius = new CornerRadius(6, 0, 0, 6), Background = DemoAccent };
            var right = new Border { Width = 60, Height = 34, CornerRadius = new CornerRadius(0, 6, 6, 0), Background = DemoLine };
            Canvas.SetTop(left, 53); Canvas.SetTop(right, 53);

            var leftX = new DoubleAnimationUsingKeyFrames { RepeatBehavior = RepeatBehavior.Forever };
            leftX.KeyFrames.Add(new EasingDoubleKeyFrame(40, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            leftX.KeyFrames.Add(new EasingDoubleKeyFrame(160, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.9))));
            leftX.KeyFrames.Add(new EasingDoubleKeyFrame(160, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1.6))));
            leftX.KeyFrames.Add(new EasingDoubleKeyFrame(40, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2.2))));
            left.BeginAnimation(Canvas.LeftProperty, leftX);

            var rightX = new DoubleAnimationUsingKeyFrames { RepeatBehavior = RepeatBehavior.Forever };
            rightX.KeyFrames.Add(new EasingDoubleKeyFrame(220, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            rightX.KeyFrames.Add(new EasingDoubleKeyFrame(220, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.9))));
            rightX.KeyFrames.Add(new EasingDoubleKeyFrame(220, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1.6))));
            rightX.KeyFrames.Add(new EasingDoubleKeyFrame(220, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2.2))));
            right.BeginAnimation(Canvas.LeftProperty, rightX);

            OnboardingDemoCanvas.Children.Add(left);
            OnboardingDemoCanvas.Children.Add(right);

            var spark = new Ellipse { Width = 24, Height = 24, Fill = Brushes.White, Opacity = 0 };
            Canvas.SetLeft(spark, 208); Canvas.SetTop(spark, 58);
            var sparkAnim = new DoubleAnimationUsingKeyFrames { RepeatBehavior = RepeatBehavior.Forever };
            sparkAnim.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.85))));
            sparkAnim.KeyFrames.Add(new EasingDoubleKeyFrame(0.9, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.95))));
            sparkAnim.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1.3))));
            spark.BeginAnimation(UIElement.OpacityProperty, sparkAnim);
            OnboardingDemoCanvas.Children.Add(spark);

            var slashHint = new TextBlock { Text = "/plugin", FontSize = 12, Foreground = DemoInk };
            Canvas.SetLeft(slashHint, 128); Canvas.SetTop(slashHint, 100);
            OnboardingDemoCanvas.Children.Add(slashHint);
        }

        /// <summary>Σελίδα 5 - "Ρυθμίσεις &amp; συντομεύσεις": ένα γρανάζι που γυρίζει συνέχεια και ένα
        /// πάνελ ρυθμίσεων που γλιστράει μέσα/έξω από τα δεξιά.</summary>
        private void BuildDemoSettings()
        {
            var gear = new Canvas { Width = 60, Height = 60, RenderTransformOrigin = new Point(0.5, 0.5) };
            var ring = new Ellipse { Width = 60, Height = 60, Stroke = DemoAccent, StrokeThickness = 5 };
            gear.Children.Add(ring);
            for (int t = 0; t < 8; t++)
            {
                double angle = t * (Math.PI * 2 / 8);
                var tooth = new Line
                {
                    X1 = 30 + Math.Cos(angle) * 30, Y1 = 30 + Math.Sin(angle) * 30,
                    X2 = 30 + Math.Cos(angle) * 38, Y2 = 30 + Math.Sin(angle) * 38,
                    Stroke = DemoAccent, StrokeThickness = 5
                };
                gear.Children.Add(tooth);
            }
            var rotate = new RotateTransform(0);
            gear.RenderTransform = rotate;
            rotate.BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation { From = 0, To = 360, Duration = TimeSpan.FromSeconds(4), RepeatBehavior = RepeatBehavior.Forever });
            Canvas.SetLeft(gear, 40); Canvas.SetTop(gear, 40);
            OnboardingDemoCanvas.Children.Add(gear);

            var panel = new Border { Width = 140, Height = 110, CornerRadius = new CornerRadius(8, 0, 0, 8), Background = DemoLine };
            Canvas.SetTop(panel, 15);
            var panelX = new DoubleAnimationUsingKeyFrames { RepeatBehavior = RepeatBehavior.Forever };
            panelX.KeyFrames.Add(new EasingDoubleKeyFrame(320, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            panelX.KeyFrames.Add(new EasingDoubleKeyFrame(180, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1))));
            panelX.KeyFrames.Add(new EasingDoubleKeyFrame(180, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2.4))));
            panelX.KeyFrames.Add(new EasingDoubleKeyFrame(320, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(3.2))));
            panel.BeginAnimation(Canvas.LeftProperty, panelX);
            OnboardingDemoCanvas.Children.Add(panel);

            for (int i = 0; i < 3; i++)
            {
                var line = new Border { Width = 90, Height = 8, CornerRadius = new CornerRadius(2), Background = DemoInk, Opacity = 0.5 };
                Canvas.SetTop(line, 32 + i * 24);
                var lineX = new DoubleAnimationUsingKeyFrames { RepeatBehavior = RepeatBehavior.Forever };
                lineX.KeyFrames.Add(new EasingDoubleKeyFrame(340, KeyTime.FromTimeSpan(TimeSpan.Zero)));
                lineX.KeyFrames.Add(new EasingDoubleKeyFrame(200, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1))));
                lineX.KeyFrames.Add(new EasingDoubleKeyFrame(200, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2.4))));
                lineX.KeyFrames.Add(new EasingDoubleKeyFrame(340, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(3.2))));
                line.BeginAnimation(Canvas.LeftProperty, lineX);
                OnboardingDemoCanvas.Children.Add(line);
            }
        }

        /// <summary>Σελίδα 6 - "Έτοιμος": ένας πύραυλος με φλόγα που ανεβαίνει και κομφετί που πέφτουν -
        /// γιορτινό κλείσιμο του οδηγού.</summary>
        private void BuildDemoReady()
        {
            var rocket = new TextBlock { Text = "🚀", FontSize = 46 };
            Canvas.SetLeft(rocket, 145);
            var rocketY = new DoubleAnimation { From = 55, To = 35, Duration = TimeSpan.FromSeconds(0.9), AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever };
            rocket.BeginAnimation(Canvas.TopProperty, rocketY);
            OnboardingDemoCanvas.Children.Add(rocket);

            var rnd = new Random(7);
            var confettiColors = new[] { DemoAccent, DemoLine, Brushes.SeaGreen };
            for (int i = 0; i < 10; i++)
            {
                var dot = new Border { Width = 6, Height = 6, CornerRadius = new CornerRadius(1), Background = confettiColors[i % confettiColors.Length] };
                double startX = 20 + rnd.Next(280);
                Canvas.SetLeft(dot, startX);
                var fall = new DoubleAnimation { From = -10, To = 150, Duration = TimeSpan.FromSeconds(1.6 + rnd.NextDouble()), RepeatBehavior = RepeatBehavior.Forever, BeginTime = TimeSpan.FromSeconds(rnd.NextDouble()) };
                dot.BeginAnimation(Canvas.TopProperty, fall);
                var fade = new DoubleAnimation { From = 0.9, To = 0.1, Duration = TimeSpan.FromSeconds(1.6 + rnd.NextDouble()), RepeatBehavior = RepeatBehavior.Forever };
                dot.BeginAnimation(UIElement.OpacityProperty, fade);
                OnboardingDemoCanvas.Children.Add(dot);
            }
        }

        private void OnboardingBackButton_Click(object sender, RoutedEventArgs e)
        {
            if (_onboardingPageIndex > 0) { _onboardingPageIndex--; RenderOnboardingPage(); }
        }

        private void OnboardingNextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_onboardingPageIndex < L.OnboardingPages.Length - 1) { _onboardingPageIndex++; RenderOnboardingPage(); }
            else HideOnboarding();
        }

        private void OnboardingSkipButton_Click(object sender, RoutedEventArgs e) => HideOnboarding();

        private void HideOnboarding()
        {
            OnboardingOverlay.Visibility = Visibility.Collapsed;
            AppShellBorder.Opacity = WindowOpacity; // επαναφορά της πραγματικής προτίμησης διαφάνειας του χρήστη
            if (_onboardingIsReplay) return; // ήδη έχει τρέξει το splash στην πρώτη εκκίνηση - μην το ξανατρέξεις

            _onboardingSeen = true;
            SaveSettings();

            if (_startMinimized)
            {
                SplashOverlay.Visibility = Visibility.Collapsed;
                Hide();
                return;
            }
            RunSplashAnimation();
        }

        // ---------- Settings persistence ----------
        private void LoadSettings()
        {
            var settings = SettingsStore.Load();
            L.Current = Enum.TryParse<Lang>(settings.Language, out var lang) ? lang : Lang.EL;
            _sidebarRight = settings.SidebarRight;
            _transparency = Enum.TryParse<TransparencyLevel>(settings.Transparency, out var tr) ? tr : TransparencyLevel.None;
            _bgStyle = Enum.TryParse<BgAnimStyle>(settings.BgStyle, out var bg) ? bg : BgAnimStyle.None;
            _centralAgentName = string.IsNullOrWhiteSpace(settings.CentralAgent) ? "Claude" : settings.CentralAgent;
            _skin = Enum.TryParse<WindowsSkin>(settings.Skin, out var sk) ? sk : WindowsSkin.Modern;
            _chatTextScale = Enum.TryParse<ChatTextScale>(settings.ChatTextScale, out var cts) ? cts : ChatTextScale.Normal;
            _customInstructions = settings.CustomInstructions ?? "";
            // Προεπιλογή "από το κουτί" ΜΟΝΟ την πρώτη φορά (όταν το πεδίο είναι ακόμα κενό, π.χ. φρέσκια
            // εγκατάσταση) - το επίσημο GitHub MCP connector του Anthropic. ΔΕΝ αγγίζει τιμή που έχει ήδη
            // αποθηκεύσει ο χρήστης. Σημείωση: χρειάζεται OAuth login στο GitHub που το Kombos δεν κάνει ακόμα
            // αυτόματα - βλ. McpServersHint.
            _mcpServersRaw = string.IsNullOrWhiteSpace(settings.McpServersRaw) && !settings.OnboardingSeen
                ? "GitHub|https://api.githubcopilot.com/mcp/"
                : settings.McpServersRaw ?? "";
            _conversationsSyncFolder = settings.ConversationsSyncFolder ?? "";
            _activeProjectPath = settings.ActiveProjectPath ?? "";
            _updateCheckRepo = settings.UpdateCheckRepo ?? "";
            _gatewayEnabled = settings.GatewayEnabled;
            _gatewayPort = settings.GatewayPort > 0 ? settings.GatewayPort : 8420;
            _autoContinueOnClaudeLimit = settings.AutoContinueOnClaudeLimit;
            _rememberedFallbackAgent = settings.RememberedFallbackAgent ?? "";
            _apiKeys.Clear();
            if (settings.ApiKeys != null)
                foreach (var kv in settings.ApiKeys) _apiKeys[kv.Key] = kv.Value;
            _agentUsernames.Clear();
            if (settings.AgentUsernames != null)
                foreach (var kv in settings.AgentUsernames) _agentUsernames[kv.Key] = kv.Value;
            _agentPasswords.Clear();
            if (settings.AgentPasswords != null)
                foreach (var kv in settings.AgentPasswords) _agentPasswords[kv.Key] = kv.Value;
            if (_apiKeys.TryGetValue("Claude", out var claudeKey)) _claude.ApiKey = claudeKey;
            _pinnedTools.Clear();
            if (settings.PinnedTools != null)
                foreach (var name in settings.PinnedTools) _pinnedTools.Add(name);
            _disabledPlugins.Clear();
            if (settings.DisabledPlugins != null)
                foreach (var name in settings.DisabledPlugins) _disabledPlugins.Add(name);
            _alwaysOnTop = settings.AlwaysOnTop;
            _startWithWindows = settings.StartWithWindows;
            _startMinimized = settings.StartMinimized;
            _soundOnReply = settings.SoundOnReply;
            _onboardingSeen = settings.OnboardingSeen;
            Topmost = _alwaysOnTop;

            // \u03a0\u03b1\u03bb\u03b9\u03ac \u03b1\u03c0\u03bf\u03b8\u03b7\u03ba\u03b5\u03c5\u03bc\u03ad\u03bd\u03b5\u03c2 \u03c1\u03c5\u03b8\u03bc\u03af\u03c3\u03b5\u03b9\u03c2 \u03b4\u03b5\u03bd \u03ad\u03c7\u03bf\u03c5\u03bd AppThemeMode - \u03bc\u03b5\u03c4\u03b1\u03bd\u03b1\u03c3\u03c4\u03b5\u03cd\u03b5\u03b9 \u03b1\u03c0\u03cc \u03c4\u03bf IsDark \u03c3\u03b5 \u03c7\u03b5\u03b9\u03c1\u03bf\u03ba\u03af\u03bd\u03b7\u03c4\u03bf
            // Light/Dark \u03ce\u03c3\u03c4\u03b5 \u03bd\u03b1 \u03bc\u03b7 \u03b3\u03c5\u03c1\u03af\u03c3\u03b5\u03b9 \u03c3\u03b9\u03c9\u03c0\u03b7\u03bb\u03ac \u03ba\u03b1\u03bd\u03b5\u03af\u03c2 \u03c3\u03b5 "\u03a3\u03cd\u03c3\u03c4\u03b7\u03bc\u03b1" \u03c7\u03c9\u03c1\u03af\u03c2 \u03bd\u03b1 \u03c4\u03bf \u03ad\u03c7\u03b5\u03b9 \u03b6\u03b7\u03c4\u03ae\u03c3\u03b5\u03b9.
            _themeMode = Enum.TryParse<AppThemeMode>(settings.ThemeMode, out var tm)
                ? tm
                : (settings.IsDark ? AppThemeMode.Dark : AppThemeMode.Light);
            ApplyThemeMode();
        }

        internal void SaveSettings()
        {
            SettingsStore.Save(new Services.AppSettings
            {
                Language = L.Current.ToString(),
                IsDark = _isDark,
                ThemeMode = _themeMode.ToString(),
                SidebarRight = _sidebarRight,
                Transparency = _transparency.ToString(),
                BgStyle = _bgStyle.ToString(),
                CentralAgent = _centralAgentName,
                Skin = _skin.ToString(),
                ChatTextScale = _chatTextScale.ToString(),
                CustomInstructions = _customInstructions,
                McpServersRaw = _mcpServersRaw,
                ConversationsSyncFolder = _conversationsSyncFolder,
                ActiveProjectPath = _activeProjectPath,
                UpdateCheckRepo = _updateCheckRepo,
                GatewayEnabled = _gatewayEnabled,
                GatewayPort = _gatewayPort,
                AutoContinueOnClaudeLimit = _autoContinueOnClaudeLimit,
                RememberedFallbackAgent = _rememberedFallbackAgent,
                ApiKeys = new Dictionary<string, string>(_apiKeys),
                AgentUsernames = new Dictionary<string, string>(_agentUsernames),
                AgentPasswords = new Dictionary<string, string>(_agentPasswords),
                PinnedTools = _pinnedTools.ToList(),
                DisabledPlugins = _disabledPlugins.ToList(),
                AlwaysOnTop = _alwaysOnTop,
                StartWithWindows = _startWithWindows,
                StartMinimized = _startMinimized,
                SoundOnReply = _soundOnReply,
                OnboardingSeen = _onboardingSeen
            });
        }

        // ---------- Theme ----------
        /// <summary>\u0391\u03bd\u03c4\u03b9\u03ba\u03b1\u03b8\u03b9\u03c3\u03c4\u03ac \u039c\u039f\u039d\u039f \u03c4\u03bf merged theme dictionary (Light.xaml/Dark.xaml - \u03c4\u03bf App.xaml \u03b4\u03b5\u03bd \u03ad\u03c7\u03b5\u03b9
        /// \u03ac\u03bb\u03bb\u03b1 \u03ba\u03bf\u03b9\u03bd\u03ac merged resources, \u03bf\u03c0\u03cc\u03c4\u03b5 \u03c4\u03bf Clear()+Add() \u03b5\u03af\u03bd\u03b1\u03b9 \u03b1\u03c3\u03c6\u03b1\u03bb\u03ad\u03c2) \u03ba\u03b1\u03b9 \u03b5\u03bd\u03b7\u03bc\u03b5\u03c1\u03ce\u03bd\u03b5\u03b9 \u03c4\u03bf \u03b5\u03b9\u03ba\u03bf\u03bd\u03af\u03b4\u03b9\u03bf
        /// \u03ae\u03bb\u03b9\u03bf\u03c5/\u03c6\u03b5\u03b3\u03b3\u03b1\u03c1\u03b9\u03bf\u03cd. \u0394\u0395\u039d \u03b1\u03b3\u03b3\u03af\u03b6\u03b5\u03b9 \u03c4\u03bf _themeMode - \u03b1\u03c5\u03c4\u03cc \u03b1\u03c0\u03bf\u03c6\u03b1\u03c3\u03af\u03b6\u03b5\u03b9 \u03bf \u03ba\u03b1\u03bb\u03ce\u03bd (\u03c7\u03b5\u03b9\u03c1\u03bf\u03ba\u03af\u03bd\u03b7\u03c4\u03b1/\u03c3\u03cd\u03c3\u03c4\u03b7\u03bc\u03b1).</summary>
        private void ApplyThemeDictionary(bool dark)
        {
            _isDark = dark;
            var uri = new Uri(dark ? "Themes/Dark.xaml" : "Themes/Light.xaml", UriKind.Relative);
            var dict = new ResourceDictionary { Source = uri };
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dict);
            // v4.6.0 - vector \u03b5\u03b9\u03ba\u03bf\u03bd\u03af\u03b4\u03b9\u03bf \u03b1\u03bd\u03c4\u03af \u03b3\u03b9\u03b1 emoji: \u03ae\u03bb\u03b9\u03bf\u03c2 (\u03bc\u03cc\u03bd\u03bf \u03c0\u03b5\u03c1\u03af\u03b3\u03c1\u03b1\u03bc\u03bc\u03b1) \u03c3\u03b5 light mode, \u03b3\u03b5\u03bc\u03ac\u03c4\u03bf \u03c6\u03b5\u03b3\u03b3\u03ac\u03c1\u03b9
            // \u03c3\u03b5 dark mode - \u03af\u03b4\u03b9\u03bf Path element, \u03bc\u03cc\u03bd\u03bf \u03b7 Data/Fill/Stroke \u03b1\u03bb\u03bb\u03ac\u03b6\u03bf\u03c5\u03bd.
            ThemeToggleIcon.Data = (Geometry)FindResource(dark ? "IconMoon" : "IconSun");
            ThemeToggleIcon.Fill = dark ? Brushes.White : Brushes.Transparent;
            ThemeToggleIcon.Stroke = dark ? null : Brushes.White;
            ThemeToggleIcon.StrokeThickness = dark ? 0 : 1.4;
        }

        /// <summary>\u039b\u03cd\u03bd\u03b5\u03b9 \u03c4\u03bf \u03c0\u03c1\u03b1\u03b3\u03bc\u03b1\u03c4\u03b9\u03ba\u03cc Light/Dark \u03b1\u03c0\u03cc \u03c4\u03bf _themeMode (Light/Dark = \u03c7\u03b5\u03b9\u03c1\u03bf\u03ba\u03af\u03bd\u03b7\u03c4\u03bf, System =
        /// \u03b4\u03b9\u03b1\u03b2\u03ac\u03b6\u03b5\u03b9 \u03c4\u03bf \u03c0\u03c1\u03b1\u03b3\u03bc\u03b1\u03c4\u03b9\u03ba\u03cc \u03b8\u03ad\u03bc\u03b1 \u03c4\u03c9\u03bd Windows) \u03ba\u03b1\u03b9 \u03c4\u03bf \u03b5\u03c6\u03b1\u03c1\u03bc\u03cc\u03b6\u03b5\u03b9. \u039a\u03b1\u03bb\u03b5\u03af\u03c4\u03b1\u03b9 \u03c3\u03c4\u03b7\u03bd \u03b5\u03ba\u03ba\u03af\u03bd\u03b7\u03c3\u03b7, \u03cc\u03c4\u03b1\u03bd \u03bf \u03c7\u03c1\u03ae\u03c3\u03c4\u03b7\u03c2
        /// \u03b1\u03bb\u03bb\u03ac\u03b6\u03b5\u03b9 \u03c4\u03b7\u03bd \u03b5\u03c0\u03b9\u03bb\u03bf\u03b3\u03ae \u03c3\u03c4\u03b9\u03c2 \u03a1\u03c5\u03b8\u03bc\u03af\u03c3\u03b5\u03b9\u03c2 \u0395\u03bc\u03c6\u03ac\u03bd\u03b9\u03c3\u03b7\u03c2, \u039a\u0391\u0399 \u03b6\u03c9\u03bd\u03c4\u03b1\u03bd\u03ac \u03cc\u03c4\u03b1\u03bd \u03c4\u03bf \u03af\u03b4\u03b9\u03bf \u03c4\u03bf \u03c3\u03cd\u03c3\u03c4\u03b7\u03bc\u03b1 \u03b1\u03bb\u03bb\u03ac\u03b6\u03b5\u03b9 \u03b8\u03ad\u03bc\u03b1
        /// \u03b5\u03bd\u03ce \u03b2\u03c1\u03b9\u03c3\u03ba\u03cc\u03bc\u03b1\u03c3\u03c4\u03b5 \u03c3\u03b5 System mode (\u03b2\u03bb. SystemEvents_UserPreferenceChanged).</summary>
        internal void ApplyThemeMode()
        {
            bool dark = _themeMode switch
            {
                AppThemeMode.Dark => true,
                AppThemeMode.Light => false,
                _ => DetectSystemDarkMode()
            };
            ApplyThemeDictionary(dark);
            ApplySkinPalette(); // \u03c4\u03bf Nexus (\u03ba\u03b1\u03b9 \u03c4\u03ce\u03c1\u03b1 \u03c4\u03bf Classic) \u03ad\u03c7\u03bf\u03c5\u03bd \u03be\u03b5\u03c7\u03c9\u03c1\u03b9\u03c3\u03c4\u03ae \u03c3\u03ba\u03bf\u03c4\u03b5\u03b9\u03bd\u03ae \u03c0\u03b1\u03bb\u03ad\u03c4\u03b1 - \u03c0\u03c1\u03ad\u03c0\u03b5\u03b9 \u03bd\u03b1 \u03be\u03b1\u03bd\u03b1\u03b5\u03c6\u03b1\u03c1\u03bc\u03bf\u03c3\u03c4\u03b5\u03af/\u03b1\u03c6\u03b1\u03b9\u03c1\u03b5\u03b8\u03b5\u03af \u03c3\u03b5 \u03ba\u03ac\u03b8\u03b5 \u03b5\u03bd\u03b1\u03bb\u03bb\u03b1\u03b3\u03ae \u03b8\u03ad\u03bc\u03b1\u03c4\u03bf\u03c2
        }

        private static bool DetectSystemDarkMode()
        {
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                return key?.GetValue("AppsUseLightTheme") is int i && i == 0;
            }
            catch { return false; } // \u03c0.\u03c7. \u03ba\u03bb\u03b5\u03b9\u03b4\u03af \u03b4\u03b5\u03bd \u03c5\u03c0\u03ac\u03c1\u03c7\u03b5\u03b9 \u03c3\u03b5 \u03c0\u03b1\u03bb\u03b9\u03cc\u03c4\u03b5\u03c1\u03b1 Windows - \u03b1\u03c3\u03c6\u03b1\u03bb\u03ae\u03c2 \u03c0\u03c1\u03bf\u03b5\u03c0\u03b9\u03bb\u03bf\u03b3\u03ae Light
        }

        private void SystemEvents_UserPreferenceChanged(object? sender, Microsoft.Win32.UserPreferenceChangedEventArgs e)
        {
            if (e.Category != Microsoft.Win32.UserPreferenceCategory.General) return;
            if (_themeMode != AppThemeMode.System) return;
            Dispatcher.Invoke(ApplyThemeMode);
        }

        /// <summary>\u03a4\u03bf \u03ba\u03bf\u03c5\u03bc\u03c0\u03af \u03ae\u03bb\u03b9\u03bf\u03c5/\u03c6\u03b5\u03b3\u03b3\u03b1\u03c1\u03b9\u03bf\u03cd \u03b5\u03af\u03bd\u03b1\u03b9 \u03c0\u03ac\u03bd\u03c4\u03b1 \u03bc\u03b9\u03b1 \u03a7\u0395\u0399\u03a1\u039f\u039a\u0399\u039d\u0397\u03a4\u0397 \u03b5\u03bd\u03ad\u03c1\u03b3\u03b5\u03b9\u03b1 - \u03b2\u03b3\u03ac\u03b6\u03b5\u03b9 \u03c4\u03bf\u03bd \u03c7\u03c1\u03ae\u03c3\u03c4\u03b7 \u03b1\u03c0\u03cc
        /// "\u03a3\u03cd\u03c3\u03c4\u03b7\u03bc\u03b1" mode \u03b1\u03bd \u03ae\u03c4\u03b1\u03bd \u03b5\u03bd\u03b5\u03c1\u03b3\u03cc, \u03b1\u03ba\u03c1\u03b9\u03b2\u03ce\u03c2 \u03cc\u03c0\u03c9\u03c2 \u03c3\u03b5 \u03ba\u03ac\u03b8\u03b5 \u03ac\u03bb\u03bb\u03b7 \u03b5\u03c6\u03b1\u03c1\u03bc\u03bf\u03b3\u03ae \u03cc\u03c0\u03bf\u03c5 \u03c4\u03bf \u03c7\u03b5\u03b9\u03c1\u03bf\u03ba\u03af\u03bd\u03b7\u03c4\u03bf toggle
        /// \u03c5\u03c0\u03b5\u03c1\u03b9\u03c3\u03c7\u03cd\u03b5\u03b9 \u03c4\u03b7\u03c2 \u03b1\u03c5\u03c4\u03cc\u03bc\u03b1\u03c4\u03b7\u03c2 \u03c0\u03b1\u03c1\u03b1\u03ba\u03bf\u03bb\u03bf\u03cd\u03b8\u03b7\u03c3\u03b7\u03c2 \u03c3\u03c5\u03c3\u03c4\u03ae\u03bc\u03b1\u03c4\u03bf\u03c2.</summary>
        internal void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            _themeMode = _isDark ? AppThemeMode.Light : AppThemeMode.Dark;
            ApplyThemeMode();
            SaveSettings();
        }

        // ---------- Language ----------
        private void LangToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (LangOptionsPanel.Children.Count == 0) BuildLangOptions();
            LangSelectPopup.PlacementTarget = StablePlacementTarget(sender, LangToggleButton);
            ConfigurePopupPlacement(LangSelectPopup);
            LangSelectPopup.IsOpen = !LangSelectPopup.IsOpen;
        }

        /// <summary>ΔΙΟΡΘΩΣΗ: πριν αγκύρωνε πάντα στο κουμπί της πλευρικής μπάρας - στο skin "Windows Classic"
        /// η μπάρα είναι κρυμμένη και η ενέργεια έρχεται από MenuItem του classic menu bar αντί για αυτήν.
        /// Το ίδιο το MenuItem ΔΕΝ είναι ασφαλές PlacementTarget: μόλις κλείσει το dropdown του Menu που το
        /// φιλοξενεί, αποσυνδέεται από το visual tree και το Popup αποτυγχάνει σιωπηλά να ανοίξει (καθόλου
        /// σφάλμα, απλά ποτέ δεν εμφανίζεται). Γι' αυτό όταν το ενεργό skin είναι Classic αγκυρώνει πάντα στο
        /// ίδιο το ClassicMenuBar (μόνιμο μέρος του visual tree), όχι στο παροδικό MenuItem.</summary>
        private UIElement StablePlacementTarget(object sender, UIElement fallback) =>
            _skin == WindowsSkin.Classic ? ClassicMenuBar : (sender as UIElement ?? fallback);

        /// <summary>ΔΙΟΡΘΩΣΗ: το Placement="Right" (ρυθμισμένο στο XAML για αγκύρωση δίπλα σε στενό κουμπί
        /// πλευρικής μπάρας) υπολογίζει τη θέση σχετικά με ΟΛΟ το bounding rectangle του PlacementTarget. Στο
        /// Windows Classic το target είναι πλέον το ClassicMenuBar (μόνιμο, ολόκληρου πλάτους παραθύρου βλ.
        /// StablePlacementTarget), οπότε "Right" έβγαζε το popup πολύ δεξιά, συχνά έξω από τα όρια του
        /// παραθύρου. Σε Classic βάζουμε πάντα Bottom (κάτω από το menu bar, στην αριστερή άκρη) αντί για Right.</summary>
        private void ConfigurePopupPlacement(Popup popup)
        {
            if (_skin == WindowsSkin.Classic)
            {
                popup.Placement = PlacementMode.Bottom;
                popup.HorizontalOffset = 4;
                popup.VerticalOffset = 0;
            }
            else
            {
                popup.Placement = PlacementMode.Right;
                popup.HorizontalOffset = 0;
                popup.VerticalOffset = 0;
            }
        }

        private void BuildLangOptions()
        {
            LangOptionsPanel.Children.Clear();
            foreach (Lang lang in Enum.GetValues(typeof(Lang)))
            {
                var row = new Border { Padding = new Thickness(8, 6, 10, 6), Cursor = Cursors.Hand, CornerRadius = new CornerRadius(4) };
                var content = new StackPanel { Orientation = Orientation.Horizontal };
                content.Children.Add(BuildFlag(lang));
                var label = new TextBlock { Text = LangName(lang), Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center, FontSize = 12.5 };
                label.SetResourceReference(TextBlock.ForegroundProperty, "InkBrush");
                content.Children.Add(label);
                row.Child = content;

                row.MouseEnter += (s, e) => row.SetResourceReference(Border.BackgroundProperty, "PaperBrush");
                row.MouseLeave += (s, e) => row.Background = Brushes.Transparent;
                row.MouseLeftButtonUp += (s, e) =>
                {
                    L.Current = lang;
                    ApplyLanguage();
                    SaveSettings();
                    LangSelectPopup.IsOpen = false;
                };

                LangOptionsPanel.Children.Add(row);
            }
        }

        private static string LangName(Lang lang) => lang switch
        {
            Lang.EL => "Ελληνικά",
            Lang.EN => "English",
            Lang.FR => "Français",
            _ => "Deutsch"
        };

        private void ApplyLanguage()
        {
            Title = L.WindowTitle;
            CustomTitleText.Text = L.WindowTitle;
            SplashTitleText.Text = L.HeaderTitle;
            SplashVersionText.Text = "v" + L.AppVersion;
            HeaderTitleText.Text = L.HeaderTitle;
            VersionText.Text = "v" + L.AppVersion;
            HeaderSubtitleText.Text = L.HeaderSubtitle(_centralAgentName);
            ApiKeyHintText.Text = L.ApiKeyHint;
            SendButton.Content = L.SendButton;
            TaskLabel.Text = _sending ? L.Thinking : L.Ready;
            BreakdownButton.Content = L.PerToolBreakdown;
            NewChatButton.Content = L.NewChatButton;
            HistoryButton.Content = L.HistoryButtonLabel;
            ThemeToggleButton.ToolTip = L.ThemeToggleTooltip;
            LangToggleButton.ToolTip = L.LangToggleTooltip;
            ThemeToggleLabelText.Text = L.SidebarThemeLabel;
            LangToggleLabelText.Text = L.SidebarLangLabel;
            AppearanceLabelText.Text = L.SidebarAppearanceLabel;
            ArtifactsLabelText.Text = L.SidebarArtifactsLabel;
            ProjectsSidebarLabelText.Text = L.SidebarProjectsLabel;
            SettingsGearLabelText.Text = L.SidebarSettingsLabel;
            DetailsOpenBtn.Content = L.OpenFreeVersion;
            DetailsMcpBtn.Content = L.AddToClaudeDesktop;
            DetailsPricingBtn.Content = L.UpgradePlan;

            var previousCategory = _activeCategoryId ?? Catalog.Categories[0].Id;
            BuildCategories();
            BuildCategoryTabs();
            if (string.IsNullOrEmpty(_toolSearchFilter)) SelectCategory(previousCategory);

            UpdateStatusBar();
            if (BreakdownPopup.IsOpen) RefreshBreakdownPanel();
            if (_currentTool != null && DetailsPanel.Visibility == Visibility.Visible) ShowDetails(_currentTool);
            _settingsWindow?.ApplyTexts();
            _appearanceWindow?.ApplyTexts();
            _trayIcon?.ApplyTexts(L.WindowTitle, L.TrayOpen, L.TrayNewChat, L.TrayAccounts, L.TrayExit);
            ScrollToBottomButton.ToolTip = L.ScrollToBottomTooltip;
            ToolSearchBox.ToolTip = L.ToolSearchPlaceholder;
            OnboardingSkipButton.Content = L.OnboardingSkip;
            if (OnboardingOverlay.Visibility == Visibility.Visible) RenderOnboardingPage();

            if (_history.Count == 0)
            {
                ChatLogPanel.Children.Clear();
                AddChatBubble(false, L.Greeting(_centralAgentName));
            }
        }

        internal void NewChatButton_Click(object sender, RoutedEventArgs e)
        {
            SaveDraft();
            _sendCts?.Cancel();
            _conversationEpoch++;
            _history.Clear();
            ChatLogPanel.Children.Clear();
            _lastRegenerateButton = null;
            _lastAssistantRow = null;
            _currentConversationId = Guid.NewGuid().ToString("N");
            // Αν είναι επιλεγμένο ένα Project (chip) όταν ξεκινά η νέα συζήτηση, ανατίθεται αυτόματα σε αυτό -
            // ίδια συμπεριφορά με το Claude Desktop όταν ξεκινάς μια νέα συζήτηση μέσα σε ένα Project.
            _pendingNewChatProjectId = _activeProjectFilterId;
            RemoveAttachmentButton_Click(sender, e);
            AddChatBubble(false, L.Greeting(_centralAgentName));
            RestoreDraft();
        }

        private void SaveDraft()
        {
            var text = ChatInputBox.Text;
            if (string.IsNullOrEmpty(text)) _drafts.Remove(_currentConversationId);
            else _drafts[_currentConversationId] = text;
        }

        private void RestoreDraft()
        {
            ChatInputBox.Text = _drafts.TryGetValue(_currentConversationId, out var draft) ? draft : "";
        }

        // ---------- Πολλαπλές αποθηκευμένες συζητήσεις ----------
        internal void SaveConversations() => ConversationStore.Save(_conversations, _conversationsSyncFolder);
        internal void SaveProjects() => ProjectsListStore.Save(_projects, _conversationsSyncFolder);

        /// <summary>Αλλάζει τον φάκελο συγχρονισμού συζητήσεων (Ρυθμίσεις → Project) - "συνεργασία ανάμεσα σε
        /// υπολογιστές" χωρίς δικό μας backend: βάλε έναν φάκελο OneDrive/Dropbox/δικτύου εδώ σε όλους τους
        /// υπολογιστές σου και θα μοιράζονται το ΙΔΙΟ conversations.json. Αν ο νέος φάκελος έχει ήδη
        /// αρχείο (π.χ. από άλλο μηχάνημα), αυτό φορτώνεται - ΔΕΝ γίνεται συγχώνευση με το τρέχον.</summary>
        internal void ChangeConversationsSyncFolder(string newFolder)
        {
            _conversationsSyncFolder = newFolder;
            _conversations = ConversationStore.Load(_conversationsSyncFolder);
            _projects = ProjectsListStore.Load(_conversationsSyncFolder);
            SetupSyncFolderWatcher();
            SaveSettings();
            BuildHistoryList();
            BuildProjectChips();
        }

        private void SaveCurrentConversation()
        {
            if (_history.Count == 0) return;
            var firstUserTurn = _history.FirstOrDefault(t => t.Role == "user")?.Content ?? "";
            var title = string.IsNullOrWhiteSpace(firstUserTurn)
                ? L.NewChatButton
                : (firstUserTurn.Length > 46 ? firstUserTurn[..46] + "…" : firstUserTurn);

            var existing = _conversations.FirstOrDefault(c => c.Id == _currentConversationId);
            if (existing == null)
            {
                existing = new Conversation { Id = _currentConversationId, ProjectId = _pendingNewChatProjectId };
                _conversations.Insert(0, existing);
            }
            existing.Title = title;
            existing.UpdatedAtUtc = DateTime.UtcNow.ToString("O");
            existing.History = _history.Select(t => new ChatTurn { Role = t.Role, Content = t.Content }).ToList();

            SaveConversations();
        }

        // ---------- Artifacts: λίστα μπλοκ κώδικα από όλες τις συζητήσεις (σαν Claude Desktop) ----------
        private static readonly Regex ArtifactFenceRegex = new(@"```([a-zA-Z0-9_+\-]*)\r?\n([\s\S]*?)```", RegexOptions.Compiled);

        internal void ArtifactsButton_Click(object sender, RoutedEventArgs e)
        {
            BuildArtifactsList();
            ArtifactsPopup.PlacementTarget = StablePlacementTarget(sender, ArtifactsButton);
            ConfigurePopupPlacement(ArtifactsPopup);
            ArtifactsPopup.IsOpen = !ArtifactsPopup.IsOpen;
        }

        private List<(string Source, string Language, string Code)> ExtractAllArtifacts()
        {
            var found = new List<(string, string, string)>();

            void ScanTurns(IEnumerable<ChatTurn> turns, string source)
            {
                foreach (var turn in turns)
                {
                    if (turn.Role != "assistant") continue;
                    foreach (Match m in ArtifactFenceRegex.Matches(turn.Content))
                    {
                        var lang = m.Groups[1].Value;
                        var code = m.Groups[2].Value.TrimEnd('\n', '\r');
                        if (code.Trim().Length == 0) continue;
                        found.Add((source, string.IsNullOrEmpty(lang) ? "text" : lang, code));
                    }
                }
            }

            ScanTurns(_history, L.ArtifactCurrentChat);
            foreach (var conv in _conversations.OrderByDescending(c => c.UpdatedAtUtc))
            {
                if (conv.Id == _currentConversationId) continue; // ήδη σαρώθηκε ως "τρέχουσα" παραπάνω
                ScanTurns(conv.History, conv.Title);
            }

            return found;
        }

        private void BuildArtifactsList()
        {
            ArtifactsTitleText.Text = L.ArtifactsTitle;
            ArtifactsListPanel.Children.Clear();

            var artifacts = ExtractAllArtifacts();
            if (artifacts.Count == 0)
            {
                ArtifactsListPanel.Children.Add(new TextBlock
                {
                    Text = L.ArtifactsEmpty, FontSize = 11, TextWrapping = TextWrapping.Wrap,
                    Foreground = (Brush)FindResource("InkSoftBrush")
                });
                return;
            }

            foreach (var (source, language, code) in artifacts)
            {
                var card = new Border
                {
                    CornerRadius = new CornerRadius(5), Padding = new Thickness(8), Margin = new Thickness(0, 0, 0, 8),
                    BorderThickness = new Thickness(1)
                };
                card.SetResourceReference(Border.BackgroundProperty, "PaperBrush");
                card.SetResourceReference(Border.BorderBrushProperty, "LineBrush");

                var stack = new StackPanel();

                var headerRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 4) };
                var langTag = new Border { CornerRadius = new CornerRadius(3), Padding = new Thickness(5, 1, 5, 1), Margin = new Thickness(0, 0, 6, 0) };
                langTag.SetResourceReference(Border.BackgroundProperty, "ClaudeBrush");
                langTag.Child = new TextBlock { Text = language, FontSize = 9.5, FontWeight = FontWeights.Bold, Foreground = Brushes.White };
                headerRow.Children.Add(langTag);
                var sourceText = new TextBlock
                {
                    Text = source, FontSize = 10.5, VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis, MaxWidth = 280
                };
                sourceText.SetResourceReference(TextBlock.ForegroundProperty, "InkSoftBrush");
                headerRow.Children.Add(sourceText);
                stack.Children.Add(headerRow);

                var lines = code.Split('\n');
                var preview = string.Join('\n', lines.Take(6));
                if (lines.Length > 6) preview += "\n…";
                var previewText = new TextBlock
                {
                    Text = preview, FontFamily = new FontFamily("Consolas"), FontSize = 10.5,
                    TextWrapping = TextWrapping.NoWrap
                };
                previewText.SetResourceReference(TextBlock.ForegroundProperty, "InkBrush");
                var previewScroll = new ScrollViewer { HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Disabled, MaxHeight = 110 };
                previewScroll.Content = previewText;
                stack.Children.Add(previewScroll);

                var actionRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 6, 0, 0), HorizontalAlignment = HorizontalAlignment.Right };
                var copyBtn = new Button
                {
                    Content = L.CopyButton, FontSize = 10, Padding = new Thickness(6, 2, 6, 2),
                    Margin = new Thickness(0, 0, 8, 0), Background = Brushes.Transparent, BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand, Opacity = 0.8
                };
                copyBtn.SetResourceReference(Button.ForegroundProperty, "InkSoftBrush");
                copyBtn.Click += (s, e) => { try { Clipboard.SetText(code); copyBtn.Content = L.CopiedLabel; } catch { } };
                actionRow.Children.Add(copyBtn);

                var saveBtn = new Button
                {
                    Content = L.ArtifactSaveAs, FontSize = 10, Padding = new Thickness(6, 2, 6, 2),
                    Background = Brushes.Transparent, BorderThickness = new Thickness(0), Cursor = Cursors.Hand, Opacity = 0.8
                };
                saveBtn.SetResourceReference(Button.ForegroundProperty, "InkSoftBrush");
                saveBtn.Click += (s, e) =>
                {
                    var dialog = new Microsoft.Win32.SaveFileDialog { FileName = $"artifact.{GuessExtension(language)}" };
                    if (dialog.ShowDialog(this) != true) return;
                    try { System.IO.File.WriteAllText(dialog.FileName, code); }
                    catch (Exception ex) { MessageBox.Show(this, ex.Message, L.LinkOpenError); }
                };
                actionRow.Children.Add(saveBtn);

                if (language.ToLowerInvariant() is "html" or "htm" or "svg" or "markdown" or "md")
                {
                    var previewBtn = new Button
                    {
                        Content = L.ArtifactPreviewButton, FontSize = 10, Padding = new Thickness(6, 2, 6, 2),
                        Margin = new Thickness(0, 0, 8, 0), Background = Brushes.Transparent, BorderThickness = new Thickness(0),
                        Cursor = Cursors.Hand, Opacity = 0.9
                    };
                    previewBtn.SetResourceReference(Button.ForegroundProperty, "ClaudeBrush");
                    var capturedLanguage = language;
                    var capturedCode = code;
                    previewBtn.Click += (s, e) => new ArtifactPreviewWindow(this, capturedLanguage, capturedCode).Show();
                    actionRow.Children.Insert(0, previewBtn);
                }

                stack.Children.Add(actionRow);
                card.Child = stack;
                ArtifactsListPanel.Children.Add(card);
            }
        }

        private static string GuessExtension(string language) => language.ToLowerInvariant() switch
        {
            "csharp" or "cs" => "cs",
            "python" or "py" => "py",
            "javascript" or "js" => "js",
            "typescript" or "ts" => "ts",
            "html" => "html",
            "css" => "css",
            "json" => "json",
            "xml" => "xml",
            "sql" => "sql",
            "bash" or "sh" or "shell" => "sh",
            "powershell" or "ps1" => "ps1",
            "java" => "java",
            "cpp" or "c++" => "cpp",
            "yaml" or "yml" => "yaml",
            _ => "txt"
        };

        private void HistoryButton_Click(object sender, RoutedEventArgs e) => ToggleHistoryPopup(HistoryButton);

        /// <summary>Το ίδιο popup ιστορικού ανοίγει είτε από το κουμπί στη γραμμή κατάστασης είτε από το
        /// εικονίδιο "Συζητήσεις" στην πλευρική μπάρα - δείχνει ΟΛΕΣ τις αποθηκευμένες συζητήσεις (τρέχουσες
        /// και παλαιότερες), αναζητήσιμες, όχι μόνο τις ενεργές.</summary>
        internal void ToggleHistoryPopup(UIElement placementTarget)
        {
            HistorySearchBox.TextChanged -= HistorySearchBox_TextChanged;
            HistorySearchBox.Text = "";
            HistorySearchBox.ToolTip = L.HistorySearchPlaceholder;
            HistorySearchBox.TextChanged += HistorySearchBox_TextChanged;
            BuildProjectChips();
            BuildHistoryList();
            HistoryPopup.PlacementTarget = placementTarget;
            HistoryPopup.IsOpen = !HistoryPopup.IsOpen;
        }

        /// <summary>Projects (v3.9.0, στυλ Claude Desktop): οριζόντια σειρά από "chips" πάνω από τη λίστα
        /// συζητήσεων - "Όλες" (χωρίς φίλτρο) + ένα ανά Project + ένα "+" για δημιουργία νέου. Επιλέγοντας ένα
        /// Project φιλτράρει τη λίστα από κάτω ΚΑΙ γίνεται το προεπιλεγμένο project για κάθε νέα συζήτηση όσο
        /// παραμένει επιλεγμένο (βλ. NewChatButton_Click).</summary>
        private void BuildProjectChips()
        {
            ProjectsSectionLabel.Text = L.ProjectsSectionLabel;
            ProjectChipsPanel.Children.Clear();

            Border MakeChip(string label, bool active, Action onClick)
            {
                var chip = new Border
                {
                    CornerRadius = new CornerRadius(11), Padding = new Thickness(10, 4, 10, 4),
                    Margin = new Thickness(0, 0, 6, 0), Cursor = Cursors.Hand
                };
                if (active) { chip.SetResourceReference(Border.BackgroundProperty, "ClaudeBrush"); }
                else { chip.Background = Brushes.Transparent; chip.SetResourceReference(Border.BorderBrushProperty, "LineBrush"); chip.BorderThickness = new Thickness(1); }
                chip.Child = new TextBlock { Text = label, FontSize = 10.5, Foreground = active ? Brushes.White : (Brush)FindResource("InkBrush") };
                chip.MouseLeftButtonUp += (s, e) => onClick();
                return chip;
            }

            ProjectChipsPanel.Children.Add(MakeChip(L.ProjectsAllChip, _activeProjectFilterId == null, () =>
            {
                _activeProjectFilterId = null;
                BuildProjectChips();
                BuildHistoryList();
            }));

            foreach (var project in _projects)
            {
                var capturedId = project.Id;
                ProjectChipsPanel.Children.Add(MakeChip(project.Name, _activeProjectFilterId == capturedId, () =>
                {
                    _activeProjectFilterId = capturedId;
                    BuildProjectChips();
                    BuildHistoryList();
                }));
            }

            var addChip = new Border
            {
                CornerRadius = new CornerRadius(11), Padding = new Thickness(10, 4, 10, 4),
                Background = Brushes.Transparent, BorderThickness = new Thickness(1), Cursor = Cursors.Hand
            };
            addChip.SetResourceReference(Border.BorderBrushProperty, "ClaudeBrush");
            addChip.Child = new TextBlock { Text = L.ProjectsNewButton, FontSize = 10.5 };
            ((TextBlock)addChip.Child).SetResourceReference(TextBlock.ForegroundProperty, "ClaudeBrush");
            addChip.MouseLeftButtonUp += (s, e) => PromptNewProject();
            ProjectChipsPanel.Children.Add(addChip);
        }

        /// <summary>Ζητά όνομα για νέο Project μέσα από ένα μικρό ενσωματωμένο TextBox (αντί για ξεχωριστό
        /// παράθυρο διαλόγου) - ίδιο ελαφρύ πνεύμα με το rename συζήτησης λίγο πιο κάτω.</summary>
        private void PromptNewProject()
        {
            // ΔΙΟΡΘΩΣΗ: μέσα σε οριζόντιο StackPanel χωρίς πλάτος, ένα άδειο TextBox παίρνει σχεδόν μηδενικό
            // αρχικό πλάτος (μόνο το Padding) - φαινόταν σαν να μην υπάρχει καθόλου χώρος για να γραφτεί
            // τίτλος. Σταθερό MinWidth + BringIntoView (η γονική ScrollViewer δεν ακολουθεί μόνη της τον
            // δρομέα) λύνουν και τα δύο.
            var inputBox = new TextBox { FontSize = 11, MinWidth = 180, Padding = new Thickness(6), Margin = new Thickness(0, 0, 0, 8) };
            ProjectChipsPanel.Children.Insert(0, inputBox);
            inputBox.Focus();
            inputBox.BringIntoView();

            void Commit()
            {
                var name = inputBox.Text.Trim();
                if (!string.IsNullOrEmpty(name))
                {
                    var project = new KombosProject { Name = name };
                    _projects.Add(project);
                    SaveProjects();
                    _activeProjectFilterId = project.Id;
                }
                BuildProjectChips();
            }
            inputBox.LostFocus += (s, e) => Commit();
            inputBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter) Commit();
                else if (e.Key == Key.Escape) BuildProjectChips();
            };
        }

        internal void ProjectsSidebarButton_Click(object sender, RoutedEventArgs e) => ToggleHistoryPopup(StablePlacementTarget(sender, ProjectsSidebarButton));

        private void HistorySearchBox_TextChanged(object sender, TextChangedEventArgs e) => BuildHistoryList();

        private void BuildHistoryList()
        {
            HistoryTitleText.Text = L.HistoryTitle;
            HistoryListPanel.Children.Clear();

            if (_conversations.Count == 0)
            {
                HistoryListPanel.Children.Add(new TextBlock
                {
                    Text = L.HistoryEmpty, FontSize = 11, TextWrapping = TextWrapping.Wrap,
                    Foreground = (Brush)FindResource("InkSoftBrush")
                });
                return;
            }

            var filter = HistorySearchBox.Text?.Trim() ?? "";
            var filtered = _conversations.OrderByDescending(c => c.UpdatedAtUtc)
                .Where(c => _activeProjectFilterId == null || c.ProjectId == _activeProjectFilterId)
                .Where(c => filter.Length == 0
                    || c.Title.Contains(filter, StringComparison.OrdinalIgnoreCase)
                    || c.History.Any(t => t.Content.Contains(filter, StringComparison.OrdinalIgnoreCase)));

            bool anyMatch = false;
            foreach (var conv in filtered)
            {
                anyMatch = true;
                var row = new Grid { Margin = new Thickness(0, 0, 0, 4) };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var item = new Border
                {
                    Padding = new Thickness(8, 6, 8, 6), CornerRadius = new CornerRadius(5), Cursor = Cursors.Hand,
                    Background = conv.Id == _currentConversationId ? (Brush)FindResource("PaperBrush") : Brushes.Transparent
                };
                var itemText = new TextBlock
                {
                    Text = conv.Title, FontSize = 11.5, TextWrapping = TextWrapping.NoWrap,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                itemText.SetResourceReference(TextBlock.ForegroundProperty, "InkBrush");
                item.Child = itemText;
                item.MouseLeftButtonUp += (s, e) => LoadConversation(conv.Id);
                Grid.SetColumn(item, 0);

                var renameButton = new Button
                {
                    Content = "✏", FontSize = 11, Width = 26, Padding = new Thickness(0),
                    Background = Brushes.Transparent, BorderThickness = new Thickness(0), Cursor = Cursors.Hand,
                    Foreground = (Brush)FindResource("InkSoftBrush"), ToolTip = L.RenameButton
                };
                renameButton.Click += (s, e) =>
                {
                    var renameBox = new TextBox
                    {
                        Text = conv.Title, FontSize = 11.5, Padding = new Thickness(2)
                    };
                    item.Child = renameBox;
                    renameBox.Focus();
                    renameBox.SelectAll();
                    void Commit()
                    {
                        var newTitle = renameBox.Text.Trim();
                        if (!string.IsNullOrEmpty(newTitle)) RenameConversation(conv.Id, newTitle);
                        else BuildHistoryList();
                    }
                    renameBox.LostFocus += (s2, e2) => Commit();
                    renameBox.KeyDown += (s2, e2) =>
                    {
                        if (e2.Key == Key.Enter) Commit();
                        else if (e2.Key == Key.Escape) BuildHistoryList();
                    };
                };
                Grid.SetColumn(renameButton, 1);

                var duplicateButton = new Button
                {
                    Content = "⧉", FontSize = 11, Width = 26, Padding = new Thickness(0),
                    Background = Brushes.Transparent, BorderThickness = new Thickness(0), Cursor = Cursors.Hand,
                    Foreground = (Brush)FindResource("InkSoftBrush"), ToolTip = L.DuplicateButton
                };
                duplicateButton.Click += (s, e) => DuplicateConversation(conv.Id);
                Grid.SetColumn(duplicateButton, 2);

                var detachButton = new Button
                {
                    Content = "⇱", FontSize = 11, Width = 26, Padding = new Thickness(0),
                    Background = Brushes.Transparent, BorderThickness = new Thickness(0), Cursor = Cursors.Hand,
                    Foreground = (Brush)FindResource("InkSoftBrush"), ToolTip = L.DetachButton
                };
                detachButton.Click += (s, e) => new ConversationViewerWindow(this, conv).Show();
                Grid.SetColumn(detachButton, 3);

                var deleteButton = new Button
                {
                    Content = "🗑", FontSize = 11, Width = 26, Padding = new Thickness(0),
                    Background = Brushes.Transparent, BorderThickness = new Thickness(0), Cursor = Cursors.Hand,
                    Foreground = (Brush)FindResource("InkSoftBrush")
                };
                deleteButton.Click += (s, e) => DeleteConversation(conv.Id);
                Grid.SetColumn(deleteButton, 4);

                row.Children.Add(item);
                row.Children.Add(renameButton);
                row.Children.Add(duplicateButton);
                row.Children.Add(detachButton);
                row.Children.Add(deleteButton);
                HistoryListPanel.Children.Add(row);
            }

            if (!anyMatch)
            {
                HistoryListPanel.Children.Add(new TextBlock
                {
                    Text = L.HistoryNoResults, FontSize = 11, TextWrapping = TextWrapping.Wrap,
                    Foreground = (Brush)FindResource("InkSoftBrush")
                });
            }
        }

        private void LoadConversation(string conversationId)
        {
            var conv = _conversations.FirstOrDefault(c => c.Id == conversationId);
            if (conv == null) return;

            SaveDraft();
            _sendCts?.Cancel();
            _conversationEpoch++;
            _currentConversationId = conv.Id;
            _history.Clear();
            _history.AddRange(conv.History.Select(t => new ChatTurn { Role = t.Role, Content = t.Content }));
            ChatLogPanel.Children.Clear();
            _lastRegenerateButton = null;
            _lastAssistantRow = null;
            // Περνάμε τα ΙΔΙΑ αντικείμενα ChatTurn που μόλις μπήκαν στο _history (όχι τα πρωτότυπα του
            // conv.History) - το κουμπί διαγραφής κάθε φούσκας κάνει _history.Remove(αναφορά), οπότε πρέπει να
            // δείχνει στο ίδιο αντικείμενο που όντως βρίσκεται μέσα στη λίστα.
            foreach (var turn in _history)
                AddChatBubble(turn.Role == "user", turn.Content, historyTurn: turn);
            RestoreDraft();

            HistoryPopup.IsOpen = false;
        }

        private void DeleteConversation(string conversationId)
        {
            _conversations.RemoveAll(c => c.Id == conversationId);
            SaveConversations();
            if (conversationId == _currentConversationId) NewChatButton_Click(this, new RoutedEventArgs());
            BuildHistoryList();
        }

        private void DuplicateConversation(string conversationId)
        {
            var conv = _conversations.FirstOrDefault(c => c.Id == conversationId);
            if (conv == null) return;
            var copy = new Conversation
            {
                Id = Guid.NewGuid().ToString("N"),
                Title = L.DuplicateTitle(conv.Title),
                History = conv.History.Select(t => new ChatTurn { Role = t.Role, Content = t.Content }).ToList(),
                UpdatedAtUtc = DateTime.UtcNow.ToString("O"),
                ProjectId = conv.ProjectId
            };
            _conversations.Insert(0, copy);
            SaveConversations();
            BuildHistoryList();
        }

        /// <summary>"Fork" (κλάδεμα, ιδέα από LibreChat's conversation forking, v4.1.0): νέα, ανεξάρτητη
        /// συζήτηση που περιέχει το ιστορικό ΜΕΧΡΙ ΚΑΙ το συγκεκριμένο μήνυμα - χρήσιμο για να δοκιμάσεις μια
        /// διαφορετική συνέχεια από ένα σημείο χωρίς να πειράξεις ή να χάσεις την αρχική συζήτηση.</summary>
        private void ForkConversationFrom(ChatTurn cutoffTurn)
        {
            var idx = _history.IndexOf(cutoffTurn);
            if (idx < 0) return;

            SaveCurrentConversation(); // η τρέχουσα συζήτηση μένει άθικτη με το πλήρες ιστορικό της
            var source = _conversations.FirstOrDefault(c => c.Id == _currentConversationId);

            var forked = new Conversation
            {
                Id = Guid.NewGuid().ToString("N"),
                Title = L.ForkedTitle(source?.Title ?? ""),
                History = _history.Take(idx + 1).Select(t => new ChatTurn { Role = t.Role, Content = t.Content }).ToList(),
                UpdatedAtUtc = DateTime.UtcNow.ToString("O"),
                ProjectId = source?.ProjectId
            };
            _conversations.Insert(0, forked);
            SaveConversations();
            LoadConversation(forked.Id);
        }

        private void RenameConversation(string conversationId, string newTitle)
        {
            var conv = _conversations.FirstOrDefault(c => c.Id == conversationId);
            if (conv == null) return;
            conv.Title = newTitle;
            SaveConversations();
            BuildHistoryList();
        }

        /// <summary>Διαγράφει ΟΛΕΣ τις αποθηκευμένες συζητήσεις (κουμπί στις Ρυθμίσεις → Project, με
        /// επιβεβαίωση) - δεν αγγίζει την τρέχουσα ανοιχτή συζήτηση· αν συνεχιστεί, θα ξανα-αποθηκευτεί
        /// κανονικά ως νέα εγγραφή.</summary>
        internal void ClearAllConversations()
        {
            _conversations.Clear();
            _drafts.Clear();
            SaveConversations();
            BuildHistoryList();
        }

        /// <summary>Εισαγωγή αντιγράφου ασφαλείας (Ρυθμίσεις → Project) - κάθε εισαγόμενη συζήτηση παίρνει
        /// ΝΕΟ id ώστε να μην υπάρχει καμία πιθανότητα σύγκρουσης με υπαρκτό id (π.χ. επανα-εισαγωγή του ίδιου
        /// αρχείου, ή μεταφορά από άλλη εγκατάσταση) - προστίθενται σαν επιπλέον εγγραφές, δεν αντικαθιστούν.</summary>
        internal void ImportConversationsBackup(List<Conversation> imported)
        {
            foreach (var conv in imported)
            {
                conv.Id = Guid.NewGuid().ToString("N");
                _conversations.Insert(0, conv);
            }
            SaveConversations();
            BuildHistoryList();
        }

        // ---------- Category tiles ----------
        /// <summary>Σελιδοποίηση αντί για scrollbar στα πλακίδια εργαλείων - το ScrollViewer κάνει την
        /// πραγματική δουλειά (PageUp/PageDown = ένα viewport τη φορά), τα βελάκια απλά το πυροδοτούν και
        /// δείχνουν πού βρίσκεσαι. Καλείται σε κάθε αλλαγή μεγέθους/περιεχομένου μέσω ScrollChanged.</summary>
        private void TilesScroll_ScrollChanged(object sender, ScrollChangedEventArgs e) => UpdateTilesPager();

        private void UpdateTilesPager()
        {
            var extent = TilesScroll.ExtentHeight;
            var viewport = TilesScroll.ViewportHeight;
            var offset = TilesScroll.VerticalOffset;

            bool hasMultiplePages = extent > viewport + 1;
            TilesPagerPanel.Visibility = hasMultiplePages ? Visibility.Visible : Visibility.Collapsed;
            if (!hasMultiplePages) return;

            var totalPages = Math.Max(1, (int)Math.Ceiling(extent / viewport));
            var currentPage = Math.Min(totalPages, (int)Math.Floor(offset / viewport) + 1);
            TilesPageIndicatorText.Text = $"{currentPage}/{totalPages}";
            TilesPageUpButton.IsEnabled = offset > 0.5;
            TilesPageDownButton.IsEnabled = offset < extent - viewport - 0.5;
        }

        private void TilesPageUpButton_Click(object sender, RoutedEventArgs e) => TilesScroll.PageUp();
        private void TilesPageDownButton_Click(object sender, RoutedEventArgs e) => TilesScroll.PageDown();

        private void BuildCategories()
        {
            CategoriesPanel.Children.Clear();
            _categoryFrames.Clear();
            foreach (var cat in Catalog.Categories)
            {
                var frame = new Border
                {
                    BorderThickness = new Thickness(_skin == WindowsSkin.Win8 ? 0 : 1),
                    CornerRadius = new CornerRadius(SkinFrameRadius),
                    Padding = new Thickness(16),
                    Margin = new Thickness(0, 0, 0, 18),
                    Visibility = Visibility.Collapsed
                };
                frame.SetResourceReference(Border.BackgroundProperty, "PanelBrush");
                frame.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
                if (SkinShadow is { } fs)
                    frame.Effect = new DropShadowEffect { Color = Colors.Black, BlurRadius = fs.Blur, ShadowDepth = fs.Depth, Opacity = fs.Opacity * 0.6 };

                var outer = new StackPanel();

                var headerPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };
                var bar = new Border { Width = 20, Height = 3, Margin = new Thickness(0, 0, 8, 0), VerticalAlignment = VerticalAlignment.Center };
                bar.SetResourceReference(Border.BackgroundProperty, cat.BrushKey);
                var icon = BuildCategoryIcon(cat.Id, 16);
                icon.Margin = new Thickness(0, 0, 8, 0);
                var label = new TextBlock { Text = CatLabel(cat), FontSize = 16, FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center };
                label.SetResourceReference(TextBlock.ForegroundProperty, "InkBrush");
                var matching = Catalog.Tools.Where(t => t.CategoryId == cat.Id)
                    .Where(t => string.IsNullOrEmpty(_toolSearchFilter) || t.Name.Contains(_toolSearchFilter, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(t => _pinnedTools.Contains(t.Name)).ToList();

                var countLabel = new TextBlock
                {
                    Text = L.ToolsCountSuffix(matching.Count),
                    FontSize = 11.5,
                    VerticalAlignment = VerticalAlignment.Center
                };
                countLabel.SetResourceReference(TextBlock.ForegroundProperty, "InkSoftBrush");
                headerPanel.Children.Add(bar);
                headerPanel.Children.Add(icon);
                headerPanel.Children.Add(label);
                headerPanel.Children.Add(countLabel);

                var wrap = new WrapPanel();
                foreach (var tool in matching)
                    wrap.Children.Add(BuildTile(tool, cat));

                outer.Children.Add(headerPanel);
                outer.Children.Add(wrap);
                frame.Child = outer;
                CategoriesPanel.Children.Add(frame);
                _categoryFrames[cat.Id] = frame;

                // Με ενεργό φίλτρο αναζήτησης, εμφανίζουμε όλες τις κατηγορίες με έστω 1 ταίριασμα ταυτόχρονα
                // (αγνοώντας ποια καρτέλα είναι επιλεγμένη) - χωρίς αυτό η αναζήτηση θα έδειχνε αποτελέσματα
                // μόνο από την τρέχουσα ενεργή κατηγορία.
                if (!string.IsNullOrEmpty(_toolSearchFilter))
                    frame.Visibility = matching.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void BuildCategoryTabs()
        {
            CategoryTabsPanel.Children.Clear();
            _categoryTabs.Clear();
            foreach (var cat in Catalog.Categories)
            {
                var tab = new Border
                {
                    BorderThickness = new Thickness(1.5),
                    CornerRadius = new CornerRadius(SkinTabRadius),
                    Padding = new Thickness(12, 6, 12, 6),
                    Margin = new Thickness(0, 0, 8, 6),
                    Cursor = Cursors.Hand,
                    SnapsToDevicePixels = true,
                    UseLayoutRounding = true
                };
                tab.SetResourceReference(Border.BorderBrushProperty, cat.BrushKey);
                tab.SetResourceReference(Border.BackgroundProperty, "PanelBrush");

                var content = new StackPanel { Orientation = Orientation.Horizontal };
                var icon = BuildCategoryIcon(cat.Id, 14);
                icon.Margin = new Thickness(0, 0, 6, 0);
                var label = new TextBlock { Text = CatLabel(cat), FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center };
                label.SetResourceReference(TextBlock.ForegroundProperty, "InkBrush");
                content.Children.Add(icon);
                content.Children.Add(label);
                tab.Child = content;

                tab.MouseLeftButtonUp += (s, e) => SelectCategory(cat.Id);

                CategoryTabsPanel.Children.Add(tab);
                _categoryTabs[cat.Id] = (tab, label, icon);
            }
        }

        private void SelectCategory(string categoryId)
        {
            _activeCategoryId = categoryId;

            foreach (var kv in _categoryFrames)
                kv.Value.Visibility = kv.Key == categoryId ? Visibility.Visible : Visibility.Collapsed;
            TilesScroll.ScrollToTop();
            UpdateTilesPager();

            foreach (var cat in Catalog.Categories)
            {
                if (!_categoryTabs.TryGetValue(cat.Id, out var t)) continue;
                bool active = cat.Id == categoryId;
                if (active)
                {
                    if (SkinUsesGlossyButtons && FindResource(cat.BrushKey) is SolidColorBrush baseBrush)
                        t.Tab.Background = GlossyBrush(baseBrush.Color);
                    else
                        t.Tab.SetResourceReference(Border.BackgroundProperty, cat.BrushKey);
                    t.Tab.BorderThickness = new Thickness(0);
                    t.Label.Foreground = Brushes.White;
                    SetIconColor(t.Icon, Brushes.White);
                }
                else
                {
                    t.Tab.SetResourceReference(Border.BackgroundProperty, "PanelBrush");
                    t.Tab.BorderThickness = new Thickness(1.5);
                    t.Label.SetResourceReference(TextBlock.ForegroundProperty, "InkBrush");
                    SetIconColor(t.Icon, null);
                }
            }
        }

        /// <summary>Αστεράκι για "καρφίτσωμα" ενός εργαλείου - τα καρφιτσωμένα εμφανίζονται πρώτα μέσα στην
        /// κατηγορία τους (βλ. ταξινόμηση στο BuildCategories). e.Handled=true ώστε το κλικ να ΜΗΝ ανοίγει και
        /// τις λεπτομέρειες του εργαλείου (το tile από πάνω έχει δικό του MouseLeftButtonUp).</summary>
        private TextBlock BuildPinStar(ToolItem tool, bool whiteText)
        {
            bool pinned = _pinnedTools.Contains(tool.Name);
            var star = new TextBlock
            {
                Text = pinned ? "★" : "☆", FontSize = 14, Margin = new Thickness(6, 0, 0, 0),
                Cursor = Cursors.Hand, VerticalAlignment = VerticalAlignment.Center, ToolTip = L.PinToolTooltip
            };
            if (whiteText) star.Foreground = Brushes.White;
            else if (pinned) star.SetResourceReference(TextBlock.ForegroundProperty, "ClaudeBrush");
            else star.SetResourceReference(TextBlock.ForegroundProperty, "InkSoftBrush");

            star.MouseLeftButtonUp += (s, e) =>
            {
                e.Handled = true;
                TogglePinnedTool(tool.Name);
            };
            return star;
        }

        private void TogglePinnedTool(string toolName)
        {
            if (!_pinnedTools.Remove(toolName)) _pinnedTools.Add(toolName);
            SaveSettings();
            RefreshCatalogTiles();
            if (_currentTool != null && _currentTool.Name == toolName && DetailsPanel.Visibility == Visibility.Visible)
                ShowDetails(_currentTool);
        }

        private string _toolSearchFilter = "";
        private DispatcherTimer? _searchDebounceTimer;

        // ---------- Ζωντανή συνεργασία (v4.5.0) - βλ. SetupSyncFolderWatcher πιο κάτω ----------
        private System.IO.FileSystemWatcher? _syncFolderWatcher;
        private DispatcherTimer? _syncReloadDebounceTimer;

        /// <summary>"Ζωντανή συνεργασία" (v4.5.0) - δύο υπολογιστές που δείχνουν στον ΙΔΙΟ φάκελο
        /// συγχρονισμού βλέπουν τις αλλαγές του άλλου χωρίς επανεκκίνηση, μέσω <see cref="FileSystemWatcher"/>
        /// πάνω στο ίδιο conversations.json/projects-list.json - όχι νέο backend, φυσική συνέχεια του ήδη
        /// υπάρχοντος μηχανισμού συγχρονισμού μέσω φακέλου (v3.8.0). ΔΕΝ αγγίζει την ενεργή _history - μόνο
        /// ξαναφορτώνει τις λίστες στη μνήμη, ώστε ό,τι γράφει αυτή τη στιγμή ο χρήστης να μη χαθεί ποτέ.</summary>
        private void SetupSyncFolderWatcher()
        {
            _syncFolderWatcher?.Dispose();
            _syncFolderWatcher = null;
            if (string.IsNullOrWhiteSpace(_conversationsSyncFolder) || !System.IO.Directory.Exists(_conversationsSyncFolder)) return;

            try
            {
                _syncFolderWatcher = new System.IO.FileSystemWatcher(_conversationsSyncFolder)
                {
                    Filter = "*.json",
                    NotifyFilter = System.IO.NotifyFilters.LastWrite | System.IO.NotifyFilters.Size,
                    EnableRaisingEvents = true
                };
                _syncFolderWatcher.Changed += (s, e) => Dispatcher.BeginInvoke(ScheduleSyncReload);
                _syncFolderWatcher.Created += (s, e) => Dispatcher.BeginInvoke(ScheduleSyncReload);
            }
            catch { /* π.χ. δικτυακός φάκελος προσωρινά μη διαθέσιμος - ξαναδοκιμάζεται στην επόμενη αλλαγή ρύθμισης */ }
        }

        private void ScheduleSyncReload()
        {
            // Debounce ίδιο πνεύμα με το ToolSearchBox_TextChanged - ένα save γράφει το αρχείο συνήθως σε
            // δύο βήματα (temp file + rename), που θα πυροδοτούσε δύο ξεχωριστά reload χωρίς αυτό.
            _syncReloadDebounceTimer ??= new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(600) };
            _syncReloadDebounceTimer.Stop();
            _syncReloadDebounceTimer.Tick -= SyncReloadDebounceTimer_Tick;
            _syncReloadDebounceTimer.Tick += SyncReloadDebounceTimer_Tick;
            _syncReloadDebounceTimer.Start();
        }

        private void SyncReloadDebounceTimer_Tick(object? sender, EventArgs e)
        {
            _syncReloadDebounceTimer!.Stop();
            _conversations = ConversationStore.Load(_conversationsSyncFolder);
            _projects = ProjectsListStore.Load(_conversationsSyncFolder);
            if (HistoryPopup.IsOpen) BuildHistoryList();
        }

        /// <summary>ΔΙΟΡΘΩΣΗ κατανάλωσης πόρων: πριν, κάθε πάτημα πλήκτρου στην αναζήτηση ξανάχτιζε ΟΛΟΝ τον
        /// κατάλογο (κάθε πλακίδιο, κάθε λογότυπο) αμέσως - αν κάποιος έγραφε μια λέξη 8 χαρακτήρων, αυτό
        /// ήταν 8 πλήρη rebuilds στη σειρά. Ένα μονό (όχι επαναλαμβανόμενο) DispatcherTimer 220ms που
        /// επανεκκινείται σε κάθε πλήκτρο μαζεύει τα ενδιάμεσα πατήματα σε ΕΝΑ rebuild μετά το τελευταίο.</summary>
        private void ToolSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchDebounceTimer ??= new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(220) };
            _searchDebounceTimer.Stop();
            _searchDebounceTimer.Tick -= SearchDebounceTimer_Tick;
            _searchDebounceTimer.Tick += SearchDebounceTimer_Tick;
            _searchDebounceTimer.Start();
        }

        private void SearchDebounceTimer_Tick(object? sender, EventArgs e)
        {
            _searchDebounceTimer!.Stop();
            _toolSearchFilter = ToolSearchBox.Text.Trim();
            RefreshCatalogTiles();
        }

        private void RefreshCatalogTiles()
        {
            var previousCategory = _activeCategoryId ?? Catalog.Categories[0].Id;
            BuildCategories();
            if (string.IsNullOrEmpty(_toolSearchFilter)) SelectCategory(previousCategory);
        }

        private Border BuildTile(ToolItem tool, CategoryDef cat)
        {
            if (SkinUsesMetroTiles) return BuildMetroTile(tool, cat);

            var tile = new Border
            {
                Width = 212,
                MinHeight = 112,
                Margin = new Thickness(0, 0, 10, 10),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(SkinTileRadius),
                Cursor = Cursors.Hand
            };
            if (SkinShadow is { } ts)
                tile.Effect = new DropShadowEffect { Color = Colors.Black, BlurRadius = ts.Blur * 0.6, ShadowDepth = ts.Depth * 0.5, Opacity = ts.Opacity * 0.7 };
            tile.SetResourceReference(Border.BackgroundProperty, "PaperBrush");
            if (SkinUsesBeveled)
            {
                // Classic = ανάγλυφο (raised) 3D πλαίσιο στυλ Windows 95/98/2000: ανοιχτόχρωμο πάνω-αριστερά,
                // σκούρο κάτω-δεξιά, διαγώνια - το πιο αναγνωρίσιμο οπτικό στοιχείο της εποχής.
                tile.BorderBrush = ClassicBevelBrush;
                tile.BorderThickness = new Thickness(2);
            }
            else
            {
                tile.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
            }

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var accent = new Border();
            if (SkinUsesGlossyButtons && FindResource(cat.BrushKey) is SolidColorBrush accentBase)
                accent.Background = GlossyBrush(accentBase.Color);
            else
                accent.SetResourceReference(Border.BackgroundProperty, cat.BrushKey);
            Grid.SetColumn(accent, 0);

            var content = new Border { Padding = new Thickness(11) };
            Grid.SetColumn(content, 1);

            var stack = new StackPanel();
            var nameRow = new StackPanel { Orientation = Orientation.Horizontal };
            nameRow.Children.Add(BuildLogo(tool, cat));
            var nameText = new TextBlock { Text = tool.Name, FontWeight = FontWeights.SemiBold, FontSize = 13.5, TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center };
            nameText.SetResourceReference(TextBlock.ForegroundProperty, "InkBrush");
            nameRow.Children.Add(nameText);
            nameRow.Children.Add(BuildPinStar(tool, whiteText: false));
            stack.Children.Add(nameRow);

            if (tool.HasMcp)
            {
                var mcpTag = new TextBlock { Text = "MCP", FontSize = 9.5, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 2, 0, 0) };
                mcpTag.SetResourceReference(TextBlock.ForegroundProperty, "ClaudeBrush");
                stack.Children.Add(mcpTag);
            }

            var freeText = new TextBlock { Text = L.PickToolText(tool.Free, tool.FreeEn, tool.FreeFr, tool.FreeDe), FontSize = 10.5, Margin = new Thickness(0, 4, 0, 4), TextWrapping = TextWrapping.Wrap };
            freeText.SetResourceReference(TextBlock.ForegroundProperty, "InkSoftBrush");
            stack.Children.Add(freeText);

            var descText = new TextBlock { Text = L.PickToolText(tool.Description, tool.DescriptionEn, tool.DescriptionFr, tool.DescriptionDe), FontSize = 11, TextWrapping = TextWrapping.Wrap };
            descText.SetResourceReference(TextBlock.ForegroundProperty, "InkSoftBrush");
            stack.Children.Add(descText);

            content.Child = stack;
            grid.Children.Add(accent);
            grid.Children.Add(content);
            tile.Child = grid;

            tile.MouseLeftButtonUp += (s, e) => ShowDetails(tool);
            return tile;
        }

        /// <summary>Windows 8 "Metro" πλακίδιο: γεμάτο χρώμα κατηγορίας, αιχμηρές γωνίες, λευκό κείμενο, χωρίς σκιά/gradient.</summary>
        private Border BuildMetroTile(ToolItem tool, CategoryDef cat)
        {
            var tile = new Border { Width = 212, MinHeight = 112, Margin = new Thickness(0, 0, 10, 10), Cursor = Cursors.Hand };
            tile.SetResourceReference(Border.BackgroundProperty, cat.BrushKey);

            var stack = new StackPanel { Margin = new Thickness(12) };
            var nameRow = new StackPanel { Orientation = Orientation.Horizontal };
            nameRow.Children.Add(BuildLogo(tool, cat));
            var nameText = new TextBlock { Text = tool.Name, FontWeight = FontWeights.Bold, FontSize = 14, Foreground = Brushes.White, TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center };
            nameRow.Children.Add(nameText);
            nameRow.Children.Add(BuildPinStar(tool, whiteText: true));
            stack.Children.Add(nameRow);

            if (tool.HasMcp)
                stack.Children.Add(new TextBlock { Text = "MCP", FontSize = 9.5, FontWeight = FontWeights.Bold, Foreground = Brushes.White, Opacity = 0.85, Margin = new Thickness(0, 3, 0, 0) });

            stack.Children.Add(new TextBlock
            {
                Text = L.PickToolText(tool.Free, tool.FreeEn, tool.FreeFr, tool.FreeDe),
                FontSize = 10.5, Foreground = Brushes.White, Opacity = 0.85, Margin = new Thickness(0, 5, 0, 4), TextWrapping = TextWrapping.Wrap
            });
            stack.Children.Add(new TextBlock
            {
                Text = L.PickToolText(tool.Description, tool.DescriptionEn, tool.DescriptionFr, tool.DescriptionDe),
                FontSize = 11, Foreground = Brushes.White, Opacity = 0.92, TextWrapping = TextWrapping.Wrap
            });

            tile.Child = stack;
            tile.MouseLeftButtonUp += (s, e) => ShowDetails(tool);
            return tile;
        }

        private static Canvas BuildFlag(Lang lang)
        {
            var canvas = new Canvas { Width = 24, Height = 16, ClipToBounds = true };

            void AddRect(double x, double y, double w, double h, Brush fill)
            {
                var r = new Rectangle { Width = w, Height = h, Fill = fill };
                Canvas.SetLeft(r, x);
                Canvas.SetTop(r, y);
                canvas.Children.Add(r);
            }

            switch (lang)
            {
                case Lang.FR:
                    var frBlue = new SolidColorBrush(Color.FromRgb(0, 85, 164));
                    var frRed = new SolidColorBrush(Color.FromRgb(239, 65, 53));
                    AddRect(0, 0, 8, 16, frBlue);
                    AddRect(8, 0, 8, 16, Brushes.White);
                    AddRect(16, 0, 8, 16, frRed);
                    break;

                case Lang.DE:
                    var deRed = new SolidColorBrush(Color.FromRgb(221, 0, 0));
                    var deGold = new SolidColorBrush(Color.FromRgb(255, 206, 0));
                    AddRect(0, 0, 24, 5.33, Brushes.Black);
                    AddRect(0, 5.33, 24, 5.33, deRed);
                    AddRect(0, 10.67, 24, 5.33, deGold);
                    break;

                case Lang.EL:
                    var elBlue = new SolidColorBrush(Color.FromRgb(13, 94, 175));
                    double stripe = 16 / 9.0;
                    for (int i = 0; i < 9; i++)
                        AddRect(0, i * stripe, 24, stripe, i % 2 == 0 ? elBlue : Brushes.White);
                    AddRect(0, 0, 9.6, 8.9, elBlue);
                    AddRect(3.8, 0, 1.9, 8.9, Brushes.White);
                    AddRect(0, 3.5, 9.6, 1.9, Brushes.White);
                    break;

                default: // EN -> Union Jack (simplified)
                    var ukRed = new SolidColorBrush(Color.FromRgb(200, 16, 46));
                    AddRect(0, 0, 24, 16, new SolidColorBrush(Color.FromRgb(1, 33, 105)));

                    void AddDiag(double x1, double y1, double x2, double y2, Brush stroke, double thickness)
                    {
                        var line = new Line { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2, Stroke = stroke, StrokeThickness = thickness };
                        canvas.Children.Add(line);
                    }

                    AddDiag(0, 0, 24, 16, Brushes.White, 4.5);
                    AddDiag(24, 0, 0, 16, Brushes.White, 4.5);
                    AddDiag(0, 0, 24, 16, ukRed, 1.8);
                    AddDiag(24, 0, 0, 16, ukRed, 1.8);

                    AddRect(0, 6, 24, 4, Brushes.White);
                    AddRect(10, 0, 4, 16, Brushes.White);
                    AddRect(0, 7.2, 24, 1.6, ukRed);
                    AddRect(11.2, 0, 1.6, 16, ukRed);
                    break;
            }

            return canvas;
        }

        /// <summary>Απλά, θεματικά vector εικονίδια ανά κατηγορία (αντί για emoji που δεν αποδίδονται συνεπώς σε όλα τα Windows).</summary>
        private Canvas BuildCategoryIcon(string categoryId, double size)
        {
            var c = new Canvas { Width = size, Height = size, SnapsToDevicePixels = true };
            double t = Math.Max(1.1, size * 0.09);

            Shape Stroke(Shape s) { s.StrokeThickness = t; s.SetResourceReference(Shape.StrokeProperty, "InkBrush"); return s; }
            Shape Fill(Shape s) { s.SetResourceReference(Shape.FillProperty, "InkBrush"); return s; }

            switch (categoryId)
            {
                case "text":
                    var bubble = new Border
                    {
                        Width = size * 0.85, Height = size * 0.6, CornerRadius = new CornerRadius(size * 0.16),
                        BorderThickness = new Thickness(t)
                    };
                    bubble.SetResourceReference(Border.BorderBrushProperty, "InkBrush");
                    Canvas.SetLeft(bubble, 0); Canvas.SetTop(bubble, 0);
                    c.Children.Add(bubble);
                    var tail = Fill(new Polygon { Points = new PointCollection { new(0, 0), new(size * 0.22, 0), new(0, size * 0.22) } });
                    Canvas.SetLeft(tail, size * 0.12); Canvas.SetTop(tail, size * 0.56);
                    c.Children.Add(tail);
                    break;

                case "image":
                    var frame = new Border { Width = size * 0.9, Height = size * 0.75, BorderThickness = new Thickness(t) };
                    frame.SetResourceReference(Border.BorderBrushProperty, "InkBrush");
                    Canvas.SetLeft(frame, 0); Canvas.SetTop(frame, size * 0.12);
                    c.Children.Add(frame);
                    var sun = Fill(new Ellipse { Width = size * 0.16, Height = size * 0.16 });
                    Canvas.SetLeft(sun, size * 0.58); Canvas.SetTop(sun, size * 0.24);
                    c.Children.Add(sun);
                    var mountain = Fill(new Polygon { Points = new PointCollection { new(0, size * 0.87), new(size * 0.35, size * 0.45), new(size * 0.55, size * 0.65), new(size * 0.7, size * 0.5), new(size * 0.9, size * 0.87) } });
                    c.Children.Add(mountain);
                    break;

                case "video":
                    var screen = new Border { Width = size * 0.9, Height = size * 0.68, CornerRadius = new CornerRadius(size * 0.08), BorderThickness = new Thickness(t) };
                    screen.SetResourceReference(Border.BorderBrushProperty, "InkBrush");
                    Canvas.SetLeft(screen, 0); Canvas.SetTop(screen, size * 0.16);
                    c.Children.Add(screen);
                    var play = Fill(new Polygon { Points = new PointCollection { new(size * 0.36, size * 0.34), new(size * 0.36, size * 0.66), new(size * 0.64, size * 0.5) } });
                    c.Children.Add(play);
                    break;

                case "audio":
                    var head1 = Fill(new Ellipse { Width = size * 0.26, Height = size * 0.2 });
                    Canvas.SetLeft(head1, 0); Canvas.SetTop(head1, size * 0.72);
                    c.Children.Add(head1);
                    var head2 = Fill(new Ellipse { Width = size * 0.26, Height = size * 0.2 });
                    Canvas.SetLeft(head2, size * 0.5); Canvas.SetTop(head2, size * 0.6);
                    c.Children.Add(head2);
                    var stem1 = Stroke(new Line { X1 = size * 0.24, Y1 = size * 0.8, X2 = size * 0.24, Y2 = size * 0.06 });
                    c.Children.Add(stem1);
                    var stem2 = Stroke(new Line { X1 = size * 0.74, Y1 = size * 0.68, X2 = size * 0.74, Y2 = 0 });
                    c.Children.Add(stem2);
                    var beam = Stroke(new Line { X1 = size * 0.24, Y1 = size * 0.06, X2 = size * 0.74, Y2 = 0 });
                    c.Children.Add(beam);
                    break;

                case "code":
                    var codeText = new TextBlock
                    {
                        Text = "</>", FontFamily = new FontFamily("Consolas"), FontWeight = FontWeights.Bold,
                        FontSize = size * 0.62, VerticalAlignment = VerticalAlignment.Center
                    };
                    codeText.SetResourceReference(TextBlock.ForegroundProperty, "InkBrush");
                    Canvas.SetTop(codeText, size * 0.14);
                    c.Children.Add(codeText);
                    break;

                case "agents":
                    var head = new Border { Width = size * 0.7, Height = size * 0.55, CornerRadius = new CornerRadius(size * 0.12), BorderThickness = new Thickness(t) };
                    head.SetResourceReference(Border.BorderBrushProperty, "InkBrush");
                    Canvas.SetLeft(head, size * 0.15); Canvas.SetTop(head, size * 0.35);
                    c.Children.Add(head);
                    var eyeL = Fill(new Ellipse { Width = size * 0.1, Height = size * 0.1 });
                    Canvas.SetLeft(eyeL, size * 0.28); Canvas.SetTop(eyeL, size * 0.55);
                    c.Children.Add(eyeL);
                    var eyeR = Fill(new Ellipse { Width = size * 0.1, Height = size * 0.1 });
                    Canvas.SetLeft(eyeR, size * 0.62); Canvas.SetTop(eyeR, size * 0.55);
                    c.Children.Add(eyeR);
                    var antenna = Stroke(new Line { X1 = size * 0.5, Y1 = size * 0.35, X2 = size * 0.5, Y2 = size * 0.12 });
                    c.Children.Add(antenna);
                    var antTip = Fill(new Ellipse { Width = size * 0.12, Height = size * 0.12 });
                    Canvas.SetLeft(antTip, size * 0.44); Canvas.SetTop(antTip, 0);
                    c.Children.Add(antTip);
                    break;

                case "research":
                    var lens = Stroke(new Ellipse { Width = size * 0.55, Height = size * 0.55 });
                    Canvas.SetLeft(lens, 0); Canvas.SetTop(lens, 0);
                    c.Children.Add(lens);
                    var handle = Stroke(new Line { X1 = size * 0.55, Y1 = size * 0.55, X2 = size * 0.95, Y2 = size * 0.95 });
                    ((Line)handle).StrokeStartLineCap = PenLineCap.Round;
                    ((Line)handle).StrokeEndLineCap = PenLineCap.Round;
                    c.Children.Add(handle);
                    break;

                case "design":
                    var palette = Stroke(new Ellipse { Width = size * 0.9, Height = size * 0.75 });
                    Canvas.SetTop(palette, size * 0.1);
                    c.Children.Add(palette);
                    (Brush, double, double)[] dots =
                    {
                        (Brushes.IndianRed, 0.28, 0.3), (Brushes.SteelBlue, 0.52, 0.24), (Brushes.Goldenrod, 0.68, 0.44)
                    };
                    foreach (var (brush, dx, dy) in dots)
                    {
                        var dot = new Ellipse { Width = size * 0.16, Height = size * 0.16, Fill = brush, Tag = "fixed" };
                        Canvas.SetLeft(dot, size * dx); Canvas.SetTop(dot, size * dy);
                        c.Children.Add(dot);
                    }
                    break;
            }

            return c;
        }

        private static void SetIconColor(Canvas icon, Brush? explicitColor)
        {
            foreach (var child in icon.Children)
            {
                if (child is Shape shape)
                {
                    if (Equals((shape as FrameworkElement)?.Tag, "fixed")) continue;
                    if (explicitColor != null)
                    {
                        if (shape.Stroke != null) shape.Stroke = explicitColor;
                        if (shape.Fill != null) shape.Fill = explicitColor;
                    }
                    else
                    {
                        if (shape.Stroke != null) shape.SetResourceReference(Shape.StrokeProperty, "InkBrush");
                        if (shape.Fill != null) shape.SetResourceReference(Shape.FillProperty, "InkBrush");
                    }
                }
                else if (child is Border border)
                {
                    if (explicitColor != null) border.BorderBrush = explicitColor;
                    else border.SetResourceReference(Border.BorderBrushProperty, "InkBrush");
                }
                else if (child is TextBlock text)
                {
                    if (explicitColor != null) text.Foreground = explicitColor;
                    else text.SetResourceReference(TextBlock.ForegroundProperty, "InkBrush");
                }
            }
        }

        /// <summary>Glossy (Aero-style) gradient: πιο ανοιχτό στην κορυφή, βασικό χρώμα στο κάτω μισό — like a Vista/7 κουμπί.</summary>
        /// <summary>Διαγώνιο ασημί→γκρι gradient που μιμείται το ανάγλυφο 3D πλαίσιο (raised bevel) των
        /// κουμπιών/πλαισίων στα Windows 95/98/2000 - το πιο αναγνωρίσιμο στοιχείο του Classic look.</summary>
        private static readonly LinearGradientBrush ClassicBevelBrush = new(
            Color.FromRgb(0xFF, 0xFF, 0xFF), Color.FromRgb(0x40, 0x40, 0x40), new Point(0, 0), new Point(1, 1));

        private static LinearGradientBrush GlossyBrush(Color baseColor)
        {
            Color Lighten(Color c, double amt)
            {
                byte L(byte ch) => (byte)Math.Min(255, ch + (255 - ch) * amt);
                return Color.FromRgb(L(c.R), L(c.G), L(c.B));
            }

            var brush = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(0, 1) };
            brush.GradientStops.Add(new GradientStop(Lighten(baseColor, 0.55), 0));
            brush.GradientStops.Add(new GradientStop(baseColor, 0.55));
            brush.GradientStops.Add(new GradientStop(baseColor, 1));
            return brush;
        }

        private static string CatLabel(CategoryDef cat) => L.Current switch
        {
            Lang.EL => cat.Label,
            Lang.EN => cat.LabelEn,
            Lang.FR => cat.LabelFr,
            _ => cat.LabelDe
        };

        private static string GetDomain(string url)
        {
            try
            {
                var host = new Uri(url).Host;
                return host.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ? host[4..] : host;
            }
            catch { return ""; }
        }

        /// <summary>Cache των ήδη αποκωδικοποιημένων εικόνων λογότυπων ανά URL - ΔΙΟΡΘΩΣΗ κατανάλωσης RAM:
        /// το BuildLogo καλείται σε ΚΑΘΕ rebuild του καταλόγου (αλλαγή θέματος/skin/γλώσσας, κάθε πάτημα
        /// πλήκτρου στην αναζήτηση, κάθε καρφίτσωμα) και έφτιαχνε ΚΑΙΝΟΥΡΙΟ BitmapImage (νέο download +
        /// αποκωδικοποίηση 128×128 εικόνας) κάθε φορά για τα ίδια ~39 λογότυπα - το WPF δεν κάνει από μόνο
        /// του dedupe BitmapImage instances με το ίδιο URI. Η επαναχρησιμοποίηση του ήδη φορτωμένου
        /// BitmapImage αντί για νέο αντικείμενο εξαλείφει το επαναλαμβανόμενο δίκτυο+αποκωδικοποίηση.</summary>
        private static readonly Dictionary<string, BitmapImage> LogoCache = new();

        private Grid BuildLogo(ToolItem tool, CategoryDef cat, double size = 22)
        {
            var box = new Grid { Width = size, Height = size, Margin = new Thickness(0, 0, 7, 0), VerticalAlignment = VerticalAlignment.Center };

            // Λευκή "πλάκα" σταθερά πίσω από κάθε logo (και στα δύο θέματα) για αντίθεση με σκούρα/μονόχρωμα εικονίδια.
            var plate = new Border { CornerRadius = new CornerRadius(size * 0.22), Background = Brushes.White };

            var fallback = new Border { CornerRadius = new CornerRadius(size / 2) };
            fallback.SetResourceReference(Border.BackgroundProperty, cat.BrushKey);
            fallback.Child = new TextBlock
            {
                Text = tool.Name.Length > 0 ? tool.Name[..1].ToUpperInvariant() : "?",
                Foreground = Brushes.White,
                FontSize = size * 0.48,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var img = new Image
            {
                Stretch = Stretch.Uniform,
                Visibility = Visibility.Collapsed,
                Margin = new Thickness(size * 0.12)
            };
            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);

            var domain = GetDomain(tool.Url);
            var logoUrl = tool.LogoUrl ?? (!string.IsNullOrEmpty(domain) ? $"https://www.google.com/s2/favicons?domain={domain}&sz=128" : null);
            if (logoUrl != null)
            {
                if (LogoCache.TryGetValue(logoUrl, out var cached))
                {
                    // Ήδη φορτωμένο από προηγούμενο rebuild - καμία νέα λήψη/αποκωδικοποίηση, απλή επανάχρηση.
                    img.Source = cached;
                    img.Visibility = Visibility.Visible;
                    fallback.Visibility = Visibility.Collapsed;
                }
                else
                {
                    try
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.UriSource = new Uri(logoUrl);
                        // ΔΙΟΡΘΩΣΗ: τα handlers ΠΡΕΠΕΙ να καταχωρηθούν πριν το EndInit() — αλλιώς όταν η εικόνα
                        // είναι ήδη cached από το WPF (π.χ. μετά από rebuild λόγω αλλαγής skin), το EndInit()
                        // ολοκληρώνεται συγχρονισμένα και το DownloadCompleted/DownloadFailed ποτέ δεν πυροδοτείται,
                        // αφήνοντας μόνιμα το fallback γράμμα αντί του πραγματικού logo.
                        bmp.DownloadCompleted += (s, e) => { LogoCache[logoUrl] = bmp; img.Visibility = Visibility.Visible; fallback.Visibility = Visibility.Collapsed; };
                        bmp.DownloadFailed += (s, e) => { img.Visibility = Visibility.Collapsed; fallback.Visibility = Visibility.Visible; plate.Visibility = Visibility.Collapsed; };
                        bmp.EndInit();
                        img.ImageFailed += (s, e) => { img.Visibility = Visibility.Collapsed; fallback.Visibility = Visibility.Visible; plate.Visibility = Visibility.Collapsed; };
                        img.Source = bmp;
                        if (!bmp.IsDownloading)
                        {
                            // Ήδη φορτωμένη συγχρονισμένα (cache) - το DownloadCompleted ίσως να μην προλάβει να πυροδοτηθεί.
                            LogoCache[logoUrl] = bmp;
                            img.Visibility = Visibility.Visible;
                            fallback.Visibility = Visibility.Collapsed;
                        }
                    }
                    catch { /* keep fallback letter */ }
                }
            }

            box.Children.Add(plate);
            box.Children.Add(fallback);
            box.Children.Add(img);
            return box;
        }

        // ---------- Details panel ----------
        internal void ShowDetails(ToolItem tool)
        {
            _currentTool = tool;
            DetailsTitle.Text = tool.Name;
            DetailsLogoBox.Children.Clear();
            var detailsCat = Catalog.Categories.FirstOrDefault(c => c.Id == tool.CategoryId) ?? Catalog.Categories[0];
            var detailsLogo = BuildLogo(tool, detailsCat, 28);
            detailsLogo.Margin = new Thickness(0);
            DetailsLogoBox.Children.Add(detailsLogo);
            DetailsPinStarBox.Children.Clear();
            DetailsPinStarBox.Children.Add(BuildPinStar(tool, whiteText: false));
            DetailsFree.Text = L.PickToolText(tool.Free, tool.FreeEn, tool.FreeFr, tool.FreeDe);
            DetailsDesc.Text = L.PickToolText(tool.Description, tool.DescriptionEn, tool.DescriptionFr, tool.DescriptionDe);
            DetailsMcpTag.Visibility = tool.HasMcp ? Visibility.Visible : Visibility.Collapsed;
            DetailsMcpBtn.Visibility = tool.HasMcp ? Visibility.Visible : Visibility.Collapsed;
            DetailsPricingBtn.Visibility = tool.PricingUrl != null ? Visibility.Visible : Visibility.Collapsed;
            DetailsPanel.Visibility = Visibility.Visible;
        }

        private void CloseDetails_Click(object sender, RoutedEventArgs e) => DetailsPanel.Visibility = Visibility.Collapsed;
        private void DetailsOpenBtn_Click(object sender, RoutedEventArgs e) => OpenUrl(_currentTool?.Url);
        private void DetailsMcpBtn_Click(object sender, RoutedEventArgs e) => OpenUrl(_currentTool?.McpUrl);
        private void DetailsPricingBtn_Click(object sender, RoutedEventArgs e) => OpenUrl(_currentTool?.PricingUrl);

        internal void OpenUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, L.LinkOpenError); }
        }

        // ---------- Agent settings / login: πραγματικό δευτερεύον Window (όχι Popup) - δεν κλείνει
        // μόνο του όταν το κύριο παράθυρο χάνει το focus. ----------
        private SettingsWindow? _settingsWindow;

        internal void OpenSettingsWindow(string initialTab = "project")
        {
            if (_settingsWindow != null)
            {
                _settingsWindow.SelectTab(initialTab);
                _settingsWindow.Activate();
                return;
            }
            _settingsWindow = new SettingsWindow(this, initialTab);
            _settingsWindow.Closed += (s, e) => _settingsWindow = null;
            _settingsWindow.Show();
        }

        private CommandPaletteWindow? _commandPalette;

        /// <summary>Ctrl+K: γρήγορη, φιλτραρισμένη αναζήτηση σε εργαλεία/εντολές/ρυθμίσεις - ίδιο πνεύμα με
        /// το command palette του VS Code / Claude Desktop.</summary>
        private void OpenCommandPalette()
        {
            if (_commandPalette != null) { _commandPalette.Activate(); return; }
            _commandPalette = new CommandPaletteWindow(this);
            _commandPalette.Closed += (s, e) => _commandPalette = null;
            _commandPalette.Show();
        }

        private AccountsWindow? _accountsWindow;

        internal void OpenAccountsWindow()
        {
            if (_accountsWindow != null) { _accountsWindow.Activate(); return; }
            _accountsWindow = new AccountsWindow(this);
            _accountsWindow.Closed += (s, e) => _accountsWindow = null;
            _accountsWindow.Show();
        }

        private AgentSettingsWindow? _agentSettingsWindow;

        internal void OpenAgentSettingsWindow()
        {
            if (_agentSettingsWindow != null)
            {
                _agentSettingsWindow.Activate();
                return;
            }
            _agentSettingsWindow = new AgentSettingsWindow(this);
            _agentSettingsWindow.Closed += (s, e) => _agentSettingsWindow = null;
            _agentSettingsWindow.Show();
        }

        // Το "C" (πρωτεύων agent) ανοίγει ΑΠΟΚΛΕΙΣΤΙΚΑ το δικό του, ξεχωριστό παράθυρο (κλειδί,
        // προσαρμοσμένες οδηγίες, συνεργασία/MCP, Hive Memory) - το γρανάζι ανοίγει τις γενικές ρυθμίσεις
        // της εφαρμογής (Project/Συμπεριφορά/Επεκτάσεις/Πληροφορίες). Δύο εντελώς ξεχωριστά παράθυρα,
        // καμία κοινή καρτέλα μεταξύ τους - παραμένουν τα μόνα δύο σημεία εισόδου στις ρυθμίσεις.
        private void AvatarBorder_Click(object sender, MouseButtonEventArgs e) => OpenAgentSettingsWindow();
        private void SettingsGearButton_Click(object sender, RoutedEventArgs e) => OpenSettingsWindow("behavior");

        // ---------- Sidebar shortcut: εμφάνιση/θέματα/skins - ξεχωριστό, αποκλειστικό παράθυρο ----------
        private AppearanceWindow? _appearanceWindow;

        internal void AppearanceButton_Click(object sender, RoutedEventArgs e)
        {
            if (_appearanceWindow != null)
            {
                _appearanceWindow.Activate();
                return;
            }
            _appearanceWindow = new AppearanceWindow(this);
            _appearanceWindow.Closed += (s, e2) => _appearanceWindow = null;
            _appearanceWindow.Show();
        }

        internal void ApplyCentralAgent()
        {
            if (_centralAgentName == "Claude")
            {
                var circle = new Border { CornerRadius = new CornerRadius(15) };
                circle.SetResourceReference(Border.BackgroundProperty, "ClaudeBrush");
                circle.Child = new TextBlock
                {
                    Text = "C", Foreground = Brushes.White, FontWeight = FontWeights.Bold, FontSize = 14,
                    FontFamily = new FontFamily("Bahnschrift"),
                    HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
                };
                AvatarBorder.Child = circle;
                return;
            }

            var tool = Catalog.Tools.FirstOrDefault(t => t.Name == _centralAgentName);
            if (tool == null) return;
            var cat = Catalog.Categories.FirstOrDefault(c => c.Id == tool.CategoryId) ?? Catalog.Categories[0];
            var logo = BuildLogo(tool, cat, 30);
            logo.Margin = new Thickness(0);
            AvatarBorder.Child = logo;
        }

        // ---------- Appearance: transparency level + animated background ----------
        internal void ApplyAppearance()
        {
            ApplySkinAccent();
            ApplySkinPalette();
            ApplySkinChrome();
            HeaderGlow.Visibility = _bgStyle == BgAnimStyle.Glow ? Visibility.Visible : Visibility.Collapsed;
            if (_bgStyle != BgAnimStyle.None && _bgStyle != BgAnimStyle.Glow)
            {
                BuildAnimatedBackground(_bgStyle);
                HeaderParticles.Visibility = Visibility.Visible;
            }
            else
            {
                HeaderParticles.Children.Clear();
                HeaderParticles.Visibility = Visibility.Collapsed;
            }

            AppShellBorder.Opacity = WindowOpacity;
            AppShellBorder.CornerRadius = new CornerRadius(WindowState == WindowState.Maximized ? 0 : SkinShellRadius);
            AppShellBorder.BorderThickness = new Thickness(SkinUsesBeveled ? 2 : 1);
            GlassTintOverlay.Visibility = SkinUsesGlassTint ? Visibility.Visible : Visibility.Collapsed;

            AppShellBorder.Effect = SkinShellShadow is { } ss
                ? new DropShadowEffect { Color = Colors.Black, BlurRadius = ss.Blur, ShadowDepth = 0, Opacity = ss.Opacity }
                : null;

            if (IsElevated || _skin != WindowsSkin.Modern)
            {
                ChatPanelBorder.CornerRadius = new CornerRadius(_skin == WindowsSkin.Modern ? 14 : SkinFrameRadius);
                if (SkinShadow is { } cs)
                    ChatPanelBorder.Effect = new DropShadowEffect { Color = Colors.Black, BlurRadius = cs.Blur, ShadowDepth = cs.Depth, Opacity = cs.Opacity, Direction = 270 };
                else
                    ChatPanelBorder.Effect = null;
            }
            else
            {
                ChatPanelBorder.CornerRadius = new CornerRadius(5);
                ChatPanelBorder.Effect = null;
            }

            ApplyCentralAgent();

            var previousCategory = _activeCategoryId ?? Catalog.Categories[0].Id;
            BuildCategories();
            BuildCategoryTabs();
            if (string.IsNullOrEmpty(_toolSearchFilter)) SelectCategory(previousCategory);
        }

        // ---------- 10 κινούμενα φόντα κεφαλίδας: Particles/Gears/Orbit/Waves/GridPulse (Gears/Orbit/Waves/
        // GridPulse εμπνευσμένα από τα αντίστοιχα στυλ του Windows Optimizer) + Network/Bokeh/Aurora/Starfield
        // (δικά μας). Το Glow παραμένει αμιγώς XAML-based (Border+Storyboard), δεν περνάει από εδώ. ----------
        private void BuildAnimatedBackground(BgAnimStyle style)
        {
            HeaderParticles.Children.Clear();
            switch (style)
            {
                case BgAnimStyle.Particles: BuildParticles(); break;
                case BgAnimStyle.Gears: BuildGears(); break;
                case BgAnimStyle.Orbit: BuildOrbit(); break;
                case BgAnimStyle.Waves: BuildWaves(); break;
                case BgAnimStyle.GridPulse: BuildGridPulse(); break;
                case BgAnimStyle.Network: BuildNetwork(); break;
                case BgAnimStyle.Bokeh: BuildBokeh(); break;
                case BgAnimStyle.Aurora: BuildAurora(); break;
                case BgAnimStyle.Starfield: BuildStarfield(); break;
            }
        }

        private static DoubleAnimation Loop(double from, double to, double seconds, bool autoReverse = true) => new()
        {
            From = from, To = to, Duration = TimeSpan.FromSeconds(seconds),
            AutoReverse = autoReverse, RepeatBehavior = RepeatBehavior.Forever
        };

        private void BuildParticles()
        {
            var rnd = new Random(7);
            for (int i = 0; i < 7; i++)
            {
                double size = 4 + rnd.Next(6);
                var dot = new Ellipse { Width = size, Height = size, Opacity = 0.42 };
                dot.SetResourceReference(Ellipse.FillProperty, "ClaudeBrush");
                Canvas.SetLeft(dot, rnd.Next(40, 1100));
                Canvas.SetTop(dot, rnd.Next(10, 100));

                var transform = new TranslateTransform(0, 0);
                dot.RenderTransform = transform;
                transform.BeginAnimation(TranslateTransform.XProperty, Loop(-15 - rnd.Next(15), 15 + rnd.Next(15), 6 + rnd.Next(6)));
                transform.BeginAnimation(TranslateTransform.YProperty, Loop(-8 - rnd.Next(8), 8 + rnd.Next(8), 5 + rnd.Next(7)));

                HeaderParticles.Children.Add(dot);
            }
        }

        /// <summary>Γρανάζια: κύκλοι με ακτινωτές "δοντιές" που περιστρέφονται αργά - στυλ Windows 11 εφέ.</summary>
        private void BuildGears()
        {
            var rnd = new Random(11);
            // ΔΙΟΡΘΩΣΗ: το πρώτο γρανάζι ήταν στο x=80, πολύ κοντά στο κινούμενο εικονίδιο του τίτλου (Canvas
            // 26x26 μέσα στο StackPanel με αριστερό περιθώριο 24 - δηλαδή περίπου x:24-50) και στο ίδιο ύψος·
            // με τα δόντια του γραναζιού (ακτίνα+6) η επικάλυψη ήταν ακόμα μεγαλύτερη. Μετακινήθηκε στα x=200,
            // αρκετά μακριά από εικονίδιο ΚΑΙ τίτλο "Κόμβος".
            var positions = new[] { (200.0, 45.0, 24.0), (1080.0, 50.0, 18.0), (560.0, 30.0, 14.0) };
            foreach (var (x, y, radius) in positions)
            {
                var gear = new Canvas { Width = radius * 2, Height = radius * 2, Opacity = 0.45, RenderTransformOrigin = new Point(0.5, 0.5) };
                var ring = new Ellipse { Width = radius * 2, Height = radius * 2 };
                ring.SetResourceReference(Shape.StrokeProperty, "ClaudeBrush");
                ring.StrokeThickness = 3;
                gear.Children.Add(ring);
                for (int t = 0; t < 8; t++)
                {
                    double angle = t * (Math.PI * 2 / 8);
                    var tooth = new Line
                    {
                        X1 = radius + Math.Cos(angle) * radius, Y1 = radius + Math.Sin(angle) * radius,
                        X2 = radius + Math.Cos(angle) * (radius + 6), Y2 = radius + Math.Sin(angle) * (radius + 6),
                        StrokeThickness = 3
                    };
                    tooth.SetResourceReference(Shape.StrokeProperty, "ClaudeBrush");
                    gear.Children.Add(tooth);
                }
                var rotate = new RotateTransform(0);
                gear.RenderTransform = rotate;
                var spin = new DoubleAnimation { From = 0, To = 360, Duration = TimeSpan.FromSeconds(18 + rnd.Next(10)), RepeatBehavior = RepeatBehavior.Forever };
                rotate.BeginAnimation(RotateTransform.AngleProperty, spin);
                Canvas.SetLeft(gear, x - radius);
                Canvas.SetTop(gear, y - radius);
                HeaderParticles.Children.Add(gear);
            }
        }

        /// <summary>Τροχιές: μικρές "σελήνες" που περιστρέφονται γύρω από σταθερά κέντρα.</summary>
        private void BuildOrbit()
        {
            var rnd = new Random(23);
            var centers = new[] { (140.0, 50.0), (620.0, 35.0), (1020.0, 55.0) };
            foreach (var (cx, cy) in centers)
            {
                double radius = 20 + rnd.Next(15);

                // ΔΙΟΡΘΩΣΗ: πριν υπήρχε μόνο η κινούμενη κουκκίδα (moon) - καμία γραμμή δεν σχεδίαζε την ίδια
                // την τροχιά της, οπότε ο κύκλος γύρω της δεν φαινόταν καθόλου. Ένα στατικό Ellipse με μόνο
                // Stroke (χωρίς Fill) κεντραρισμένο στο ίδιο σημείο δείχνει το μονοπάτι της τροχιάς.
                var ring = new Ellipse
                {
                    Width = radius * 2, Height = radius * 2, Opacity = 0.22,
                    StrokeThickness = 1.2, Fill = Brushes.Transparent
                };
                ring.SetResourceReference(Shape.StrokeProperty, "ClaudeBrush");
                Canvas.SetLeft(ring, cx - radius);
                Canvas.SetTop(ring, cy - radius);
                HeaderParticles.Children.Add(ring);

                var pivot = new Canvas { Width = 1, Height = 1 };
                Canvas.SetLeft(pivot, cx);
                Canvas.SetTop(pivot, cy);
                var moon = new Ellipse { Width = 6, Height = 6, Opacity = 0.55 };
                moon.SetResourceReference(Ellipse.FillProperty, "ClaudeBrush");
                Canvas.SetLeft(moon, radius);
                Canvas.SetTop(moon, 0);
                var rotate = new RotateTransform(0, 0, 0);
                pivot.RenderTransform = rotate;
                rotate.BeginAnimation(RotateTransform.AngleProperty,
                    new DoubleAnimation { From = 0, To = 360, Duration = TimeSpan.FromSeconds(10 + rnd.Next(8)), RepeatBehavior = RepeatBehavior.Forever });
                pivot.Children.Add(moon);
                HeaderParticles.Children.Add(pivot);
            }
        }

        /// <summary>Κύματα: οριζόντιες γραμμές που ταλαντεύονται κατακόρυφα με μετατοπισμένη φάση.</summary>
        private void BuildWaves()
        {
            for (int i = 0; i < 3; i++)
            {
                var line = new Line { X1 = 20, Y1 = 0, X2 = 1140, Y2 = 0, StrokeThickness = 2.5, Opacity = 0.35 };
                line.SetResourceReference(Shape.StrokeProperty, "ClaudeBrush");
                double baseY = 35 + i * 22;
                Canvas.SetTop(line, baseY);
                var transform = new TranslateTransform();
                line.RenderTransform = transform;
                var anim = Loop(-6, 6, 3.5 + i * 0.8);
                anim.BeginTime = TimeSpan.FromSeconds(i * 0.5);
                transform.BeginAnimation(TranslateTransform.YProperty, anim);
                HeaderParticles.Children.Add(line);
            }
        }

        /// <summary>Πλέγμα: κουκκίδες σε πλέγμα που "χτυπούν" (opacity pulse) με σταδιακή καθυστέρηση.</summary>
        private void BuildGridPulse()
        {
            for (int col = 0; col < 10; col++)
            {
                for (int row = 0; row < 2; row++)
                {
                    var dot = new Ellipse { Width = 5, Height = 5, Opacity = 0.25 };
                    dot.SetResourceReference(Ellipse.FillProperty, "ClaudeBrush");
                    Canvas.SetLeft(dot, 60 + col * 115);
                    Canvas.SetTop(dot, 28 + row * 40);
                    var pulse = new DoubleAnimation
                    {
                        From = 0.18, To = 0.65, Duration = TimeSpan.FromSeconds(1.6),
                        AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever,
                        BeginTime = TimeSpan.FromSeconds((col + row) * 0.15)
                    };
                    dot.BeginAnimation(UIElement.OpacityProperty, pulse);
                    HeaderParticles.Children.Add(dot);
                }
            }
        }

        /// <summary>Network: κόμβοι συνδεδεμένοι με γραμμές που σιγοσβήνουν/ανάβουν - ταιριάζει θεματικά με το "Κόμβος".</summary>
        private void BuildNetwork()
        {
            var rnd = new Random(41);
            var nodes = new List<Point>();
            for (int i = 0; i < 9; i++)
                nodes.Add(new Point(rnd.Next(40, 1140), rnd.Next(12, 95)));

            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = i + 1; j < nodes.Count; j++)
                {
                    double dist = (nodes[i] - nodes[j]).Length;
                    if (dist > 220) continue;
                    var line = new Line { X1 = nodes[i].X, Y1 = nodes[i].Y, X2 = nodes[j].X, Y2 = nodes[j].Y, StrokeThickness = 1.25 };
                    line.SetResourceReference(Shape.StrokeProperty, "ClaudeBrush");
                    var pulse = new DoubleAnimation
                    {
                        From = 0.14, To = 0.4, Duration = TimeSpan.FromSeconds(3 + rnd.NextDouble() * 3),
                        AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever
                    };
                    line.BeginAnimation(UIElement.OpacityProperty, pulse);
                    HeaderParticles.Children.Add(line);
                }
            }
            foreach (var p in nodes)
            {
                var node = new Ellipse { Width = 6, Height = 6, Opacity = 0.65 };
                node.SetResourceReference(Ellipse.FillProperty, "ClaudeBrush");
                Canvas.SetLeft(node, p.X - 2.5);
                Canvas.SetTop(node, p.Y - 2.5);
                var scale = new ScaleTransform(1, 1);
                node.RenderTransform = scale;
                node.RenderTransformOrigin = new Point(0.5, 0.5);
                var pulse = Loop(0.7, 1.5, 2 + rnd.NextDouble() * 2);
                scale.BeginAnimation(ScaleTransform.ScaleXProperty, pulse);
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, pulse);
                HeaderParticles.Children.Add(node);
            }
        }

        /// <summary>Bokeh: μεγάλες, θολές, ημιδιάφανες φούσκες που αργοκινούνται.</summary>
        private void BuildBokeh()
        {
            var rnd = new Random(59);
            for (int i = 0; i < 5; i++)
            {
                double size = 26 + rnd.Next(30);
                var bubble = new Ellipse { Width = size, Height = size, Opacity = 0.22, Effect = new BlurEffect { Radius = 6 } };
                bubble.SetResourceReference(Ellipse.FillProperty, "ClaudeBrush");
                Canvas.SetLeft(bubble, rnd.Next(20, 1120));
                Canvas.SetTop(bubble, rnd.Next(0, 80));
                var transform = new TranslateTransform();
                bubble.RenderTransform = transform;
                transform.BeginAnimation(TranslateTransform.XProperty, Loop(-20 - rnd.Next(20), 20 + rnd.Next(20), 10 + rnd.Next(8)));
                transform.BeginAnimation(TranslateTransform.YProperty, Loop(-10, 10, 8 + rnd.Next(6)));
                HeaderParticles.Children.Add(bubble);
            }
        }

        /// <summary>Aurora: πλατιές, θολές λωρίδες χρώματος που ολισθαίνουν οριζόντια σαν βόρειο σέλας.</summary>
        private void BuildAurora()
        {
            for (int i = 0; i < 3; i++)
            {
                var streak = new Ellipse
                {
                    Width = 260, Height = 50, Opacity = 0.28,
                    Effect = new BlurEffect { Radius = 18 }
                };
                streak.SetResourceReference(Ellipse.FillProperty, "ClaudeBrush");
                Canvas.SetLeft(streak, 60 + i * 380);
                Canvas.SetTop(streak, 5 + i * 15);
                var transform = new TranslateTransform();
                streak.RenderTransform = transform;
                transform.BeginAnimation(TranslateTransform.XProperty, Loop(-40, 40, 9 + i * 2));
                streak.BeginAnimation(UIElement.OpacityProperty, Loop(0.18, 0.4, 5 + i));
                HeaderParticles.Children.Add(streak);
            }
        }

        /// <summary>Starfield: πολλές μικρές κουκκίδες που τρεμοπαίζουν (twinkle) και αργοσέρνονται πλευρικά.</summary>
        private void BuildStarfield()
        {
            var rnd = new Random(83);
            for (int i = 0; i < 16; i++)
            {
                double size = 2.5 + rnd.NextDouble() * 2.5;
                var star = new Ellipse { Width = size, Height = size, Opacity = 0.45 };
                star.SetResourceReference(Ellipse.FillProperty, "ClaudeBrush");
                Canvas.SetLeft(star, rnd.Next(20, 1140));
                Canvas.SetTop(star, rnd.Next(5, 95));
                star.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation
                {
                    From = 0.2, To = 0.3 + rnd.NextDouble() * 0.5, Duration = TimeSpan.FromSeconds(1 + rnd.NextDouble() * 3),
                    AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever
                });
                HeaderParticles.Children.Add(star);
            }
        }

        // ---------- Sidebar position ----------
        internal void ApplySidebarPosition()
        {
            if (_sidebarRight)
            {
                Col0.Width = new GridLength(1, GridUnitType.Star);
                Grid.SetColumn(SidebarBorder, 1);
                Grid.SetColumn(MainContentGrid, 0);
                Grid.SetColumn(DetailsPanel, 0);
                DetailsPanel.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else
            {
                Col1.Width = new GridLength(1, GridUnitType.Star);
                Grid.SetColumn(SidebarBorder, 0);
                Grid.SetColumn(MainContentGrid, 1);
                Grid.SetColumn(DetailsPanel, 1);
                DetailsPanel.HorizontalAlignment = HorizontalAlignment.Right;
            }
            ApplySidebarWidth();
        }

        // ---------- Chat ----------
        private static readonly Dictionary<string, string> ImageMediaTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            [".png"] = "image/png", [".jpg"] = "image/jpeg", [".jpeg"] = "image/jpeg",
            [".gif"] = "image/gif", [".webp"] = "image/webp"
        };

        /// <summary>Το κουμπί επισύναψης δέχεται ΚΑΘΕ είδος αρχείου, όχι μόνο εικόνες - οι εικόνες ακολουθούν
        /// το ήδη υπάρχον pending-attachment/vision μονοπάτι (thumbnail, στέλνεται σαν image block στο API),
        /// ενώ οποιοδήποτε άλλο αρχείο κειμένου/κώδικα (.md/.txt/.json/.csv/κ.λπ.) μπαίνει απευθείας στη
        /// συζήτηση σαν πλαίσιο κειμένου - ίδιο μονοπάτι με το ImportFileAsContext (Ρυθμίσεις → Project),
        /// αφού το Anthropic API δεν δέχεται αυθαίρετα αρχεία σαν "εικόνα".</summary>
        private void AttachButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Όλα τα αρχεία (*.*)|*.*|Εικόνες (*.png;*.jpg;*.jpeg;*.gif;*.webp)|*.png;*.jpg;*.jpeg;*.gif;*.webp"
            };
            if (dialog.ShowDialog(this) != true) return;

            var ext = System.IO.Path.GetExtension(dialog.FileName);
            if (ImageMediaTypes.ContainsKey(ext))
            {
                SetPendingImageFromFile(dialog.FileName);
                return;
            }

            if (!ImportFileAsContext(dialog.FileName, out var error) && error != null)
                MessageBox.Show(this, error, L.LinkOpenError);
        }

        private void SetPendingImageFromFile(string path)
        {
            _pendingImagePath = path;
            PendingAttachmentName.Text = System.IO.Path.GetFileName(path);
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = new Uri(path);
                bmp.EndInit();
                PendingAttachmentThumb.Source = bmp;
            }
            catch { /* keep the chip text-only if the preview fails to decode */ }
            PendingAttachmentChip.Visibility = Visibility.Visible;
        }

        /// <summary>Επικολλημένη εικόνα (π.χ. στιγμιότυπο οθόνης) από το clipboard δεν έχει δικό της αρχείο -
        /// αποθηκεύεται σε ένα προσωρινό PNG ώστε να ακολουθήσει το ίδιο μονοπάτι με τα κανονικά attachments.</summary>
        private void SetPendingImageFromClipboardBitmap(BitmapSource src)
        {
            try
            {
                var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"kombos-paste-{Guid.NewGuid():N}.png");
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(src));
                using (var fs = System.IO.File.Create(path))
                    encoder.Save(fs);
                SetPendingImageFromFile(path);
            }
            catch { /* clipboard image decode/save failed - just ignore the paste */ }
        }

        private void RemoveAttachmentButton_Click(object sender, RoutedEventArgs e)
        {
            _pendingImagePath = null;
            PendingAttachmentThumb.Source = null;
            PendingAttachmentChip.Visibility = Visibility.Collapsed;
        }

        /// <summary>Drag & drop πάνω στο chat panel - διαλειτουργικότητα με Explorer/άλλες εφαρμογές: εικόνα ->
        /// επισύναψη, αρχείο/φάκελος κειμένου/κώδικα -> εισαγωγή ως context στη συζήτηση (ίδια λογική με τα
        /// κουμπιά "Εισαγωγή" στις Ρυθμίσεις).</summary>
        private void ChatPanelBorder_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            if (e.Data.GetData(DataFormats.FileDrop) is not string[] files || files.Length == 0) return;

            var imagePath = files.FirstOrDefault(f => ImageMediaTypes.ContainsKey(System.IO.Path.GetExtension(f)));
            if (imagePath != null) { SetPendingImageFromFile(imagePath); return; }

            foreach (var path in files)
            {
                if (System.IO.Directory.Exists(path))
                    ImportFolderAsContext(path, out var folderError);
                else if (System.IO.File.Exists(path))
                    ImportFileAsContext(path, out var fileError);
            }
        }

        private void ChatPanelBorder_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        // ---------- Project import ως chat context: κοινή λογική για Settings + drag&drop ----------
        private static readonly string[] ImportableExtensions =
        {
            ".txt", ".md", ".json", ".yaml", ".yml", ".xml", ".csv",
            ".cs", ".xaml", ".py", ".js", ".jsx", ".ts", ".tsx", ".html", ".css",
            ".java", ".kt", ".go", ".rb", ".php", ".c", ".cpp", ".h", ".hpp", ".sql", ".sh"
        };
        private static readonly string[] ExcludedDirNames =
        {
            "bin", "obj", "node_modules", ".git", ".vs", "dist", "build", "packages", ".idea"
        };

        internal bool ImportFileAsContext(string path, out string? error)
        {
            error = null;
            string content;
            try { content = System.IO.File.ReadAllText(path); }
            catch (Exception ex) { error = ex.Message; return false; }

            const int maxChars = 20000;
            if (content.Length > maxChars) content = content[..maxChars] + "\n…";

            var fileName = System.IO.Path.GetFileName(path);
            var withPrefix = L.ImportedFilePrefix(fileName) + content;
            var importTurn = new ChatTurn { Role = "user", Content = withPrefix };
            AddChatBubble(true, withPrefix, historyTurn: importTurn);
            _history.Add(importTurn);
            return true;
        }

        internal bool ImportFolderAsContext(string folderPath, out string? error)
        {
            error = null;
            const int maxTotalChars = 100_000;
            var sb = new System.Text.StringBuilder();
            int fileCount = 0;
            bool truncated = false;

            IEnumerable<string> files;
            try
            {
                files = System.IO.Directory.EnumerateFiles(folderPath, "*", System.IO.SearchOption.AllDirectories)
                    .Where(f => ImportableExtensions.Contains(System.IO.Path.GetExtension(f).ToLowerInvariant()))
                    .Where(f => !f.Split(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar)
                                   .Any(seg => ExcludedDirNames.Contains(seg, StringComparer.OrdinalIgnoreCase)));
            }
            catch (Exception ex) { error = ex.Message; return false; }

            foreach (var file in files)
            {
                if (sb.Length >= maxTotalChars) { truncated = true; break; }
                string text;
                try { text = System.IO.File.ReadAllText(file); }
                catch { continue; }

                var relPath = System.IO.Path.GetRelativePath(folderPath, file);
                var remaining = maxTotalChars - sb.Length;
                var chunk = text.Length > remaining ? text[..remaining] : text;
                sb.AppendLine($"--- {relPath} ---");
                sb.AppendLine(chunk);
                sb.AppendLine();
                fileCount++;
                if (text.Length > remaining) { truncated = true; break; }
            }

            if (fileCount == 0) return false;
            if (truncated) sb.AppendLine("…");

            var folderName = System.IO.Path.GetFileName(folderPath.TrimEnd(System.IO.Path.DirectorySeparatorChar));
            var withPrefix = L.ImportedFolderPrefix(folderName, fileCount) + sb;
            var importTurn = new ChatTurn { Role = "user", Content = withPrefix };
            AddChatBubble(true, withPrefix, historyTurn: importTurn);
            _history.Add(importTurn);
            return true;
        }

        /// <summary>"Εισαγωγή project" v3.6.0: αντί να διαβάσει και να στείλει ΟΛΟΚΛΗΡΟ το περιεχόμενο κάθε
        /// αρχείου (ImportFolderAsContext, ακριβό σε tokens και αργό), χτίζει/ενημερώνει τοπικά ένα ευρετήριο
        /// και στέλνει στον agent ΜΟΝΟ το δέντρο των αρχείων. Ο agent/χρήστης μετά ζητά στοχευμένα
        /// συγκεκριμένα αρχεία με /readfile - όπως ακριβώς θα διάβαζε κώδικα ένας πραγματικός συνεργάτης.</summary>
        internal bool ImportProjectIndexAsContext(string folderPath, out string? error)
        {
            error = null;
            ProjectIndex index;
            try { index = ProjectMemoryStore.BuildOrUpdateIndex(folderPath); }
            catch (Exception ex) { error = ex.Message; return false; }

            if (index.Files.Count == 0) return false;

            _activeProjectPath = folderPath;
            SaveSettings();

            var tree = ProjectMemoryStore.BuildTreeText(index);
            var folderName = System.IO.Path.GetFileName(folderPath.TrimEnd(System.IO.Path.DirectorySeparatorChar));
            var withPrefix = L.ImportedProjectIndexPrefix(folderName, index.Files.Count) + tree;
            var importTurn = new ChatTurn { Role = "user", Content = withPrefix };
            AddChatBubble(true, withPrefix, historyTurn: importTurn);
            _history.Add(importTurn);
            return true;
        }

        /// <summary>/readfile - φέρνει στοχευμένα ΕΝΑ αρχείο του ενεργού project στη συζήτηση, αντί να έχει
        /// ήδη σταλεί (και να χρεώνεται σε κάθε turn) ολόκληρο το project. Περιορισμένο ΜΟΝΟ μέσα στο
        /// _activeProjectPath - καμία πρόσβαση εκτός του ρητά εισηγμένου project.</summary>
        private Task<string> ReadProjectFileAsync(string relativePath)
        {
            relativePath = relativePath.Trim();
            if (string.IsNullOrEmpty(_activeProjectPath)) return Task.FromResult(L.NoActiveProject);
            if (string.IsNullOrEmpty(relativePath)) return Task.FromResult(L.ReadFileUsage);

            var fullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(_activeProjectPath, relativePath));
            var rootFull = System.IO.Path.GetFullPath(_activeProjectPath);
            if (!fullPath.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(L.ReadFileOutsideProject);

            string content;
            try { content = System.IO.File.ReadAllText(fullPath); }
            catch (Exception ex) { return Task.FromResult(L.SomethingWentWrong(ex.Message)); }

            const int maxChars = 20000;
            if (content.Length > maxChars) content = content[..maxChars] + "\n…";

            var withPrefix = L.ImportedFilePrefix(relativePath) + content;
            var importTurn = new ChatTurn { Role = "user", Content = withPrefix };
            AddChatBubble(true, withPrefix, historyTurn: importTurn);
            _history.Add(importTurn);
            return Task.FromResult(L.ReadFileAdded(relativePath));
        }

        /// <summary>/search (v4.5.0) - τοπική αναζήτηση λέξεων-κλειδιών σε ΟΛΟ το περιεχόμενο του ενεργού
        /// project ("local RAG-lite" - βλ. roadmap honesty note για γιατί ΟΧΙ embeddings/vector DB: κάθε chunk
        /// θα χρειαζόταν δική του κλήση API, επαναλαμβανόμενο κόστος). Απλή βαθμολόγηση με συχνότητα όρου
        /// (case-insensitive) πάνω στα ήδη ευρετηριασμένα αρχεία του ProjectMemoryStore - καμία κλήση AI,
        /// μηδενικό κόστος tokens μέχρι ο χρήστης να διαλέξει /readfile σε ένα από τα αποτελέσματα.</summary>
        private async Task<string> SearchProjectAsync(string query)
        {
            query = query.Trim();
            if (string.IsNullOrEmpty(_activeProjectPath)) return L.NoActiveProject;
            if (string.IsNullOrEmpty(query)) return L.SearchUsage;

            var index = ProjectMemoryStore.LoadIndex(_activeProjectPath);
            if (index == null || index.Files.Count == 0) return L.NoActiveProject;

            var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var scored = new List<(string RelPath, int Score, string Snippet)>();

            foreach (var file in index.Files)
            {
                var fullPath = System.IO.Path.Combine(_activeProjectPath, file.RelativePath);
                string content;
                try { content = await System.IO.File.ReadAllTextAsync(fullPath); }
                catch { continue; }

                int score = terms.Sum(term => CountOccurrences(content, term));
                if (score == 0) continue;

                scored.Add((file.RelativePath, score, ExtractSnippet(content, terms[0])));
            }

            if (scored.Count == 0) return L.SearchNoResults(query);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(L.SearchResultsHeader(query));
            foreach (var r in scored.OrderByDescending(s => s.Score).Take(5))
                sb.AppendLine($"- `{r.RelPath}` ({r.Score}) — {r.Snippet}");
            sb.AppendLine();
            sb.Append(L.SearchHint);
            return sb.ToString();
        }

        private static int CountOccurrences(string haystack, string needle)
        {
            if (needle.Length == 0) return 0;
            int count = 0, idx = 0;
            while ((idx = haystack.IndexOf(needle, idx, StringComparison.OrdinalIgnoreCase)) != -1) { count++; idx += needle.Length; }
            return count;
        }

        private static readonly Regex PlainTextMarkdownRegex = new(@"[*_`#]+|~~", RegexOptions.Compiled);

        /// <summary>Αφαιρεί τα πιο κοινά markdown σύμβολα για "Αντιγραφή ως απλό κείμενο" - όχι πλήρης markdown
        /// parser, απλός καθαρισμός (ίδιο πνεύμα με SpeechService.StripMarkdownForSpeech).</summary>
        private static string ToPlainText(string markdown) => PlainTextMarkdownRegex.Replace(markdown, "");

        private static string ExtractSnippet(string content, string term)
        {
            var idx = content.IndexOf(term, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return "";
            var start = Math.Max(0, idx - 40);
            var len = Math.Min(120, content.Length - start);
            return "…" + content.Substring(start, len).Replace("\r", "").Replace("\n", " ").Trim() + "…";
        }

        /// <summary>/image (v4.5.0) - πραγματική δημιουργία εικόνας ΜΕΣΑ στη συνομιλία (OpenAI Images API,
        /// βλ. Services/OpenAiImageClient.cs), χρησιμοποιώντας το ήδη αποθηκευμένο κλειδί ChatGPT - όχι απλή
        /// παραπομπή σε εξωτερικό εργαλείο. Δείχνει τη φούσκα (με την εικόνα) μόνο του και επιστρέφει κενό
        /// string ώστε ο dispatcher (βλ. πάνω) να μην προσθέσει και δεύτερη, άδεια φούσκα.</summary>
        private async Task<string> GenerateImageAsync(string prompt)
        {
            prompt = prompt.Trim();
            if (string.IsNullOrEmpty(prompt)) return L.ImageUsage;
            if (!_apiKeys.TryGetValue("ChatGPT", out var key) || string.IsNullOrWhiteSpace(key)) return L.ImageNoKey;

            try
            {
                var filePath = await OpenAiImageClient.GenerateAndSaveAsync(key, prompt);
                AddChatBubble(false, "", imagePath: filePath, showRegenerate: false);
                return "";
            }
            catch (Exception ex) { return L.SomethingWentWrong(ex.Message); }
        }

        /// <summary>/verify - τοπικός, μηδενικού-κόστους έλεγχος συμβατότητας: τρέχει το build/lint toolchain
        /// του ΕΝΕΡΓΟΥ project (αυτόματη ανίχνευση) και επιστρέφει pass/fail + έξοδο compiler, χωρίς καμία
        /// κλήση σε AI. Ένας deterministic έλεγχος πιάνει σφάλματα συμβατότητας πιο αξιόπιστα από το να
        /// ζητηθεί από ένα μοντέλο να κρίνει μόνο του τη δική του αλλαγή.</summary>
        private async Task<string> VerifyActiveProjectAsync(string arg)
        {
            if (string.IsNullOrEmpty(_activeProjectPath)) return L.NoActiveProject;
            var result = await ProjectVerifier.VerifyAsync(_activeProjectPath);
            if (!result.Ran) return L.VerifyCouldNotRun(result.Output);
            var status = result.Success ? L.VerifySuccess(result.Command) : L.VerifyFailed(result.Command);
            return string.IsNullOrWhiteSpace(result.Output) ? status : $"{status}\n```\n{result.Output}\n```";
        }

        private void ThinkingToggleButton_Click(object sender, RoutedEventArgs e)
        {
            _thinkingEnabled = !_thinkingEnabled;
            if (_thinkingEnabled)
                ThinkingToggleButton.SetResourceReference(Button.BackgroundProperty, "ClaudeBrush");
            else
                ThinkingToggleButton.Background = Brushes.Transparent;
        }

        private void WebSearchToggleButton_Click(object sender, RoutedEventArgs e)
        {
            _webSearchEnabled = !_webSearchEnabled;
            if (_webSearchEnabled)
                WebSearchToggleButton.SetResourceReference(Button.BackgroundProperty, "ClaudeBrush");
            else
                WebSearchToggleButton.Background = Brushes.Transparent;
        }

        private void CompareToggleButton_Click(object sender, RoutedEventArgs e)
        {
            _compareModeEnabled = !_compareModeEnabled;
            if (_compareModeEnabled)
                CompareToggleButton.SetResourceReference(Button.BackgroundProperty, "ClaudeBrush");
            else
                CompareToggleButton.Background = Brushes.Transparent;
        }

        /// <summary>Ονόματα agents με πραγματική δυνατότητα κλήσης αυτή τη στιγμή (Claude πάντα, τα υπόλοιπα
        /// μόνο αν έχουν αποθηκευμένο API key) - ίδια λογική με το Compare mode.</summary>
        private List<string> GetAvailableAgentNames()
        {
            var names = new List<string> { "Claude" };
            names.AddRange(Catalog.Tools.Where(t => t.OpenAiCompatBaseUrl != null && _apiKeys.ContainsKey(t.Name) && !string.IsNullOrWhiteSpace(_apiKeys[t.Name])).Select(t => t.Name));
            return names;
        }

        /// <summary>Στέλνει ένα ΜΕΜΟΝΩΜΕΝΟ prompt (όχι το πλήρες _history) σε έναν agent - βάση για το Handoff
        /// pipeline, όπου κάθε στάδιο είναι ανεξάρτητο από τη συνέχεια της κύριας συνομιλίας.</summary>
        private async Task<ClaudeResult> SendToSingleAgentAsync(string agentName, string prompt, CancellationToken ct)
        {
            var scratchHistory = new List<ChatTurn> { new ChatTurn { Role = "user", Content = prompt } };
            if (agentName == "Claude")
                return await _claude.SendAsync(L.SystemPrompt(BuildCatalogJson(), agentName, ComposeInstructions()), scratchHistory, null, false, ct, false, null);

            var tool = Catalog.Tools.First(t => t.Name == agentName);
            return await OpenAiCompatibleClient.SendAsync(tool.OpenAiCompatBaseUrl!, tool.OpenAiCompatModel!, _apiKeys[agentName],
                L.SystemPrompt(BuildCatalogJson(), agentName, ComposeInstructions()), scratchHistory, ct);
        }

        private void HandoffButton_Click(object sender, RoutedEventArgs e)
        {
            var agents = GetAvailableAgentNames();
            HandoffTitleText.Text = L.HandoffTitle;
            HandoffStepALabel.Text = L.HandoffStepALabel;
            HandoffStepBLabel.Text = L.HandoffStepBLabel;
            HandoffHintText.Text = L.HandoffHint;
            HandoffRunButton.Content = L.HandoffRunButton;

            HandoffAgentACombo.ItemsSource = agents;
            HandoffAgentBCombo.ItemsSource = agents;
            if (HandoffAgentACombo.SelectedItem == null) HandoffAgentACombo.SelectedIndex = 0;
            if (HandoffAgentBCombo.SelectedItem == null) HandoffAgentBCombo.SelectedIndex = agents.Count > 1 ? 1 : 0;

            HandoffPopup.PlacementTarget = HandoffButton;
            HandoffPopup.IsOpen = !HandoffPopup.IsOpen;
        }

        /// <summary>Βιβλιοθήκη Prompts (v4.1.0, ιδέα εμπνευσμένη από γκαλερί έτοιμων "assistants"/"masks" σε
        /// άλλες open-source εφαρμογές - βλ. Data/PromptLibrary.cs για γιατί το περιεχόμενο είναι δικό μας,
        /// όχι αντιγραμμένο). Επιλογή γεμίζει το πεδίο μηνύματος - ο χρήστης αποφασίζει πότε/αν θα το στείλει,
        /// άρα μηδενικό κόστος tokens μέχρι εκείνη τη στιγμή.</summary>
        private bool _listening;

        /// <summary>Push-to-talk φωνητική εισαγωγή (v4.5.0) - πάτα, μίλα ΜΙΑ φορά, το αναγνωρισμένο κείμενο
        /// προστίθεται στο πεδίο μηνύματος. Όχι συνεχής ακρόαση/πραγματικός realtime διάλογος (βλ. σημείωση
        /// στο roadmap) - 100% τοπικό μέσω Windows Speech Recognition, καμία κλήση cloud/API key.</summary>
        private void MicButton_Click(object sender, RoutedEventArgs e)
        {
            if (_listening)
            {
                SpeechService.StopListening();
                _listening = false;
                SetMicIconListening(false);
                return;
            }

            bool started = SpeechService.TryStartListening(recognized =>
            {
                Dispatcher.Invoke(() =>
                {
                    _listening = false;
                    SetMicIconListening(false);
                    if (string.IsNullOrWhiteSpace(recognized)) return;
                    ChatInputBox.Text = string.IsNullOrEmpty(ChatInputBox.Text)
                        ? recognized
                        : ChatInputBox.Text.TrimEnd() + " " + recognized;
                    ChatInputBox.CaretIndex = ChatInputBox.Text.Length;
                    ChatInputBox.Focus();
                });
            });

            if (started)
            {
                _listening = true;
                SetMicIconListening(true); // κόκκινη κουκκίδα - ακούει
            }
            else
            {
                MessageBox.Show(this, L.MicUnavailable, L.WindowTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>v4.6.0 - vector εικονίδιο αντί για emoji: περίγραμμα μικροφώνου όταν αδρανές, γεμάτη
        /// κόκκινη κουκκίδα όσο ακούει (ίδιο Path element, μόνο η Data/Fill/Stroke αλλάζουν).</summary>
        private void SetMicIconListening(bool listening)
        {
            if (listening)
            {
                MicIcon.Data = new EllipseGeometry(new Point(12, 12), 7, 7);
                MicIcon.Fill = new SolidColorBrush(Color.FromRgb(0xE0, 0x4E, 0x4E));
                MicIcon.Stroke = null;
            }
            else
            {
                MicIcon.Data = (Geometry)FindResource("IconMic");
                MicIcon.Fill = Brushes.Transparent;
                MicIcon.SetResourceReference(System.Windows.Shapes.Shape.StrokeProperty, "InkBrush");
            }
        }

        private void PromptLibraryButton_Click(object sender, RoutedEventArgs e)
        {
            BuildPromptLibraryList();
            PromptLibraryPopup.PlacementTarget = PromptLibraryButton;
            PromptLibraryPopup.IsOpen = !PromptLibraryPopup.IsOpen;
        }

        private void BuildPromptLibraryList()
        {
            PromptLibraryTitleText.Text = L.PromptLibraryTitle;
            PromptLibraryListPanel.Children.Clear();

            string? lastCategory = null;
            foreach (var item in PromptLibrary.Items)
            {
                var cat = Catalog.Categories.FirstOrDefault(c => c.Id == item.CategoryKey);
                var catLabel = cat != null ? L.PickToolText(cat.Label, cat.LabelEn, cat.LabelFr, cat.LabelDe) : item.CategoryKey;
                if (catLabel != lastCategory)
                {
                    var header = new TextBlock
                    {
                        Text = (cat?.Icon ?? "") + " " + catLabel, FontSize = 10.5, FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, lastCategory == null ? 0 : 10, 0, 4)
                    };
                    header.SetResourceReference(TextBlock.ForegroundProperty, "InkSoftBrush");
                    PromptLibraryListPanel.Children.Add(header);
                    lastCategory = catLabel;
                }

                var title = L.PickToolText(item.Title, item.TitleEn, item.TitleFr, item.TitleDe);
                var prompt = L.PickToolText(item.Prompt, item.PromptEn, item.PromptFr, item.PromptDe);

                var row = new Border
                {
                    Padding = new Thickness(8, 6, 8, 6), CornerRadius = new CornerRadius(5), Cursor = Cursors.Hand,
                    Margin = new Thickness(0, 0, 0, 3), Background = Brushes.Transparent
                };
                var text = new TextBlock { Text = title, FontSize = 11.5 };
                text.SetResourceReference(TextBlock.ForegroundProperty, "InkBrush");
                row.Child = text;
                row.MouseEnter += (s, e2) => row.SetResourceReference(Border.BackgroundProperty, "PanelBrush");
                row.MouseLeave += (s, e2) => row.Background = Brushes.Transparent;
                row.MouseLeftButtonUp += (s, e2) =>
                {
                    ChatInputBox.Text = prompt;
                    ChatInputBox.Focus();
                    ChatInputBox.CaretIndex = prompt.Length;
                    PromptLibraryPopup.IsOpen = false;
                };
                PromptLibraryListPanel.Children.Add(row);
            }
        }

        /// <summary>Handoff: το κείμενο του χρήστη πάει πρώτα στον Agent A (πρόχειρη απάντηση), μετά η
        /// απάντησή του πάει στον Agent B μαζί με το αρχικό αίτημα, ζητώντας βελτίωση/κριτική - το "collective"
        /// σκέλος του Nexus, agents που χτίζουν ο ένας πάνω στη δουλειά του άλλου. Ανεξάρτητο από το _history
        /// της κύριας συνομιλίας (δεν το αλλοιώνει), ίδιο μοτίβο ασφάλειας με τις εντολές plugin.</summary>
        private async void HandoffRunButton_Click(object sender, RoutedEventArgs e)
        {
            var prompt = ChatInputBox.Text.Trim();
            if (string.IsNullOrEmpty(prompt) || _sending) return;
            var agentA = HandoffAgentACombo.SelectedItem as string ?? "Claude";
            var agentB = HandoffAgentBCombo.SelectedItem as string ?? "Claude";
            HandoffPopup.IsOpen = false;

            var userTurn = new ChatTurn { Role = "user", Content = prompt };
            AddChatBubble(true, prompt, historyTurn: null, showRegenerate: false);
            ChatInputBox.Text = "";

            _sending = true;
            _sendCts = new CancellationTokenSource();
            SendButton.Content = L.StopButton;
            TaskLabel.Text = L.Thinking;
            TaskProgress.IsIndeterminate = true;
            ShowTyping();
            try
            {
                ClaudeResult resultA;
                try { resultA = await SendToSingleAgentAsync(agentA, prompt, _sendCts.Token); }
                catch (OperationCanceledException) { return; }
                catch (Exception ex) { RemoveTyping(); AddChatBubble(false, L.SomethingWentWrong(ex.Message), agentName: agentA, showRegenerate: false); return; }

                var replyA = ExtractReplyText(resultA.ReplyText, out _);
                RemoveTyping();
                AddChatBubble(false, replyA, agentName: L.HandoffStageLabel(agentA, 1), showRegenerate: false);
                if (agentA == "Claude") { _claudeUsed += resultA.InputTokens + resultA.OutputTokens; _cacheReadTotal += resultA.CacheReadInputTokens; _cacheCreationTotal += resultA.CacheCreationInputTokens; }

                ShowTyping();
                var promptB = L.HandoffStageBPrompt(prompt, replyA);
                ClaudeResult resultB;
                try { resultB = await SendToSingleAgentAsync(agentB, promptB, _sendCts.Token); }
                catch (OperationCanceledException) { return; }
                catch (Exception ex) { RemoveTyping(); AddChatBubble(false, L.SomethingWentWrong(ex.Message), agentName: agentB, showRegenerate: false); return; }

                var replyB = ExtractReplyText(resultB.ReplyText, out var recsB);
                RemoveTyping();
                AddChatBubble(false, replyB, recsB, agentName: L.HandoffStageLabel(agentB, 2), showRegenerate: false);
                if (agentB == "Claude") { _claudeUsed += resultB.InputTokens + resultB.OutputTokens; _cacheReadTotal += resultB.CacheReadInputTokens; _cacheCreationTotal += resultB.CacheCreationInputTokens; }

                UpdateStatusBar();
            }
            finally
            {
                RemoveTyping();
                _sending = false;
                _sendCts?.Dispose();
                _sendCts = null;
                SendButton.Content = L.SendButton;
                TaskProgress.IsIndeterminate = false;
                TaskProgress.Value = 0;
                TaskLabel.Text = L.Ready;
                ChatInputBox.Focus();
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (_sending) { _sendCts?.Cancel(); return; }
            await SendMessageAsync();
        }

        private async void ChatInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Shift) == 0)
            {
                e.Handled = true;
                await SendMessageAsync();
                return;
            }

            // Ctrl+V με εικόνα στο clipboard (π.χ. στιγμιότυπο οθόνης) -> επισύναψη αντί για επικόλληση κενού κειμένου.
            if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control && Clipboard.ContainsImage())
            {
                e.Handled = true;
                var image = Clipboard.GetImage();
                if (image != null) SetPendingImageFromClipboardBitmap(image);
            }
        }

        private async Task SendMessageAsync()
        {
            var text = ChatInputBox.Text.Trim();
            if ((string.IsNullOrEmpty(text) && _pendingImagePath == null) || _sending) return;

            var imagePath = _pendingImagePath;
            _pendingImagePath = null;
            PendingAttachmentThumb.Source = null;
            PendingAttachmentChip.Visibility = Visibility.Collapsed;

            var userTurn = new ChatTurn { Role = "user", Content = text };
            AddChatBubble(true, text, imagePath: imagePath, historyTurn: userTurn);
            ChatInputBox.Text = "";

            // ΝΕΟ: εντολές "/κάτι ..." καταχωρημένες από plugins - τρέχουν τοπικά, δεν πάνε στον Claude και
            // δεν μπαίνουν στο _history (δεν είναι μέρος της συνομιλίας, είναι εργαλειακές).
            var firstWord = text.Split(' ', 2)[0];
            if (firstWord.StartsWith('/') && _pluginHost.SlashCommands.TryGetValue(firstWord, out var handler))
            {
                var arg = text.Length > firstWord.Length ? text[(firstWord.Length + 1)..] : "";
                ShowTyping();
                try
                {
                    var result = await handler(arg);
                    RemoveTyping();
                    // showRegenerate: false - η φούσκα εντολής plugin δεν αντιστοιχεί σε ChatTurn του _history,
                    // οπότε ένα κουμπί "Επανάληψη" πάνω της θα αφαιρούσε λάθος (προηγούμενο, πραγματικό) turn.
                    // Κενό αποτέλεσμα σημαίνει "ο handler έδειξε ήδη μόνος του τη φούσκα του" (π.χ. /image
                    // δείχνει εικόνα, όχι απλό κείμενο) - δεν προσθέτουμε άδεια δεύτερη φούσκα από πάνω.
                    if (!string.IsNullOrEmpty(result)) AddChatBubble(false, result, showRegenerate: false);
                }
                catch (Exception ex)
                {
                    RemoveTyping();
                    AddChatBubble(false, L.SomethingWentWrong(ex.Message), showRegenerate: false);
                }
                return;
            }

            _history.Add(userTurn);

            if (_compareModeEnabled) await CompareAgentsAsync(imagePath, restoreTextOnFail: text);
            else await GetAssistantReplyAsync(imagePath, restoreTextOnFail: text);
        }

        /// <summary>Στέλνει το ίδιο ερώτημα ταυτόχρονα στον Claude + όσα OpenAI-compatible εργαλεία έχουν
        /// αποθηκευμένο κλειδί API, και δείχνει όλες τις απαντήσεις μία κάτω από την άλλη με ετικέτα agent -
        /// για σύγκριση. Μόνο η απάντηση του Claude προστίθεται στο ιστορικό (συνέχεια συζήτησης)· οι υπόλοιπες
        /// είναι μόνο για σύγκριση σε αυτή τη στιγμή.</summary>
        private async Task CompareAgentsAsync(string? imagePath, string? restoreTextOnFail = null)
        {
            var epoch = _conversationEpoch;
            _sending = true;
            _sendCts = new CancellationTokenSource();
            SendButton.Content = L.StopButton;
            TaskLabel.Text = L.Thinking;
            TaskProgress.IsIndeterminate = true;
            ShowTyping();

            var candidates = new List<string> { "Claude" };
            candidates.AddRange(Catalog.Tools.Where(t => t.OpenAiCompatBaseUrl != null && _apiKeys.ContainsKey(t.Name) && !string.IsNullOrWhiteSpace(_apiKeys[t.Name])).Select(t => t.Name));

            try
            {
                foreach (var agentName in candidates)
                {
                    ClaudeResult result;
                    try
                    {
                        if (agentName == "Claude")
                        {
                            (byte[] Bytes, string MediaType)? image = null;
                            if (imagePath != null)
                            {
                                var ext = System.IO.Path.GetExtension(imagePath);
                                if (ImageMediaTypes.TryGetValue(ext, out var mediaType))
                                    image = (System.IO.File.ReadAllBytes(imagePath), mediaType);
                            }
                            result = await _claude.SendAsync(L.SystemPrompt(BuildCatalogJson(), agentName, ComposeInstructions()), BuildEffectiveHistory(), image, _thinkingEnabled, _sendCts.Token, _webSearchEnabled, ParseMcpServers());
                        }
                        else
                        {
                            var tool = Catalog.Tools.First(t => t.Name == agentName);
                            result = await OpenAiCompatibleClient.SendAsync(tool.OpenAiCompatBaseUrl!, tool.OpenAiCompatModel!, _apiKeys[agentName],
                                L.SystemPrompt(BuildCatalogJson(), agentName, ComposeInstructions()), BuildEffectiveHistory(), _sendCts.Token);
                        }
                    }
                    catch (OperationCanceledException) { break; }
                    catch (Exception ex)
                    {
                        if (epoch != _conversationEpoch) break;
                        AddChatBubble(false, L.SomethingWentWrong(ex.Message), agentName: agentName, showRegenerate: false);
                        continue;
                    }

                    if (epoch != _conversationEpoch) break;

                    var replyText = ExtractReplyText(result.ReplyText, out var recs);
                    var claudeTurn = agentName == "Claude" ? new ChatTurn { Role = "assistant", Content = result.ReplyText } : null;
                    AddChatBubble(false, replyText, recs, thinkingText: result.ThinkingText, citations: result.Citations,
                        agentName: agentName, showRegenerate: agentName == "Claude", historyTurn: claudeTurn);

                    if (agentName == "Claude")
                    {
                        _history.Add(claudeTurn!);
                        NotifyReplyIfInBackground(replyText);
                        _claudeUsed += result.InputTokens + result.OutputTokens;
                        _cacheReadTotal += result.CacheReadInputTokens;
                        _cacheCreationTotal += result.CacheCreationInputTokens;
                        UpdateStatusBar();
                        SaveCurrentConversation();
                    }
                }
            }
            finally
            {
                RemoveTyping();
                if (epoch == _conversationEpoch)
                {
                    // ΔΙΟΡΘΩΣΗ: αν ΚΑΝΕΝΑΣ agent (ούτε ο Claude) δεν πρόλαβε να απαντήσει (ακύρωση/σφάλμα πριν
                    // την πρώτη επιτυχή κλήση), το "user" turn που προστέθηκε στο SendMessageAsync μένει μόνο
                    // του στο ιστορικό - το επόμενο μήνυμα θα έστελνε 2 συνεχόμενα "user" turns στο API.
                    if (_history.Count > 0 && _history[^1].Role == "user")
                    {
                        _history.RemoveAt(_history.Count - 1);
                        if (restoreTextOnFail != null) ChatInputBox.Text = restoreTextOnFail;
                    }
                    _sending = false;
                    _sendCts?.Dispose();
                    _sendCts = null;
                    SendButton.Content = L.SendButton;
                    TaskProgress.IsIndeterminate = false;
                    TaskProgress.Value = 0;
                    TaskLabel.Text = L.Ready;
                    ChatInputBox.Focus();
                }
            }
        }

        // Μνήμη cache: το ίδιο catalog JSON ξαναχτίζεται σε ΚΑΘΕ μήνυμα ενώ σχεδόν ποτέ δεν αλλάζει
        // (μόνο σε αλλαγή γλώσσας ή φόρτωση/αφαίρεση plugin) - η επανάχρησή του κρατάει το system prompt
        // byte-identical μεταξύ διαδοχικών κλήσεων, που είναι ακριβώς αυτό που χρειάζεται το prompt caching
        // κάθε agent (κανόνας ελάχιστου κόστους τόκενς).
        private string? _catalogJsonCache;
        private Lang _catalogJsonCacheLang;
        private int _catalogJsonCacheToolCount = -1;

        private string BuildCatalogJson()
        {
            if (_catalogJsonCache != null && _catalogJsonCacheLang == L.Current && _catalogJsonCacheToolCount == Catalog.Tools.Count)
                return _catalogJsonCache;

            var simplified = Catalog.Tools.Select(t => new { name = t.Name, cat = t.CategoryId, desc = L.PickToolText(t.Description, t.DescriptionEn, t.DescriptionFr, t.DescriptionDe), mcp = t.HasMcp });
            _catalogJsonCache = JsonSerializer.Serialize(simplified);
            _catalogJsonCacheLang = L.Current;
            _catalogJsonCacheToolCount = Catalog.Tools.Count;
            return _catalogJsonCache;
        }

        internal static string ExtractReplyText(string rawReply, out List<ToolRecommendation> recs)
        {
            var replyText = rawReply;
            recs = new List<ToolRecommendation>();
            try
            {
                using var doc = JsonDocument.Parse(StripFences(rawReply));
                var root = doc.RootElement;
                if (root.TryGetProperty("reply", out var replyProp))
                    replyText = replyProp.GetString() ?? replyText;
                if (root.TryGetProperty("tools", out var toolsProp) && toolsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var t in toolsProp.EnumerateArray())
                    {
                        var rec = new ToolRecommendation();
                        if (t.TryGetProperty("tool", out var nameProp)) rec.ToolName = nameProp.GetString() ?? "";
                        if (t.TryGetProperty("why", out var whyProp)) rec.Why = whyProp.GetString() ?? "";
                        if (!string.IsNullOrEmpty(rec.ToolName)) recs.Add(rec);
                    }
                }
            }
            catch
            {
                // Model didn't return clean JSON this turn - show the raw text instead of crashing.
            }
            return replyText;
        }

        /// <summary>Ξαναζητά απάντηση για το ΤΕΛΕΥΤΑΙΟ μήνυμα χρήστη (χωρίς να προσθέσει νέο) - αφαιρεί πρώτα
        /// την προηγούμενη απάντηση του agent από το ιστορικό, ώστε το context να μείνει σωστό.</summary>
        private async void RegenerateButton_Click(object sender, RoutedEventArgs e)
        {
            if (_sending) return;
            if (_history.Count > 0 && _history[^1].Role == "assistant") _history.RemoveAt(_history.Count - 1);
            if (_history.Count == 0 || _history[^1].Role != "user") return;

            // ΔΙΟΡΘΩΣΗ: αφαιρούμε ΚΑΙ την ορατή φούσκα της παλιάς απάντησης - χωρίς αυτό η νέα προστίθεται
            // ΚΑΤΩ από την παλιά (φαινομενικά διπλή απάντηση) αντί να την αντικαθιστά.
            if (_lastAssistantRow != null)
            {
                ChatLogPanel.Children.Remove(_lastAssistantRow);
                _lastAssistantRow = null;
                _lastRegenerateButton = null;
            }

            await GetAssistantReplyAsync(null, restoreTextOnFail: null, isRegenerate: true);
        }

        private async Task GetAssistantReplyAsync(string? imagePath, string? restoreTextOnFail, bool isRegenerate = false)
        {
            // ΔΙΟΡΘΩΣΗ: το Cancel() ενός CancellationTokenSource δεν εγγυάται ότι το ήδη απεσταλμένο HTTP
            // request δεν θα προλάβει να απαντήσει - αν ο χρήστης άλλαξε συνομιλία (New Chat/Load) όσο αυτό
            // περίμενε, ελέγχουμε το epoch πριν αγγίξουμε _history/UI/_sendCts πιο κάτω ώστε μια καθυστερημένη
            // απάντηση να μη "μολύνει" τη νέα συνομιλία ή να χαλάσει το CancellationTokenSource της.
            var epoch = _conversationEpoch;
            _sending = true;
            _sendCts = new CancellationTokenSource();
            SendButton.Content = L.StopButton;

            TaskLabel.Text = L.Thinking;
            TaskProgress.IsIndeterminate = true;
            ShowTyping();

            try
            {
                ClaudeResult result;
                string? autoContinueAgent = null;

                if (_centralAgentName == "Claude")
                {
                    // Αυτόματη ΠΡΟΤΑΣΗ (όχι σιωπηλή εναλλαγή) άλλου, ήδη ρυθμισμένου agent όταν το Claude
                    // πλησιάζει/χτυπάει το όριό του (v4.6.0, ρητό αίτημα χρήστη - βλ. AskAutoContinue: πάντα
                    // pop-up με Ναι/Όχι, ποτέ σιωπηλή εναλλαγή χωρίς να ρωτηθεί ο χρήστης). Το
                    // ClaudeSessionBudget είναι δικό μας, δηλωτικό όριο εμφάνισης (όχι πραγματικό Anthropic
                    // quota) - το πραγματικό rate-limit της Anthropic ανιχνεύεται αντιδραστικά πιο κάτω (HTTP
                    // 429). "5ωρο rolling window" είναι έννοια του Claude.ai Pro/Max UI, όχι ενός API-key
                    // client όπως το Κόμβος - το πρακτικό αντίστοιχο εδώ είναι ακριβώς αυτό το 429.
                    bool nearOwnBudget = _autoContinueOnClaudeLimit && _claudeUsed >= ClaudeSessionBudget * 0.9;
                    var candidateFallback = nearOwnBudget ? ResolveFallbackAgent(askIfNeeded: true, alreadyFailed: false) : null;

                    if (candidateFallback != null)
                    {
                        result = await OpenAiCompatibleClient.SendAsync(
                            candidateFallback.BaseUrl!, candidateFallback.Model!, candidateFallback.ApiKey, BuildSystemPrompt(), BuildEffectiveHistory(), _sendCts.Token);
                        autoContinueAgent = candidateFallback.Name;
                    }
                    else
                    {
                        (byte[] Bytes, string MediaType)? image = null;
                        if (imagePath != null)
                        {
                            var ext = System.IO.Path.GetExtension(imagePath);
                            if (ImageMediaTypes.TryGetValue(ext, out var mediaType))
                                image = (System.IO.File.ReadAllBytes(imagePath), mediaType);
                        }
                        try
                        {
                            result = await _claude.SendAsync(BuildSystemPrompt(), BuildEffectiveHistory(), image, _thinkingEnabled, _sendCts.Token, _webSearchEnabled, ParseMcpServers());
                        }
                        catch (Exception ex) when (_autoContinueOnClaudeLimit && LooksLikeRateLimit(ex) && !isRegenerate)
                        {
                            var fallback = ResolveFallbackAgent(askIfNeeded: true, alreadyFailed: true);
                            if (fallback == null) throw;

                            result = await OpenAiCompatibleClient.SendAsync(
                                fallback.BaseUrl!, fallback.Model!, fallback.ApiKey, BuildSystemPrompt(), BuildEffectiveHistory(), _sendCts.Token);
                            autoContinueAgent = fallback.Name;
                        }
                    }
                }
                else
                {
                    var centralTool = Catalog.Tools.FirstOrDefault(t => t.Name == _centralAgentName);
                    if (centralTool?.OpenAiCompatBaseUrl != null)
                    {
                        var key = _apiKeys.TryGetValue(_centralAgentName, out var k) ? k : "";
                        result = await OpenAiCompatibleClient.SendAsync(
                            centralTool.OpenAiCompatBaseUrl, centralTool.OpenAiCompatModel!, key, BuildSystemPrompt(), BuildEffectiveHistory(), _sendCts.Token);
                    }
                    else
                    {
                        RemoveTyping();
                        var noIntegrationMsg = L.NoIntegrationReply(_centralAgentName);
                        var noIntegrationTurn = new ChatTurn { Role = "assistant", Content = noIntegrationMsg };
                        AddChatBubble(false, noIntegrationMsg, historyTurn: noIntegrationTurn);
                        _history.Add(noIntegrationTurn);
                        return;
                    }
                }
                RemoveTyping();
                if (epoch != _conversationEpoch) return;

                var replyText = ExtractReplyText(result.ReplyText, out var recs);
                var assistantTurn = new ChatTurn { Role = "assistant", Content = result.ReplyText };
                var bubbleAgentName = autoContinueAgent != null ? L.AutoContinuedAgentLabel(autoContinueAgent) : null;
                AddChatBubble(false, replyText, recs, thinkingText: result.ThinkingText, citations: result.Citations, agentName: bubbleAgentName, historyTurn: assistantTurn);
                _history.Add(assistantTurn);
                NotifyReplyIfInBackground(replyText);

                // Όταν η απάντηση ήρθε από fallback agent (auto-continue), τα tokens ανήκουν σε ΕΚΕΙΝΟΝ, όχι
                // στο δικό-μας UI budget του Claude - αλλιώς θα φαινόταν σαν το Claude να "έτρωγε" κατανάλωση
                // που ποτέ δεν έκανε.
                if (autoContinueAgent == null)
                {
                    _claudeUsed += result.InputTokens + result.OutputTokens;
                    _cacheReadTotal += result.CacheReadInputTokens;
                    _cacheCreationTotal += result.CacheCreationInputTokens;
                }
                else
                {
                    var row = _usageRows.FirstOrDefault(r => r.Name == autoContinueAgent);
                    if (row != null) { row.Used += result.InputTokens + result.OutputTokens; row.IsConnected = true; }
                }
                UpdateStatusBar();
                SaveCurrentConversation();
            }
            catch (OperationCanceledException)
            {
                RemoveTyping();
                if (epoch == _conversationEpoch)
                {
                    // ΔΙΟΡΘΩΣΗ: στο regenerate το τελευταίο "user" turn ΔΕΝ προστέθηκε τώρα - είναι το ίδιο,
                    // προϋπάρχον ερώτημα. Αφαίρεσή του θα έσβηνε μόνιμα πραγματικό ιστορικό της συζήτησης.
                    if (!isRegenerate && _history.Count > 0 && _history[^1].Role == "user")
                        _history.RemoveAt(_history.Count - 1);
                    if (restoreTextOnFail != null) ChatInputBox.Text = restoreTextOnFail;
                }
            }
            catch (Exception ex)
            {
                RemoveTyping();
                if (epoch == _conversationEpoch)
                {
                    AddChatBubble(false, L.SomethingWentWrong(ex.Message), showRegenerate: false);
                    if (!isRegenerate && _history.Count > 0 && _history[^1].Role == "user")
                        _history.RemoveAt(_history.Count - 1);
                    if (restoreTextOnFail != null) ChatInputBox.Text = restoreTextOnFail;
                }
            }
            finally
            {
                // Αν η συνομιλία άλλαξε όσο περιμέναμε, το _sendCts/_sending ανήκουν πλέον σε ΑΛΛΟ, ενεργό
                // αίτημα της νέας συνομιλίας - αγγίζοντάς τα εδώ θα το χαλούσαμε.
                if (epoch == _conversationEpoch)
                {
                    _sending = false;
                    _sendCts?.Dispose();
                    _sendCts = null;
                    SendButton.Content = L.SendButton;
                    TaskProgress.IsIndeterminate = false;
                    TaskProgress.Value = 0;
                    TaskLabel.Text = L.Ready;
                    ChatInputBox.Focus();
                }
            }
        }

        internal static string StripFences(string s) => s.Replace("```json", "").Replace("```", "").Trim();

        private string BuildSystemPrompt() => L.SystemPrompt(BuildCatalogJson(), _centralAgentName, ComposeInstructions());

        /// <summary>Kombos Anywhere (v4.5.0) - "quick ask" χωρίς καμία επίδραση στην κύρια συζήτηση (δεν
        /// αγγίζει _history/ChatLogPanel). Χρησιμοποιεί το ΙΔΙΟ BuildSystemPrompt() με την κανονική
        /// συνομιλία (ίδιος agent/κατάλογος/οδηγίες) ώστε να επωφελείται από το ήδη ενεργό prompt cache αντί
        /// να δημιουργεί ένα καινούριο, μη-cached system prompt (standing κανόνας ελάχιστου κόστους).</summary>
        internal async Task<string> QuickAskAsync(string prompt, CancellationToken ct)
        {
            var scratchHistory = new List<ChatTurn> { new ChatTurn { Role = "user", Content = prompt } };
            ClaudeResult result;
            if (_centralAgentName == "Claude")
            {
                result = await _claude.SendAsync(BuildSystemPrompt(), scratchHistory, null, false, ct);
            }
            else
            {
                var centralTool = Catalog.Tools.FirstOrDefault(t => t.Name == _centralAgentName);
                if (centralTool?.OpenAiCompatBaseUrl == null) return L.NoIntegrationReply(_centralAgentName);
                var key = _apiKeys.TryGetValue(_centralAgentName, out var k) ? k : "";
                result = await OpenAiCompatibleClient.SendAsync(centralTool.OpenAiCompatBaseUrl, centralTool.OpenAiCompatModel!, key, BuildSystemPrompt(), scratchHistory, ct);
            }
            return ExtractReplyText(result.ReplyText, out _);
        }

        /// <summary>Καταγράφει ένα Kombos Anywhere Q&amp;A σαν πραγματικά turns στην ΤΡΕΧΟΥΣΑ κύρια συζήτηση -
        /// το κουμπί "Προσθήκη στη συζήτηση" στο mini bar.</summary>
        internal void AppendQuickAskToConversation(string prompt, string reply)
        {
            var userTurn = new ChatTurn { Role = "user", Content = prompt };
            AddChatBubble(true, prompt, historyTurn: userTurn);
            _history.Add(userTurn);
            var assistantTurn = new ChatTurn { Role = "assistant", Content = reply };
            AddChatBubble(false, reply, historyTurn: assistantTurn, showRegenerate: false);
            _history.Add(assistantTurn);
            SaveCurrentConversation();
        }

        // Πάνω από αυτό το πλήθος turns, τα παλαιότερα (πέρα από τα τελευταία TrimKeepRecentTurns) στέλνονται
        // περικομμένα αντί για ολόκληρα - μεγάλο λεξιλόγιο κόστους σε πολύ μακριές συζητήσεις χωρίς να χαθεί
        // η ροή (ο τίτλος/η πρόθεση κάθε παλιού μηνύματος μένει ορατή στον agent). Το πραγματικό _history (και
        // ό,τι αποθηκεύεται/εμφανίζεται στον χρήστη) ΔΕΝ αγγίζεται - μόνο το αντίγραφο που φεύγει στο API.
        private const int TrimHistoryThreshold = 40;
        private const int TrimKeepRecentTurns = 24;
        private const int TrimOlderTurnMaxChars = 280;

        private List<ChatTurn> BuildEffectiveHistory()
        {
            if (_history.Count <= TrimHistoryThreshold) return _history;

            var older = _history.Take(_history.Count - TrimKeepRecentTurns);
            var recent = _history.Skip(_history.Count - TrimKeepRecentTurns);
            var trimmed = older.Select(t => t.Content.Length <= TrimOlderTurnMaxChars ? t
                : new ChatTurn { Role = t.Role, Content = t.Content[..TrimOlderTurnMaxChars] + L.TrimmedTurnSuffix });
            return trimmed.Concat(recent).ToList();
        }

        /// <summary>Ενώνει τις προσωπικές οδηγίες του χρήστη με τη συλλογική "Hive Memory" - έτσι ΚΑΘΕ agent
        /// (Claude και κάθε OpenAI-compatible εργαλείο) βλέπει τα ίδια μαθημένα γεγονότα, χωρίς να χρειάζεται
        /// ξεχωριστό μηχανισμό ανά provider.</summary>
        private string ComposeInstructions()
        {
            if (_hiveMemory.Count == 0) return _customInstructions;
            var hiveBlock = L.HiveMemoryPromptPrefix + string.Join(" ", _hiveMemory.Select(f => $"- {f}"));
            return string.IsNullOrWhiteSpace(_customInstructions) ? hiveBlock : hiveBlock + "\n\n" + _customInstructions;
        }

        private void RegisterBuiltInSlashCommands()
        {
            _pluginHost.RegisterSlashCommand("/remember", async arg =>
            {
                await Task.CompletedTask;
                if (string.IsNullOrWhiteSpace(arg)) return L.HiveMemoryRememberUsage;
                _hiveMemory = HiveMemoryStore.Add(_hiveMemory, arg);
                return L.HiveMemoryRemembered(arg.Trim());
            });
            _pluginHost.RegisterSlashCommand("/hive", async arg =>
            {
                await Task.CompletedTask;
                return _hiveMemory.Count == 0 ? L.HiveMemoryEmpty : string.Join("\n", _hiveMemory.Select(f => $"• {f}"));
            });
            _pluginHost.RegisterSlashCommand("/readfile", ReadProjectFileAsync);
            _pluginHost.RegisterSlashCommand("/search", SearchProjectAsync);
            _pluginHost.RegisterSlashCommand("/image", GenerateImageAsync);
            _pluginHost.RegisterSlashCommand("/verify", VerifyActiveProjectAsync);
        }

        /// <summary>Διαβάζει το _mcpServersRaw (μία γραμμή ανά server, "όνομα|url") σε λίστα - γραμμές χωρίς
        /// το διαχωριστικό "|" ή με κενό url αγνοούνται σιωπηλά αντί να ρίχνουν σφάλμα.</summary>
        private List<(string Name, string Url)> ParseMcpServers()
        {
            var result = new List<(string, string)>();
            foreach (var line in _mcpServersRaw.Split('\n'))
            {
                var trimmed = line.Trim();
                if (trimmed.Length == 0) continue;
                var parts = trimmed.Split('|', 2);
                if (parts.Length != 2) continue;
                var name = parts[0].Trim();
                var url = parts[1].Trim();
                if (name.Length == 0 || url.Length == 0) continue;
                result.Add((name, url));
            }
            return result;
        }

        internal void AddChatBubble(bool isUser, string text, List<ToolRecommendation>? tools = null, string? imagePath = null,
            string? thinkingText = null, List<(string Title, string Url)>? citations = null, string? agentName = null,
            bool showRegenerate = true, ChatTurn? historyTurn = null)
        {
            var row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 12),
                HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left
            };

            var avatar = new Border { Width = 24, Height = 24, CornerRadius = new CornerRadius(12), Margin = new Thickness(0, 0, 8, 0) };
            if (!isUser && agentName != null && agentName != "Claude")
            {
                var agentTool = Catalog.Tools.FirstOrDefault(t => t.Name == agentName);
                if (agentTool != null)
                {
                    var agentCat = Catalog.Categories.FirstOrDefault(c => c.Id == agentTool.CategoryId) ?? Catalog.Categories[0];
                    var logo = BuildLogo(agentTool, agentCat, 24);
                    logo.Margin = new Thickness(0, 0, 8, 0);
                    row.Children.Add(logo);
                }
                else row.Children.Add(avatar);
            }
            else
            {
                avatar.SetResourceReference(Border.BackgroundProperty, isUser ? "InkSoftBrush" : "ClaudeBrush");
                avatar.Child = new TextBlock
                {
                    Text = isUser ? (L.Current == Lang.EL ? "Ε" : "U") : "C",
                    Foreground = Brushes.White,
                    FontSize = 10,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                row.Children.Add(avatar);
            }

            var bubble = new Border { MaxWidth = 420, Padding = new Thickness(10, 8, 10, 8), CornerRadius = new CornerRadius(8), BorderThickness = new Thickness(1) };
            bubble.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
            bubble.SetResourceReference(Border.BackgroundProperty, isUser ? "ClaudeBrush" : "PaperBrush");

            if (text.Length > 0)
            {
                var contextMenu = new ContextMenu();
                var copyMdItem = new MenuItem { Header = L.CopyAsMarkdown };
                copyMdItem.Click += (s, e) => { try { Clipboard.SetText(text); } catch { /* clipboard busy - not critical */ } };
                var copyPlainItem = new MenuItem { Header = L.CopyAsPlainText };
                copyPlainItem.Click += (s, e) => { try { Clipboard.SetText(ToPlainText(text)); } catch { /* clipboard busy - not critical */ } };
                var quoteItem = new MenuItem { Header = L.QuoteInNewMessage };
                quoteItem.Click += (s, e) =>
                {
                    ChatInputBox.Text = "> " + text.Replace("\n", "\n> ") + "\n\n";
                    ChatInputBox.Focus();
                    ChatInputBox.CaretIndex = ChatInputBox.Text.Length;
                };
                contextMenu.Items.Add(copyMdItem);
                contextMenu.Items.Add(copyPlainItem);
                contextMenu.Items.Add(quoteItem);
                bubble.ContextMenu = contextMenu;
            }

            var bubbleStack = new StackPanel();

            if (!isUser && agentName != null)
            {
                var nameLabel = new TextBlock { Text = agentName, FontSize = 10.5, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 4) };
                nameLabel.SetResourceReference(TextBlock.ForegroundProperty, "ClaudeBrush");
                bubbleStack.Children.Add(nameLabel);
            }

            if (!string.IsNullOrWhiteSpace(thinkingText))
            {
                var thinkingExpander = new Expander
                {
                    Header = L.ThinkingHeader,
                    IsExpanded = false,
                    FontSize = 10.5,
                    Margin = new Thickness(0, 0, 0, 8)
                };
                thinkingExpander.SetResourceReference(Expander.ForegroundProperty, "InkSoftBrush");
                var thinkingBody = new TextBlock
                {
                    Text = thinkingText, TextWrapping = TextWrapping.Wrap, FontSize = 11.5,
                    FontStyle = FontStyles.Italic, Margin = new Thickness(0, 6, 0, 0)
                };
                thinkingBody.SetResourceReference(TextBlock.ForegroundProperty, "InkSoftBrush");
                thinkingExpander.Content = thinkingBody;
                bubbleStack.Children.Add(thinkingExpander);
            }

            if (imagePath != null)
            {
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.UriSource = new Uri(imagePath);
                    bmp.DecodePixelWidth = 260;
                    bmp.EndInit();
                    var img = new Image { Source = bmp, MaxWidth = 260, MaxHeight = 200, Stretch = Stretch.Uniform, Margin = new Thickness(0, 0, 0, text.Length > 0 ? 6 : 0) };
                    bubbleStack.Children.Add(img);
                }
                catch { /* η φούσκα δείχνει έστω το κείμενο αν η εικόνα δεν φορτώσει */ }
            }

            if (text.Length > 0)
            {
                var contentPanel = new StackPanel();
                RenderMarkdown(contentPanel, text, isUser);
                bubbleStack.Children.Add(contentPanel);
            }

            if (tools != null && tools.Count > 0)
            {
                var label = new TextBlock { Text = L.AgentsInTask, FontSize = 10.5, Margin = new Thickness(0, 8, 0, 4) };
                label.SetResourceReference(TextBlock.ForegroundProperty, "InkSoftBrush");
                bubbleStack.Children.Add(label);

                var wrap = new WrapPanel();
                foreach (var rec in tools)
                {
                    var tool = Catalog.Tools.FirstOrDefault(t => t.Name.Equals(rec.ToolName, StringComparison.OrdinalIgnoreCase));
                    if (tool == null) continue;

                    var chip = new Border { BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(12), Padding = new Thickness(6, 3, 8, 3), Margin = new Thickness(0, 0, 5, 5), Cursor = Cursors.Hand };
                    chip.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
                    chip.SetResourceReference(Border.BackgroundProperty, "PanelBrush");
                    chip.ToolTip = rec.Why;

                    var chipRow = new StackPanel { Orientation = Orientation.Horizontal };
                    var chipCat = Catalog.Categories.FirstOrDefault(c => c.Id == tool.CategoryId) ?? Catalog.Categories[0];
                    var chipLogo = BuildLogo(tool, chipCat, 14);
                    chipLogo.Margin = new Thickness(0, 0, 5, 0);
                    chipRow.Children.Add(chipLogo);
                    var chipText = new TextBlock { Text = tool.HasMcp ? tool.Name + "  MCP" : tool.Name, FontSize = 11, VerticalAlignment = VerticalAlignment.Center };
                    chipText.SetResourceReference(TextBlock.ForegroundProperty, "InkBrush");
                    chipRow.Children.Add(chipText);
                    chip.Child = chipRow;

                    chip.MouseLeftButtonUp += (s, e) => ShowDetails(tool);
                    wrap.Children.Add(chip);
                }
                bubbleStack.Children.Add(wrap);
            }

            if (citations != null && citations.Count > 0)
            {
                var citeLabel = new TextBlock { Text = L.SourcesLabel, FontSize = 10.5, Margin = new Thickness(0, 8, 0, 4) };
                citeLabel.SetResourceReference(TextBlock.ForegroundProperty, "InkSoftBrush");
                bubbleStack.Children.Add(citeLabel);

                var citeWrap = new WrapPanel();
                foreach (var (title, url) in citations)
                {
                    var chip = new Border
                    {
                        BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(12),
                        Padding = new Thickness(6, 3, 8, 3), Margin = new Thickness(0, 0, 5, 5), Cursor = Cursors.Hand,
                        ToolTip = url
                    };
                    chip.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
                    chip.SetResourceReference(Border.BackgroundProperty, "PanelBrush");

                    var domain = GetDomain(url);
                    var chipText = new TextBlock
                    {
                        Text = "🔗 " + (string.IsNullOrEmpty(domain) ? title : domain), FontSize = 11,
                        VerticalAlignment = VerticalAlignment.Center, MaxWidth = 220, TextTrimming = TextTrimming.CharacterEllipsis
                    };
                    chipText.SetResourceReference(TextBlock.ForegroundProperty, "InkBrush");
                    chip.Child = chipText;

                    chip.MouseLeftButtonUp += (s, e) => OpenUrl(url);
                    citeWrap.Children.Add(chip);
                }
                bubbleStack.Children.Add(citeWrap);
            }

            if (text.Length > 0)
            {
                var actionRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 6, 0, 0), HorizontalAlignment = HorizontalAlignment.Right };

                if (!isUser && showRegenerate)
                {
                    if (_lastRegenerateButton != null) _lastRegenerateButton.Visibility = Visibility.Collapsed;
                    var regenButton = new Button
                    {
                        Content = L.RegenerateButton, FontSize = 10, Padding = new Thickness(6, 2, 6, 2),
                        Margin = new Thickness(0, 0, 8, 0), Background = Brushes.Transparent,
                        BorderThickness = new Thickness(0), Cursor = Cursors.Hand, Opacity = 0.75
                    };
                    regenButton.SetResourceReference(Button.ForegroundProperty, "InkSoftBrush");
                    regenButton.Click += RegenerateButton_Click;
                    actionRow.Children.Add(regenButton);
                    _lastRegenerateButton = regenButton;
                    _lastAssistantRow = row;
                }

                if (isUser)
                {
                    var editButton = new Button
                    {
                        Content = L.EditButton, FontSize = 10, Padding = new Thickness(6, 2, 6, 2),
                        Margin = new Thickness(0, 0, 8, 0), Background = Brushes.Transparent, Foreground = Brushes.White,
                        BorderThickness = new Thickness(0), Cursor = Cursors.Hand, Opacity = 0.75
                    };
                    editButton.Click += (s, e) =>
                    {
                        ChatInputBox.Text = text;
                        ChatInputBox.Focus();
                        ChatInputBox.CaretIndex = text.Length;
                    };
                    actionRow.Children.Add(editButton);
                }

                var copyButton = new Button
                {
                    Content = L.CopyButton, FontSize = 10, Padding = new Thickness(6, 2, 6, 2),
                    Background = Brushes.Transparent, BorderThickness = new Thickness(0), Cursor = Cursors.Hand
                };
                copyButton.Foreground = isUser ? Brushes.White : (Brush)FindResource("InkSoftBrush");
                copyButton.Opacity = 0.75;
                copyButton.Click += (s, e) =>
                {
                    try { Clipboard.SetText(text); copyButton.Content = L.CopiedLabel; }
                    catch { /* clipboard busy/unavailable - not critical */ }
                };
                actionRow.Children.Add(copyButton);

                if (!isUser)
                {
                    var speakButton = new Button
                    {
                        Content = L.SpeakButton, FontSize = 10, Padding = new Thickness(6, 2, 6, 2),
                        Margin = new Thickness(8, 0, 0, 0), Background = Brushes.Transparent, BorderThickness = new Thickness(0),
                        Cursor = Cursors.Hand, Opacity = 0.75, ToolTip = L.SpeakButtonTooltip
                    };
                    speakButton.SetResourceReference(Button.ForegroundProperty, "InkSoftBrush");
                    speakButton.Click += (s, e) =>
                    {
                        if (!SpeechService.TrySpeak(text)) speakButton.Content = L.SpeakUnavailable;
                    };
                    actionRow.Children.Add(speakButton);
                }

                if (historyTurn != null)
                {
                    var forkButton = new Button
                    {
                        Content = L.ForkButton, FontSize = 10, Padding = new Thickness(6, 2, 6, 2),
                        Margin = new Thickness(8, 0, 0, 0), Background = Brushes.Transparent, BorderThickness = new Thickness(0),
                        Cursor = Cursors.Hand, Opacity = 0.75, ToolTip = L.ForkButtonTooltip
                    };
                    forkButton.Foreground = isUser ? Brushes.White : (Brush)FindResource("InkSoftBrush");
                    forkButton.Click += (s, e) => ForkConversationFrom(historyTurn);
                    actionRow.Children.Add(forkButton);

                    var deleteMsgButton = new Button
                    {
                        Content = L.DeleteMessageButton, FontSize = 10, Padding = new Thickness(6, 2, 6, 2),
                        Margin = new Thickness(8, 0, 0, 0), Background = Brushes.Transparent, BorderThickness = new Thickness(0),
                        Cursor = Cursors.Hand, Opacity = 0.75
                    };
                    deleteMsgButton.Foreground = isUser ? Brushes.White : (Brush)FindResource("InkSoftBrush");
                    deleteMsgButton.Click += (s, e) =>
                    {
                        // _history.Remove() δουλεύει με ΑΝΑΦΟΡΑ στο συγκεκριμένο ChatTurn, όχι index - ασφαλές
                        // ό,τι κι αν έχει διαγραφεί/προστεθεί στο μεταξύ αλλού στη συζήτηση. Αν το ChatTurn δεν
                        // υπάρχει καν στο _history (π.χ. φούσκα εντολής plugin), το Remove() είναι ασφαλές no-op.
                        _history.Remove(historyTurn);
                        ChatLogPanel.Children.Remove(row);
                        if (ReferenceEquals(row, _lastAssistantRow)) { _lastAssistantRow = null; _lastRegenerateButton = null; }
                        SaveCurrentConversation();
                    };
                    actionRow.Children.Add(deleteMsgButton);
                }

                bubbleStack.Children.Add(actionRow);
            }

            bubble.Child = bubbleStack;
            row.Children.Add(bubble);
            ChatLogPanel.Children.Add(row);
            ChatScroll.ScrollToEnd();
        }

        /// <summary>Εμφανίζει το κουμπί "μετάβαση στο τέλος" μόνο όταν ο χρήστης έχει σκρολάρει αρκετά ψηλά -
        /// αλλιώς παραμένει κρυφό όσο βρίσκεται ήδη κοντά στο τέλος της συζήτησης.</summary>
        private void ChatScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            const double nearBottomThreshold = 60;
            bool nearBottom = ChatScroll.ScrollableHeight - ChatScroll.VerticalOffset <= nearBottomThreshold;
            ScrollToBottomButton.Visibility = nearBottom ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ScrollToBottomButton_Click(object sender, RoutedEventArgs e) => ChatScroll.ScrollToEnd();

        // ---------- Ελαφρύ markdown rendering (bold/italic/inline code/fenced code blocks) ----------
        private static readonly Regex CodeFenceRegex = new(@"```[a-zA-Z0-9_+\-]*\r?\n([\s\S]*?)```", RegexOptions.Compiled);
        private static readonly Regex InlineMdRegex = new(@"(\*\*[^*\n]+?\*\*|`[^`\n]+`|\*[^*\n]+?\*)", RegexOptions.Compiled);

        private double ChatFontScale => _chatTextScale switch { ChatTextScale.Small => 0.85, ChatTextScale.Large => 1.15, _ => 1.0 };

        internal void RenderMarkdown(StackPanel container, string text, bool isUser)
        {
            int last = 0;
            var matches = CodeFenceRegex.Matches(text);
            foreach (Match m in matches)
            {
                if (m.Index > last)
                {
                    var prose = text.Substring(last, m.Index - last).Trim('\n');
                    if (prose.Trim().Length > 0) container.Children.Add(BuildProseBlock(prose, isUser));
                }
                container.Children.Add(BuildCodeBlock(m.Groups[1].Value.TrimEnd('\n', '\r')));
                last = m.Index + m.Length;
            }
            if (last < text.Length)
            {
                var rest = text.Substring(last).Trim('\n');
                if (rest.Trim().Length > 0) container.Children.Add(BuildProseBlock(rest, isUser));
            }
            if (container.Children.Count == 0)
                container.Children.Add(BuildProseBlock(text, isUser));
        }

        private TextBlock BuildProseBlock(string text, bool isUser)
        {
            var scale = ChatFontScale;
            var tb = new TextBlock { TextWrapping = TextWrapping.Wrap, FontSize = 13 * scale };
            if (isUser) tb.Foreground = Brushes.White;
            else tb.SetResourceReference(TextBlock.ForegroundProperty, "InkBrush");

            int pos = 0;
            foreach (Match m in InlineMdRegex.Matches(text))
            {
                if (m.Index > pos) tb.Inlines.Add(new Run(text.Substring(pos, m.Index - pos)));
                var token = m.Value;
                if (token.StartsWith("**"))
                {
                    tb.Inlines.Add(new Run(token[2..^2]) { FontWeight = FontWeights.Bold });
                }
                else if (token.StartsWith("`"))
                {
                    var codeRun = new Run(token[1..^1]) { FontFamily = new FontFamily("Consolas"), FontSize = 12 * scale };
                    codeRun.SetResourceReference(TextElement.BackgroundProperty, "PanelBrush");
                    tb.Inlines.Add(codeRun);
                }
                else
                {
                    tb.Inlines.Add(new Run(token[1..^1]) { FontStyle = FontStyles.Italic });
                }
                pos = m.Index + m.Length;
            }
            if (pos < text.Length) tb.Inlines.Add(new Run(text.Substring(pos)));
            return tb;
        }

        private Border BuildCodeBlock(string code)
        {
            var border = new Border { CornerRadius = new CornerRadius(5), Margin = new Thickness(0, 4, 0, 4), Padding = new Thickness(8) };
            border.SetResourceReference(Border.BackgroundProperty, "SidebarBrush");

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var copyCodeButton = new Button
            {
                Content = L.CopyButton, FontSize = 9.5, Padding = new Thickness(5, 1, 5, 1),
                Background = Brushes.Transparent, BorderThickness = new Thickness(0), Foreground = Brushes.White,
                Cursor = Cursors.Hand, Opacity = 0.8, HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 0, 4)
            };
            copyCodeButton.Click += (s, e) =>
            {
                try { Clipboard.SetText(code); copyCodeButton.Content = L.CopiedLabel; }
                catch { /* clipboard busy/unavailable - not critical */ }
            };
            Grid.SetRow(copyCodeButton, 0);

            var codeScroll = new ScrollViewer { HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Disabled };
            var codeText = new TextBlock { Text = code, FontFamily = new FontFamily("Consolas"), FontSize = 12 * ChatFontScale, Foreground = Brushes.White, TextWrapping = TextWrapping.NoWrap };
            codeScroll.Content = codeText;
            Grid.SetRow(codeScroll, 1);

            grid.Children.Add(copyCodeButton);
            grid.Children.Add(codeScroll);
            border.Child = grid;
            return border;
        }

        private void ShowTyping()
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };
            var avatar = new Border { Width = 24, Height = 24, CornerRadius = new CornerRadius(12), Margin = new Thickness(0, 0, 8, 0) };
            avatar.SetResourceReference(Border.BackgroundProperty, "ClaudeBrush");
            avatar.Child = new TextBlock { Text = "C", Foreground = Brushes.White, FontSize = 10, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

            var bubble = new Border { Padding = new Thickness(10, 8, 10, 8), CornerRadius = new CornerRadius(8), BorderThickness = new Thickness(1) };
            bubble.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
            bubble.SetResourceReference(Border.BackgroundProperty, "PaperBrush");
            var text = new TextBlock { Text = L.ThinkingBubble, FontStyle = FontStyles.Italic };
            text.SetResourceReference(TextBlock.ForegroundProperty, "InkSoftBrush");
            bubble.Child = text;

            row.Children.Add(avatar);
            row.Children.Add(bubble);
            ChatLogPanel.Children.Add(row);
            _typingRow = row;
            ChatScroll.ScrollToEnd();
        }

        private void RemoveTyping()
        {
            if (_typingRow != null)
            {
                ChatLogPanel.Children.Remove(_typingRow);
                _typingRow = null;
            }
        }

        // ---------- Status bar / usage ----------
        private void BuildUsageRows()
        {
            _usageRows.Clear();
            _usageRows.Add(new UsageRow { Name = "Claude", IsConnected = true });
            foreach (var tool in Catalog.Tools)
                _usageRows.Add(new UsageRow { Name = tool.Name, IsConnected = false });
        }

        /// <summary>Ενημερώνει το IsConnected κάθε γραμμής από τα πραγματικά αποθηκευμένα API keys - "συνδεδεμένο"
        /// σημαίνει "έχει κλειδί", όχι πραγματικό δικτυακό ping (μηδενικό επιπλέον κόστος tokens/κλήσεων). Καλείται
        /// στην εκκίνηση και κάθε φορά που αποθηκεύεται/διαγράφεται κλειδί, ώστε η γραμμή κατάστασης και το
        /// παράθυρο Λογαριασμοί να δείχνουν πάντα την ίδια, ενημερωμένη κατάσταση.</summary>
        internal void SyncUsageConnectionStatus()
        {
            foreach (var row in _usageRows)
            {
                if (row.Name == "Claude") continue;
                row.IsConnected = _apiKeys.TryGetValue(row.Name, out var k) && !string.IsNullOrWhiteSpace(k);
            }
        }

        internal void UpdateStatusBar()
        {
            _usageRows[0].Used = _claudeUsed;
            long combined = _usageRows.Sum(r => r.Used);
            ClaudeTokenText.Text = L.ClaudeTokens(_claudeUsed, ClaudeSessionBudget);
            CombinedTokenText.Text = L.CombinedTokens(combined);

            long cacheTotal = _cacheReadTotal + _cacheCreationTotal;
            if (cacheTotal > 0)
            {
                var hitRate = (int)Math.Round(_cacheReadTotal * 100.0 / cacheTotal);
                CacheHitText.Text = L.CacheHitIndicator(hitRate);
                CacheHitText.Visibility = Visibility.Visible;
            }
            else
            {
                CacheHitText.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>Τοπικό analytics dashboard (v4.5.0) - καταγράφει ΜΙΑ εγγραφή ανά πραγματικό κλείσιμο της
        /// εφαρμογής (όχι κάθε "απόκρυψη στο tray") με τα σωρευτικά tokens της συνεδρίας, μόνο αν
        /// χρησιμοποιήθηκε έστω ένα εργαλείο - βλ. Services/UsageLogStore.cs.</summary>
        private void LogUsageSessionIfAny()
        {
            var used = _usageRows.Where(r => r.Used > 0).ToDictionary(r => r.Name, r => r.Used);
            if (used.Count == 0) return;
            UsageLogStore.Append(new UsageLogEntry { TimestampUtc = DateTime.UtcNow.ToString("O"), PerToolTokens = used });
        }

        /// <summary>"Κόμβος Gateway" (v4.6.0) - ξεκινά/σταματά την τοπική HTTP πύλη ανάλογα με το
        /// _gatewayEnabled. Καλείται στην εκκίνηση και κάθε φορά που ο χρήστης αλλάζει τη ρύθμιση/θύρα.</summary>
        internal void ApplyGatewaySetting()
        {
            _gatewayServer ??= new GatewayServer(GetGatewayPriorityAgents, OnGatewayRequestLogged);
            _gatewayServer.Stop();
            _gatewayStartError = null;

            if (!_gatewayEnabled) return;

            if (!_gatewayServer.TryStart(_gatewayPort, out var error))
                _gatewayStartError = error;
        }

        private void OnGatewayRequestLogged(GatewayRequestLogEntry entry)
        {
            Dispatcher.Invoke(() =>
            {
                _gatewayLog.Insert(0, entry);
                if (_gatewayLog.Count > 50) _gatewayLog.RemoveAt(_gatewayLog.Count - 1);
            });
        }

        /// <summary>Σειρά προτεραιότητας δρομολόγησης για το Gateway: πρώτα ο κεντρικός agent (αν έχει
        /// κλειδί), μετά όλοι οι υπόλοιποι με αποθηκευμένο κλειδί, με τη σειρά του καταλόγου - ίδιο πνεύμα με
        /// το automatic-fallback-across-providers του OmniRoute, χωρίς όμως να χρειάζεται δικιά του ρύθμιση:
        /// χρησιμοποιεί ό,τι κλειδιά έχει ήδη βάλει ο χρήστης στο Κόμβος για τη δική του χρήση.</summary>
        internal List<GatewayAgentEntry> GetGatewayPriorityAgents()
        {
            var ordered = new List<string> { _centralAgentName };
            ordered.AddRange(Catalog.Tools.Select(t => t.Name).Where(n => n != _centralAgentName));
            if (!ordered.Contains("Claude")) ordered.Insert(0, "Claude");

            var result = new List<GatewayAgentEntry>();
            foreach (var name in ordered)
            {
                if (!_apiKeys.TryGetValue(name, out var key) || string.IsNullOrWhiteSpace(key)) continue;
                if (name == "Claude") { result.Add(new GatewayAgentEntry(name, key, null, null)); continue; }

                var tool = Catalog.Tools.FirstOrDefault(t => t.Name == name);
                if (tool?.OpenAiCompatBaseUrl == null) continue; // δεν έχει πραγματικό chat-completions endpoint
                result.Add(new GatewayAgentEntry(name, key, tool.OpenAiCompatBaseUrl, tool.OpenAiCompatModel));
            }
            return result;
        }

        /// <summary>Το Anthropic API επιστρέφει HTTP 429 τόσο για πραγματικό rate-limit όσο και για
        /// εξάντληση quota - ο ClaudeClient ενσωματώνει τον κωδικό στο μήνυμα ως "(429)" (βλ.
        /// ClaudeClient.SendAsync). Δεν υπάρχει πραγματικό "5ωρο rolling window" σε ένα API-key client (αυτό
        /// είναι έννοια του Claude.ai Pro/Max UI, όχι του Messages API) - το πρακτικό αντίστοιχο εδώ είναι
        /// ακριβώς αυτό το 429.</summary>
        internal static bool LooksLikeRateLimit(Exception ex) => ex.Message.Contains("(429)");

        /// <summary>Ποιος agent θα συνεχίσει τη συζήτηση όταν το Claude πλησιάζει/χτυπάει το όριό του (v4.6.0,
        /// ρητό αίτημα χρήστη): ΤΗΝ ΠΡΩΤΗ ΦΟΡΑ ρωτάει με pop-up Ναι/Όχι ποιον agent να χρησιμοποιήσει (τον
        /// πρώτο διαθέσιμο στη σειρά προτεραιότητας του Gateway - βλ. GetGatewayPriorityAgents) και τον
        /// θυμάται (_rememberedFallbackAgent, αποθηκεύεται). Από εκεί και πέρα τον ξαναχρησιμοποιεί ΑΥΤΟΜΑΤΑ,
        /// χωρίς να ξαναρωτήσει, ώστε η συζήτηση/project να συνεχίζει απρόσκοπτα - εκτός αν το θυμημένο agent
        /// δεν έχει πια έγκυρο κλειδί, οπότε ρωτάει ξανά σαν να ήταν η πρώτη φορά.</summary>
        private GatewayAgentEntry? ResolveFallbackAgent(bool askIfNeeded, bool alreadyFailed)
        {
            if (!string.IsNullOrEmpty(_rememberedFallbackAgent))
            {
                var remembered = GetGatewayPriorityAgents().FirstOrDefault(a => a.Name == _rememberedFallbackAgent);
                if (remembered != null) return remembered;
            }

            if (!askIfNeeded) return null;

            var proposed = GetGatewayPriorityAgents().FirstOrDefault(a => a.Name != "Claude" && a.BaseUrl != null);
            if (proposed == null) return null;

            var message = alreadyFailed ? L.ClaudeLimitReachedAsk(proposed.Name) : L.ClaudeLimitNearAsk(proposed.Name);
            if (MessageBox.Show(this, message, L.WindowTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return null;

            _rememberedFallbackAgent = proposed.Name;
            SaveSettings();
            return proposed;
        }

        private AnalyticsWindow? _analyticsWindow;

        internal void OpenAnalyticsWindow()
        {
            if (_analyticsWindow != null) { _analyticsWindow.Activate(); return; }
            _analyticsWindow = new AnalyticsWindow(this);
            _analyticsWindow.Closed += (s, e) => _analyticsWindow = null;
            _analyticsWindow.Show();
        }

        private void AnalyticsButton_Click(object sender, RoutedEventArgs e) => OpenAnalyticsWindow();

        private void BreakdownButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshBreakdownPanel();
            BreakdownPopup.PlacementTarget = BreakdownButton;
            BreakdownPopup.IsOpen = !BreakdownPopup.IsOpen;
        }

        internal void RefreshBreakdownPanel()
        {
            BreakdownPanel.Children.Clear();
            foreach (var row in _usageRows)
            {
                var line = new TextBlock { FontSize = 12, Margin = new Thickness(0, 0, 0, 4) };
                line.SetResourceReference(TextBlock.ForegroundProperty, "InkBrush");
                var limitPart = row.Limit.HasValue ? $" / {row.Limit.Value:N0}" : "";
                line.Text = row.IsConnected
                    ? $"{row.Name}: {row.Used:N0}{limitPart} {row.Unit}"
                    : $"{row.Name}: {L.NotConnected}";
                BreakdownPanel.Children.Add(line);
            }
        }
    }
}
