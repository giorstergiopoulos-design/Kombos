using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Kombos.Data;
using Kombos.Localization;
using Kombos.Models;
using Kombos.Services;

namespace Kombos
{
    /// <summary>Ρυθμίσεις εμφάνισης/agent/project σε πραγματικό δευτερεύον Window (όχι Popup) — δεν κλείνει
    /// μόνο του όταν το κύριο παράθυρο χάνει το focus, ίδια λογική με το Show-AppearanceSettingsWindow
    /// του WinOptimizer (καρτέλες pill-tabs, ζωντανή εφαρμογή αλλαγών χωρίς επανεκκίνηση).</summary>
    public partial class SettingsWindow : Window
    {
        private readonly MainWindow _owner;

        public SettingsWindow(MainWindow owner, string initialTab = "project")
        {
            InitializeComponent();
            _owner = owner;
            Owner = owner;

            ApplyTexts();
            SyncFromOwnerState();
            ApplySkinLayout();
            SkinChrome.ApplySquareIfClassic(_owner._skin, ShellBorder, SettingsHeader);
            SelectTab(initialTab);
        }

        // ---------- Header drag / close ----------
        private void SettingsHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
        private void CloseSettingsButton_Click(object sender, RoutedEventArgs e) => Close();

        /// <summary>Το skin "Nexus" εμφανίζει τις καρτέλες σε κάθετη ράγα στα αριστερά (στυλ classic Windows
        /// Properties dialog) αντί για την οριζόντια σειρά pill-tabs - η ΜΟΝΗ οπτική διαφορά διάταξης ανάμεσα
        /// στα skins σε αυτό το παράθυρο, ζητήθηκε ρητά ως "διαφορετική διάταξη" για το δικό του, πρωτότυπο skin.</summary>
        private void ApplySkinLayout()
        {
            bool vertical = _owner._skin == WindowsSkin.Nexus;
            VerticalRailColumn.Width = new GridLength(vertical ? 138 : 0);
            if (vertical && Width < 440) Width = 440; // αρκετός χώρος ώστε η κάθετη ράγα να μην κόβει τις ετικέτες
            VerticalTabRail.Visibility = vertical ? Visibility.Visible : Visibility.Collapsed;
            HorizontalTabStrip.Visibility = vertical ? Visibility.Collapsed : Visibility.Visible;
        }

        // ---------- Tab strip οριζόντια κύλιση (βελάκια) - τα tabs ΔΕΝ τυλίγονται ποτέ σε 2η γραμμή. ----------
        private void TabStripLeftArrow_Click(object sender, RoutedEventArgs e) =>
            TabStripScrollViewer.ScrollToHorizontalOffset(Math.Max(0, TabStripScrollViewer.HorizontalOffset - 90));
        private void TabStripRightArrow_Click(object sender, RoutedEventArgs e) =>
            TabStripScrollViewer.ScrollToHorizontalOffset(TabStripScrollViewer.HorizontalOffset + 90);

        // ---------- Tabs ----------
        private void TabProject_Click(object sender, MouseButtonEventArgs e) => SelectTab("project");
        private void TabBehavior_Click(object sender, MouseButtonEventArgs e) => SelectTab("behavior");
        private void TabPlugins_Click(object sender, MouseButtonEventArgs e) => SelectTab("plugins");
        private void TabAbout_Click(object sender, MouseButtonEventArgs e) => SelectTab("about");

        internal void SelectTab(string tab)
        {
            if (tab == "agent") tab = "project"; // η καρτέλα Agent μετακόμισε σε δικό της παράθυρο (κουμπί "C")
            PanelProject.Visibility = tab == "project" ? Visibility.Visible : Visibility.Collapsed;
            PanelBehavior.Visibility = tab == "behavior" ? Visibility.Visible : Visibility.Collapsed;
            PanelPlugins.Visibility = tab == "plugins" ? Visibility.Visible : Visibility.Collapsed;
            PanelAbout.Visibility = tab == "about" ? Visibility.Visible : Visibility.Collapsed;

            // Στυλάρει ΚΑΙ το οριζόντιο pill ΚΑΙ το αντίστοιχο κάθετο (μόνο ένα από τα δύο σετ είναι ορατό
            // κάθε φορά, ανάλογα με το skin, αλλά κρατάμε και τα δύο συγχρονισμένα ώστε μια αλλαγή skin εν
            // ώρα λειτουργίας να μη δείχνει το λάθος tab επιλεγμένο.
            void Style(Border pill, TextBlock label, bool active, double radius)
            {
                pill.CornerRadius = new CornerRadius(radius);
                if (active)
                {
                    pill.SetResourceReference(Border.BackgroundProperty, "ClaudeBrush");
                    pill.BorderThickness = new Thickness(0);
                    label.Foreground = Brushes.White;
                }
                else
                {
                    pill.Background = Brushes.Transparent;
                    pill.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
                    pill.BorderThickness = new Thickness(1);
                    label.SetResourceReference(TextBlock.ForegroundProperty, "InkBrush");
                }
            }
            Style(TabProjectPill, TabProjectLabel, tab == "project", 13);
            Style(TabBehaviorPill, TabBehaviorLabel, tab == "behavior", 13);
            Style(TabPluginsPill, TabPluginsLabel, tab == "plugins", 13);
            Style(TabAboutPill, TabAboutLabel, tab == "about", 13);
            Style(TabProjectPillV, TabProjectLabelV, tab == "project", 6);
            Style(TabBehaviorPillV, TabBehaviorLabelV, tab == "behavior", 6);
            Style(TabPluginsPillV, TabPluginsLabelV, tab == "plugins", 6);
            Style(TabAboutPillV, TabAboutLabelV, tab == "about", 6);
            if (tab == "plugins") { BuildPluginsList(); BuildGalleryList(); }
        }

        // ---------- Localized static texts ----------
        internal void ApplyTexts()
        {
            SettingsTitleText.Text = L.SettingsTitle;
            TabProjectLabel.Text = TabProjectLabelV.Text = L.SectionProject;
            TabBehaviorLabel.Text = TabBehaviorLabelV.Text = L.SectionBehavior;
            TabPluginsLabel.Text = TabPluginsLabelV.Text = L.SectionPlugins;
            TabAboutLabel.Text = TabAboutLabelV.Text = L.SectionAbout;

            ImportProjectButton.Content = L.ImportProjectButton;
            ImportFolderButton.Content = L.ImportFolderButton;
            ImportProjectHintText.Text = L.ImportProjectHint;
            ProjectMemoryLabelText.Text = L.ProjectMemoryLabel;
            ProjectMemoryHintText.Text = L.ProjectMemoryHint;
            ImportProjectIndexButton.Content = L.ImportProjectIndexButton;
            VerifyProjectButton.Content = L.VerifyProjectButton;
            RefreshActiveProjectDisplay();
            ExportChatButton.Content = L.ExportChatButton;
            ExportBackupButton.Content = L.ExportBackupButton;
            ImportBackupButton.Content = L.ImportBackupButton;
            ImportChatExportButton.Content = L.ImportChatExportButton;
            ImportChatExportHintText.Text = L.ImportChatExportHint;
            ClearAllConversationsButton.Content = L.ClearAllConversationsButton;
            SyncFolderLabelText.Text = L.SyncFolderLabel;
            SyncFolderHintText.Text = L.SyncFolderHint;
            RefreshSyncFolderDisplay();
            SharedProjectsLabelText.Text = L.SharedProjectsLabel;
            SharedProjectsHintText.Text = L.SharedProjectsHint;
            PublishActiveProjectButton.Content = L.PublishActiveProjectButton;
            BuildSharedProjectsList();

            AlwaysOnTopCheckBox.Content = L.AlwaysOnTopLabel;
            StartWithWindowsCheckBox.Content = L.StartWithWindowsLabel;
            StartMinimizedCheckBox.Content = L.StartMinimizedLabel;
            StartMinimizedHintText.Text = L.StartMinimizedHint;
            SoundOnReplyCheckBox.Content = L.SoundOnReplyLabel;
            HotkeyInfoText.Text = L.HotkeyInfo;
            PluginsFolderHintText.Text = L.PluginsFolderHint;
            OpenPluginsFolderButton.Content = L.OpenPluginsFolderButton;
            PluginsRestartHintText.Text = L.PluginsRestartHint;

            HelpExpander.Header = L.HelpHeader;
            HelpBodyText.Text = L.HelpText;
            ReplayOnboardingButton.Content = L.ReplayOnboardingButton;
            UpdatesExpander.Header = L.UpdatesHeader;
            UpdatesHintText.Text = L.UpdatesHint;
            UpdateRepoBox.Text = _owner._updateCheckRepo;
            CheckUpdateButton.Content = L.CheckUpdateButton;
            UpdateResultText.Text = "";
            HistoryExpander.Header = L.HistoryHeader;
            HistoryBodyText.Text = L.VersionHistoryText;
            DisclaimerExpander.Header = L.DisclaimerHeader;
            DisclaimerBodyText.Text = L.DisclaimerText;
        }

        private void SyncFromOwnerState()
        {
            AlwaysOnTopCheckBox.Checked -= AlwaysOnTopCheckBox_Changed;
            AlwaysOnTopCheckBox.Unchecked -= AlwaysOnTopCheckBox_Changed;
            AlwaysOnTopCheckBox.IsChecked = _owner._alwaysOnTop;
            AlwaysOnTopCheckBox.Checked += AlwaysOnTopCheckBox_Changed;
            AlwaysOnTopCheckBox.Unchecked += AlwaysOnTopCheckBox_Changed;

            StartWithWindowsCheckBox.Checked -= StartWithWindowsCheckBox_Changed;
            StartWithWindowsCheckBox.Unchecked -= StartWithWindowsCheckBox_Changed;
            StartWithWindowsCheckBox.IsChecked = _owner._startWithWindows;
            StartWithWindowsCheckBox.Checked += StartWithWindowsCheckBox_Changed;
            StartWithWindowsCheckBox.Unchecked += StartWithWindowsCheckBox_Changed;

            StartMinimizedCheckBox.Checked -= StartMinimizedCheckBox_Changed;
            StartMinimizedCheckBox.Unchecked -= StartMinimizedCheckBox_Changed;
            StartMinimizedCheckBox.IsChecked = _owner._startMinimized;
            StartMinimizedCheckBox.Checked += StartMinimizedCheckBox_Changed;
            StartMinimizedCheckBox.Unchecked += StartMinimizedCheckBox_Changed;

            SoundOnReplyCheckBox.Checked -= SoundOnReplyCheckBox_Changed;
            SoundOnReplyCheckBox.Unchecked -= SoundOnReplyCheckBox_Changed;
            SoundOnReplyCheckBox.IsChecked = _owner._soundOnReply;
            SoundOnReplyCheckBox.Checked += SoundOnReplyCheckBox_Changed;
            SoundOnReplyCheckBox.Unchecked += SoundOnReplyCheckBox_Changed;
        }

        private void SoundOnReplyCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            _owner._soundOnReply = SoundOnReplyCheckBox.IsChecked == true;
            _owner.SaveSettings();
        }

        private void AlwaysOnTopCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            _owner._alwaysOnTop = AlwaysOnTopCheckBox.IsChecked == true;
            _owner.Topmost = _owner._alwaysOnTop;
            _owner.SaveSettings();
        }

        private void StartWithWindowsCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            _owner._startWithWindows = StartWithWindowsCheckBox.IsChecked == true;
            MainWindow.SetStartWithWindows(_owner._startWithWindows);
            _owner.SaveSettings();
        }

        private void StartMinimizedCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            _owner._startMinimized = StartMinimizedCheckBox.IsChecked == true;
            _owner.SaveSettings();
        }

        // ---------- Project import / chat export ----------
        private void ImportProjectButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Text/Markdown/JSON (*.txt;*.md;*.json)|*.txt;*.md;*.json|Όλα τα αρχεία (*.*)|*.*"
            };
            if (dialog.ShowDialog(this) != true) return;
            if (!_owner.ImportFileAsContext(dialog.FileName, out var error)) MessageBox.Show(this, error, L.LinkOpenError);
            else Close();
        }

        private void ImportFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();
            if (dialog.ShowDialog(this) != true) return;
            if (!_owner.ImportFolderAsContext(dialog.FolderName, out var error))
            {
                if (error != null) MessageBox.Show(this, error, L.LinkOpenError);
                else MessageBox.Show(this, L.ImportFolderEmpty);
            }
            else Close();
        }

        private void ImportProjectIndexButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();
            if (dialog.ShowDialog(this) != true) return;
            if (!_owner.ImportProjectIndexAsContext(dialog.FolderName, out var error))
            {
                if (error != null) MessageBox.Show(this, error, L.LinkOpenError);
                else MessageBox.Show(this, L.ImportFolderEmpty);
            }
            else { RefreshActiveProjectDisplay(); Close(); }
        }

        private void RefreshActiveProjectDisplay()
        {
            ActiveProjectStatusText.Text = string.IsNullOrEmpty(_owner._activeProjectPath)
                ? L.NoActiveProject
                : L.ActiveProjectStatus(_owner._activeProjectPath);
        }

        private async void VerifyProjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_owner._activeProjectPath)) { MessageBox.Show(this, L.NoActiveProject); return; }
            VerifyProjectButton.IsEnabled = false;
            try
            {
                var result = await Services.ProjectVerifier.VerifyAsync(_owner._activeProjectPath);
                var status = !result.Ran ? L.VerifyCouldNotRun(result.Output)
                    : result.Success ? L.VerifySuccess(result.Command) : L.VerifyFailed(result.Command);
                var body = result.Ran && !string.IsNullOrWhiteSpace(result.Output) ? $"{status}\n\n{result.Output}" : status;
                MessageBox.Show(this, body, L.SectionProject);
            }
            finally { VerifyProjectButton.IsEnabled = true; }
        }

        private void ExportChatButton_Click(object sender, RoutedEventArgs e)
        {
            if (_owner._history.Count == 0)
            {
                MessageBox.Show(this, L.ExportChatEmpty);
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Markdown (*.md)|*.md|Text (*.txt)|*.txt",
                FileName = $"kombos-chat-{DateTime.Now:yyyy-MM-dd-HHmm}.md"
            };
            if (dialog.ShowDialog(this) != true) return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"# {L.HeaderTitle} — {DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine();
            foreach (var (role, content) in _owner._history)
            {
                sb.AppendLine(role == "user" ? "**User:**" : "**Claude:**");
                sb.AppendLine();
                sb.AppendLine(content);
                sb.AppendLine();
            }

            try { System.IO.File.WriteAllText(dialog.FileName, sb.ToString()); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, L.LinkOpenError); return; }

            Close();
        }

        private void ClearAllConversationsButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(this, L.ClearAllConfirmMessage, L.ClearAllConfirmTitle,
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (result != MessageBoxResult.Yes) return;
            _owner.ClearAllConversations();
        }

        private void BuildPluginsList()
        {
            PluginsListPanel.Children.Clear();

            var folders = new List<string>();
            try
            {
                if (Directory.Exists(PluginLoader.PluginsRoot))
                    folders = Directory.GetDirectories(PluginLoader.PluginsRoot)
                        .Select(Path.GetFileName).Where(n => !string.IsNullOrEmpty(n)).Select(n => n!).ToList();
            }
            catch { /* π.χ. δεν υπάρχει ακόμα ο φάκελος - φυσιολογικό, απλά κενή λίστα */ }

            if (folders.Count == 0)
            {
                PluginsListPanel.Children.Add(new TextBlock
                {
                    Text = L.PluginsEmpty, FontSize = 11, TextWrapping = TextWrapping.Wrap,
                    Foreground = (Brush)FindResource("InkSoftBrush")
                });
                return;
            }

            foreach (var folder in folders)
            {
                var loaded = _owner._loadedPlugins.FirstOrDefault(p => p.FolderName == folder);
                var chk = new CheckBox
                {
                    Content = loaded != null ? $"{loaded.Name} (v{loaded.Version})" : $"{folder} ({L.PluginBroken})",
                    FontSize = 11.5, Margin = new Thickness(0, 0, 0, 6),
                    IsChecked = !_owner._disabledPlugins.Contains(folder),
                    Foreground = (Brush)FindResource("InkBrush")
                };
                chk.Checked += (s, e) => { _owner._disabledPlugins.Remove(folder); _owner.SaveSettings(); };
                chk.Unchecked += (s, e) => { _owner._disabledPlugins.Add(folder); _owner.SaveSettings(); };
                PluginsListPanel.Children.Add(chk);
            }
        }

        private void BuildGalleryList()
        {
            GalleryHeaderText.Text = L.GalleryHeader;
            GalleryHintText.Text = L.GalleryHint;
            PluginSdkHeaderText.Text = L.PluginSdkHeader;
            PluginSdkHintText.Text = L.PluginSdkHint;
            ScaffoldPluginButton.Content = L.ScaffoldPluginButton;
            ScaffoldResultText.Text = "";
            MarketplaceHeaderText.Text = L.MarketplaceHeader;
            MarketplaceHintText.Text = L.MarketplaceHint;
            InstallFromFolderButton.Content = L.InstallFromFolderButton;
            InstallFromGitButton.Content = L.InstallFromGitButton;
            MarketplaceResultText.Text = "";
            GalleryListPanel.Children.Clear();

            var installed = new HashSet<string>();
            try
            {
                if (Directory.Exists(PluginLoader.PluginsRoot))
                    foreach (var d in Directory.GetDirectories(PluginLoader.PluginsRoot))
                        installed.Add(Path.GetFileName(d) ?? "");
            }
            catch { /* κενός φάκελος, φυσιολογικό */ }

            var bundled = PluginLoader.GetBundledPluginFolders();
            if (bundled.Count == 0)
            {
                GalleryListPanel.Children.Add(new TextBlock
                {
                    Text = L.GalleryEmpty, FontSize = 11, TextWrapping = TextWrapping.Wrap,
                    Foreground = (Brush)FindResource("InkSoftBrush")
                });
                return;
            }

            foreach (var folder in bundled)
            {
                var row = new Grid { Margin = new Thickness(0, 0, 0, 8) };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var isInstalled = installed.Contains(folder);
                var label = new TextBlock
                {
                    Text = isInstalled ? $"{folder}  ✓ {L.GalleryInstalledLabel}" : folder,
                    FontSize = 11.5, VerticalAlignment = VerticalAlignment.Center,
                    Foreground = (Brush)FindResource(isInstalled ? "InkSoftBrush" : "InkBrush")
                };
                Grid.SetColumn(label, 0);
                row.Children.Add(label);

                var btn = new Button
                {
                    Content = isInstalled ? L.GalleryUninstallButton : L.GalleryInstallButton,
                    Padding = new Thickness(10, 4, 10, 4), FontSize = 10.5,
                    Background = Brushes.Transparent, BorderBrush = (Brush)FindResource("LineBrush"), BorderThickness = new Thickness(1),
                    Foreground = (Brush)FindResource("InkBrush"), Cursor = System.Windows.Input.Cursors.Hand
                };
                var capturedFolder = folder;
                btn.Click += (s, e) =>
                {
                    try
                    {
                        if (isInstalled) PluginLoader.UninstallPlugin(capturedFolder);
                        else PluginLoader.InstallBundledPlugin(capturedFolder);
                        BuildPluginsList();
                        BuildGalleryList();
                    }
                    catch (Exception ex) { MessageBox.Show(this, ex.Message, L.LinkOpenError); }
                };
                Grid.SetColumn(btn, 1);
                row.Children.Add(btn);

                GalleryListPanel.Children.Add(row);
            }
        }

        private void ReplayOnboardingButton_Click(object sender, RoutedEventArgs e)
        {
            _owner.ShowOnboarding(isReplay: true);
            Close();
        }

        private string? _lastUpdateReleaseUrl;

        private void UpdateRepoBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _owner._updateCheckRepo = UpdateRepoBox.Text.Trim();
            _owner.SaveSettings();
        }

        /// <summary>Έλεγχος νέας έκδοσης (v4.5.0) - ΜΟΝΟ όταν ο χρήστης πατήσει ρητά αυτό το κουμπί, ΠΟΤΕ
        /// αυτόματα στο παρασκήνιο. Ένα HTTP GET στο δημόσιο GitHub Releases API - καμία σιωπηλή λήψη ή
        /// εγκατάσταση, μόνο ειδοποίηση + link.</summary>
        private async void CheckUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            CheckUpdateButton.IsEnabled = false;
            UpdateResultText.Text = L.CheckUpdateChecking;
            _lastUpdateReleaseUrl = null;

            var result = await UpdateChecker.CheckAsync(_owner._updateCheckRepo, L.AppVersion);

            if (!result.Success)
            {
                UpdateResultText.Text = result.Error == "no-repo" ? L.CheckUpdateNoRepo : L.CheckUpdateError;
            }
            else if (result.UpdateAvailable)
            {
                UpdateResultText.Text = L.CheckUpdateAvailable(result.LatestVersion!);
                _lastUpdateReleaseUrl = result.ReleaseUrl;
            }
            else
            {
                UpdateResultText.Text = L.CheckUpdateLatest;
            }
            CheckUpdateButton.IsEnabled = true;
        }

        private void UpdateResultText_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!string.IsNullOrEmpty(_lastUpdateReleaseUrl)) _owner.OpenUrl(_lastUpdateReleaseUrl);
        }

        private void OpenPluginsFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Directory.CreateDirectory(PluginLoader.PluginsRoot);
                Process.Start(new ProcessStartInfo(PluginLoader.PluginsRoot) { UseShellExecute = true });
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, L.LinkOpenError); }
        }

        private void RefreshSyncFolderDisplay()
        {
            var folder = _owner._conversationsSyncFolder;
            SyncFolderBox.Text = string.IsNullOrEmpty(folder) ? L.SyncFolderDefault : folder;
            ResetSyncFolderButton.Visibility = string.IsNullOrEmpty(folder) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void BrowseSyncFolderButton_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = L.SyncFolderHint,
                UseDescriptionForTitle = true
            };
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            _owner.ChangeConversationsSyncFolder(dialog.SelectedPath);
            RefreshSyncFolderDisplay();
            BuildSharedProjectsList();
        }

        private void ResetSyncFolderButton_Click(object sender, RoutedEventArgs e)
        {
            _owner.ChangeConversationsSyncFolder("");
            RefreshSyncFolderDisplay();
            BuildSharedProjectsList();
        }

        // ---------- Κοινόχρηστα Projects μεταξύ υπολογιστών (πάνω στον ήδη υπάρχοντα φάκελο συγχρονισμού -
        // ΔΕΝ διαβάζει τίποτα από claude.ai, δεν υπάρχει δημόσιο API για αυτό· βλ. σχόλιο στο SharedProjectsStore). ----------
        private void BuildSharedProjectsList()
        {
            SharedProjectsListPanel.Children.Clear();
            var syncFolder = _owner._conversationsSyncFolder;
            if (string.IsNullOrEmpty(syncFolder))
            {
                SharedProjectsListPanel.Children.Add(new TextBlock
                {
                    Text = L.SharedProjectsNoSyncFolder, FontSize = 11, TextWrapping = TextWrapping.Wrap,
                    Foreground = (Brush)FindResource("InkSoftBrush")
                });
                return;
            }

            var shared = Services.SharedProjectsStore.ListShared(syncFolder);
            if (shared.Count == 0)
            {
                SharedProjectsListPanel.Children.Add(new TextBlock
                {
                    Text = L.SharedProjectsEmpty, FontSize = 11, TextWrapping = TextWrapping.Wrap,
                    Foreground = (Brush)FindResource("InkSoftBrush")
                });
                return;
            }

            foreach (var proj in shared)
            {
                var card = new Border
                {
                    BorderBrush = (Brush)FindResource("LineBrush"), BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6), Padding = new Thickness(10), Margin = new Thickness(0, 0, 0, 8)
                };
                var stack = new StackPanel();

                var headerRow = new Grid();
                headerRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                headerRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                var nameText = new TextBlock { Text = proj.Name, FontSize = 12, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("InkBrush") };
                Grid.SetColumn(nameText, 0);
                headerRow.Children.Add(nameText);
                var statusBadge = new Border
                {
                    CornerRadius = new CornerRadius(10), Padding = new Thickness(8, 2, 8, 2),
                    Background = StatusColor(proj.Status)
                };
                statusBadge.Child = new TextBlock { Text = L.SharedProjectStatusLabel(proj.Status), FontSize = 9.5, Foreground = Brushes.White };
                Grid.SetColumn(statusBadge, 1);
                headerRow.Children.Add(statusBadge);
                stack.Children.Add(headerRow);

                stack.Children.Add(new TextBlock
                {
                    Text = L.SharedProjectMeta(proj.OriginMachine, proj.FileCount, proj.LastUpdatedUtc.ToLocalTime()),
                    FontSize = 10, Margin = new Thickness(0, 4, 0, 8), Foreground = (Brush)FindResource("InkSoftBrush")
                });

                var actionsRow = new StackPanel { Orientation = Orientation.Horizontal };
                var statusCombo = new ComboBox { Width = 120, FontSize = 10.5, Margin = new Thickness(0, 0, 6, 0) };
                statusCombo.ItemsSource = new[] { "Pending", "InProgress", "Completed" }.Select(L.SharedProjectStatusLabel).ToList();
                statusCombo.SelectedIndex = proj.Status switch { "InProgress" => 1, "Completed" => 2, _ => 0 };
                var capturedId = proj.Id;
                statusCombo.SelectionChanged += (s, e) =>
                {
                    var raw = statusCombo.SelectedIndex switch { 1 => "InProgress", 2 => "Completed", _ => "Pending" };
                    Services.SharedProjectsStore.UpdateStatus(syncFolder, capturedId, raw);
                    BuildSharedProjectsList();
                };
                actionsRow.Children.Add(statusCombo);

                var downloadBtn = new Button
                {
                    Content = L.SharedProjectDownloadButton, Padding = new Thickness(8, 4, 8, 4), FontSize = 10.5,
                    Background = Brushes.Transparent, BorderBrush = (Brush)FindResource("LineBrush"), BorderThickness = new Thickness(1),
                    Foreground = (Brush)FindResource("InkBrush"), Cursor = System.Windows.Input.Cursors.Hand, Margin = new Thickness(0, 0, 6, 0)
                };
                var capturedName = proj.Name;
                downloadBtn.Click += (s, e) =>
                {
                    var dialog = new Microsoft.Win32.SaveFileDialog { Filter = "ZIP (*.zip)|*.zip", FileName = $"{capturedName}.zip" };
                    if (dialog.ShowDialog(this) != true) return;
                    try { Services.SharedProjectsStore.DownloadSnapshot(syncFolder, capturedId, dialog.FileName); MessageBox.Show(this, L.SharedProjectDownloaded(dialog.FileName)); }
                    catch (Exception ex) { MessageBox.Show(this, ex.Message, L.LinkOpenError); }
                };
                actionsRow.Children.Add(downloadBtn);

                var removeBtn = new Button
                {
                    Content = "✕", Width = 26, Padding = new Thickness(0), FontSize = 10.5,
                    Background = Brushes.Transparent, BorderBrush = (Brush)FindResource("LineBrush"), BorderThickness = new Thickness(1),
                    Foreground = (Brush)FindResource("InkSoftBrush"), Cursor = System.Windows.Input.Cursors.Hand
                };
                removeBtn.Click += (s, e) => { Services.SharedProjectsStore.RemoveShared(syncFolder, capturedId); BuildSharedProjectsList(); };
                actionsRow.Children.Add(removeBtn);

                stack.Children.Add(actionsRow);
                card.Child = stack;
                SharedProjectsListPanel.Children.Add(card);
            }
        }

        private static Brush StatusColor(string status) => status switch
        {
            "Completed" => new SolidColorBrush(Color.FromRgb(0x2E, 0x8B, 0x57)),
            "InProgress" => new SolidColorBrush(Color.FromRgb(0xE0, 0x9A, 0x2D)),
            _ => new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80))
        };

        private void PublishActiveProjectButton_Click(object sender, RoutedEventArgs e)
        {
            var syncFolder = _owner._conversationsSyncFolder;
            if (string.IsNullOrEmpty(syncFolder)) { MessageBox.Show(this, L.SharedProjectsNoSyncFolder); return; }
            if (string.IsNullOrEmpty(_owner._activeProjectPath)) { MessageBox.Show(this, L.NoActiveProject); return; }

            try
            {
                var name = System.IO.Path.GetFileName(_owner._activeProjectPath.TrimEnd(System.IO.Path.DirectorySeparatorChar));
                Services.SharedProjectsStore.PublishSnapshot(syncFolder, _owner._activeProjectPath, name, "Pending");
                BuildSharedProjectsList();
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, L.LinkOpenError); }
        }

        private void ExportBackupButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON (*.json)|*.json",
                FileName = $"kombos-backup-{DateTime.Now:yyyy-MM-dd-HHmm}.json"
            };
            if (dialog.ShowDialog(this) != true) return;
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(_owner._conversations,
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(dialog.FileName, json);
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, L.LinkOpenError); }
        }

        private void ImportBackupButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog { Filter = "JSON (*.json)|*.json" };
            if (dialog.ShowDialog(this) != true) return;
            try
            {
                var json = System.IO.File.ReadAllText(dialog.FileName);
                var imported = System.Text.Json.JsonSerializer.Deserialize<List<Conversation>>(json);
                if (imported == null || imported.Count == 0) { MessageBox.Show(this, L.ImportBackupEmpty); return; }
                _owner.ImportConversationsBackup(imported);
                MessageBox.Show(this, L.ImportBackupSuccess(imported.Count));
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, L.LinkOpenError); }
        }

        /// <summary>Εισαγωγή από αρχεία εξαγωγής ChatGPT/Claude.ai (v4.5.0) - ΤΟΠΙΚΟ parsing του JSON που ο
        /// χρήστης ήδη έχει κατεβάσει από το "Data export" της κάθε υπηρεσίας του, καμία ζωντανή κλήση API ή
        /// scraping. Βλ. Services/ConversationImporter.cs για τα δύο αναγνωρισμένα σχήματα.</summary>
        private void ImportChatExportButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog { Filter = "JSON (*.json)|*.json" };
            if (dialog.ShowDialog(this) != true) return;
            try
            {
                var json = System.IO.File.ReadAllText(dialog.FileName);
                var imported = ConversationImporter.ImportFromJson(json);
                if (imported.Count == 0) { MessageBox.Show(this, L.ImportChatExportEmpty); return; }
                _owner.ImportConversationsBackup(imported);
                MessageBox.Show(this, L.ImportBackupSuccess(imported.Count));
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, L.LinkOpenError); }
        }

        /// <summary>Plugin SDK v2 (v4.5.0) - "Νέο Plugin" wizard, βλ. Services/PluginScaffolder.cs.</summary>
        private void ScaffoldPluginButton_Click(object sender, RoutedEventArgs e)
        {
            var name = NewPluginNameBox.Text.Trim();
            if (string.IsNullOrEmpty(name)) { ScaffoldResultText.Text = L.ScaffoldNeedsName; return; }

            try
            {
                var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName
                    ?? System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Kombos.exe");
                var projectDir = PluginScaffolder.Scaffold(name, exePath);
                ScaffoldResultText.Text = L.ScaffoldSuccess(projectDir);
                System.Diagnostics.Process.Start("explorer.exe", projectDir);
            }
            catch (Exception ex) { ScaffoldResultText.Text = L.SomethingWentWrong(ex.Message); }
        }

        /// <summary>Nexus Marketplace (v4.5.0) - εγκατάσταση από τοπικό φάκελο (ήδη-χτισμένα .dll).</summary>
        private void InstallFromFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            try
            {
                var dlls = System.IO.Directory.GetFiles(dialog.SelectedPath, "*.dll");
                if (dlls.Length == 0) { MarketplaceResultText.Text = L.MarketplaceNoDlls; return; }

                var name = System.IO.Path.GetFileName(dialog.SelectedPath.TrimEnd('\\', '/'));
                PluginLoader.InstallFromDllFiles(name, dlls);
                MarketplaceResultText.Text = L.MarketplaceInstalled(name);
                BuildPluginsList();
                BuildGalleryList();
            }
            catch (Exception ex) { MarketplaceResultText.Text = L.SomethingWentWrong(ex.Message); }
        }

        /// <summary>Nexus Marketplace (v4.5.0) - εγκατάσταση από Git URL, βλ. Services/PluginMarketplace.cs.</summary>
        private async void InstallFromGitButton_Click(object sender, RoutedEventArgs e)
        {
            var url = MarketplaceGitUrlBox.Text.Trim();
            if (string.IsNullOrEmpty(url)) return;

            InstallFromGitButton.IsEnabled = false;
            MarketplaceResultText.Text = L.MarketplaceDownloading;
            try
            {
                var name = await PluginMarketplace.InstallFromGitHubAsync(url);
                MarketplaceResultText.Text = L.MarketplaceInstalled(name);
                BuildPluginsList();
                BuildGalleryList();
            }
            catch (Exception ex) { MarketplaceResultText.Text = L.SomethingWentWrong(ex.Message); }
            finally { InstallFromGitButton.IsEnabled = true; }
        }
    }
}
