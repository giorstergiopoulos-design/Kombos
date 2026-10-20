using System;
using System.Collections.Generic;
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
    /// <summary>Εξατομικευμένες ρυθμίσεις του πρωτεύοντα agent (κλειδί API, προσαρμοσμένες οδηγίες, MCP
    /// servers, Συλλογική Μνήμη) - ξεχωριστό παράθυρο από τις γενικές ρυθμίσεις εφαρμογής (SettingsWindow).
    /// Το ΜΟΝΟ σημείο εισόδου είναι το κουμπί "C" (avatar) στην πλευρική μπάρα - το γρανάζι δεν το αγγίζει.</summary>
    public partial class AgentSettingsWindow : Window
    {
        private readonly MainWindow _owner;

        public AgentSettingsWindow(MainWindow owner)
        {
            InitializeComponent();
            _owner = owner;
            Owner = owner;

            ApplyTexts();
            EnsureAgentComboPopulated();
            SyncFromOwnerState();
            PopulatePresetsCombo();
            SkinChrome.ApplySquareIfClassic(_owner._skin, ShellBorder, AgentHeader);
        }

        private void AgentHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
        private void CloseAgentSettingsButton_Click(object sender, RoutedEventArgs e) => Close();

        internal void ApplyTexts()
        {
            AgentTitleText.Text = L.SectionAgent;
            SettingsAgentLabelText.Text = L.SettingsAgentLabel;
            SettingsKeyLabelText.Text = L.SettingsKeyLabel;
            SetCentralAgentButton.Content = L.SetCentralAgentButton;
            InstructionsExpander.Header = L.CustomInstructionsLabel;
            CustomInstructionsHintText.Text = L.CustomInstructionsHint;
            McpExpander.Header = L.McpServersLabel;
            McpServersHintText.Text = L.McpServersHint;
            LoginCredentialsExpander.Header = L.LoginCredentialsLabel;
            LoginCredentialsHintText.Text = L.LoginCredentialsHint;
            HiveMemoryExpander.Header = L.HiveMemoryLabel;
            HiveMemoryHintText.Text = L.HiveMemoryHint;
            HiveMemoryClearButton.Content = L.HiveMemoryClearButton;
            BuildHiveMemoryList();
            PresetsLabelText.Text = L.PresetsLabel;
            PresetsHintText.Text = L.PresetsHint;
            SavePresetButton.Content = L.SavePresetButtonText;

            UpdateCentralAgentStatus();
            UpdateSettingsHint();
        }

        private void SyncFromOwnerState()
        {
            AgentSelectCombo.SelectionChanged -= AgentSelectCombo_SelectionChanged;
            AgentSelectCombo.SelectedItem = _owner._centralAgentName;
            AgentSelectCombo.SelectionChanged += AgentSelectCombo_SelectionChanged;

            AgentApiKeyBox.PasswordChanged -= AgentApiKeyBox_PasswordChanged;
            AgentApiKeyBox.Password = _owner._apiKeys.TryGetValue(_owner._centralAgentName, out var key) ? key : "";
            AgentApiKeyBox.PasswordChanged += AgentApiKeyBox_PasswordChanged;

            AgentUsernameBox.Text = _owner._agentUsernames.TryGetValue(_owner._centralAgentName, out var user) ? user : "";

            AgentPasswordBox.PasswordChanged -= AgentPasswordBox_PasswordChanged;
            AgentPasswordBox.Password = _owner._agentPasswords.TryGetValue(_owner._centralAgentName, out var pass) ? pass : "";
            AgentPasswordBox.PasswordChanged += AgentPasswordBox_PasswordChanged;

            CustomInstructionsBox.Text = _owner._customInstructions;
            McpServersBox.Text = _owner._mcpServersRaw;
        }

        private void CustomInstructionsBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _owner._customInstructions = CustomInstructionsBox.Text;
            _owner.SaveSettings();
        }

        private void McpServersBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _owner._mcpServersRaw = McpServersBox.Text;
            _owner.SaveSettings();
        }

        private void EnsureAgentComboPopulated()
        {
            var names = new List<string> { "Claude" };
            names.AddRange(Catalog.Tools.Select(t => t.Name).Where(n => n != "Claude"));
            AgentSelectCombo.ItemsSource = names;
        }

        private void UpdateSettingsHint()
        {
            var selected = AgentSelectCombo.SelectedItem as string ?? _owner._centralAgentName;
            SettingsHintText.Text = selected == "Claude" ? L.SettingsHintClaude : L.SettingsHintOther;
        }

        private void UpdateCentralAgentStatus()
        {
            var tool = Catalog.Tools.FirstOrDefault(t => t.Name == _owner._centralAgentName);
            bool hasIntegration = _owner._centralAgentName == "Claude" || tool?.OpenAiCompatBaseUrl != null;
            CentralAgentStatusText.Text = L.CentralAgentStatus(_owner._centralAgentName, hasIntegration);
        }

        private void SetCentralAgentButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = AgentSelectCombo.SelectedItem as string;
            if (string.IsNullOrEmpty(selected)) return;
            _owner._centralAgentName = selected;
            _owner.ApplyCentralAgent();
            _owner.HeaderSubtitleText.Text = L.HeaderSubtitle(_owner._centralAgentName);
            UpdateCentralAgentStatus();
            _owner.SaveSettings();
        }

        /// <summary>Presets/Personas (v4.5.0, ιδέα από LibreChat presets / NextChat "Masks") - συνδυασμός
        /// κεντρικού agent + προσαρμοσμένων οδηγιών, εναλλαγή με ένα κλικ. Πάντα τοπικά αποθηκευμένα
        /// (%AppData%\Kombos\presets-list.json), ανεξάρτητα από τον προαιρετικό φάκελο συγχρονισμού.</summary>
        private void PopulatePresetsCombo()
        {
            PresetsCombo.SelectionChanged -= PresetsCombo_SelectionChanged;
            PresetsCombo.DisplayMemberPath = nameof(AgentPreset.Name);
            PresetsCombo.ItemsSource = null;
            PresetsCombo.ItemsSource = _owner._presets;
            PresetsCombo.SelectedIndex = -1;
            PresetsCombo.SelectionChanged += PresetsCombo_SelectionChanged;
        }

        private void PresetsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PresetsCombo.SelectedItem is not AgentPreset preset) return;

            _owner._centralAgentName = preset.CentralAgent;
            _owner._customInstructions = preset.CustomInstructions;
            _owner.ApplyCentralAgent();
            _owner.HeaderSubtitleText.Text = L.HeaderSubtitle(_owner._centralAgentName);
            _owner.SaveSettings();

            SyncFromOwnerState();
            UpdateCentralAgentStatus();
            UpdateSettingsHint();
        }

        private void SavePresetButton_Click(object sender, RoutedEventArgs e)
        {
            PresetNameBox.Text = "";
            PresetNameBox.Visibility = Visibility.Visible;
            PresetNameBox.Focus();
        }

        private void PresetNameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) CommitNewPreset();
            else if (e.Key == Key.Escape) PresetNameBox.Visibility = Visibility.Collapsed;
        }

        private void PresetNameBox_LostFocus(object sender, RoutedEventArgs e) => PresetNameBox.Visibility = Visibility.Collapsed;

        private void CommitNewPreset()
        {
            var name = PresetNameBox.Text.Trim();
            if (!string.IsNullOrEmpty(name))
            {
                _owner._presets.Add(new AgentPreset
                {
                    Name = name,
                    CentralAgent = _owner._centralAgentName,
                    CustomInstructions = _owner._customInstructions
                });
                PresetsStore.Save(_owner._presets);
                PopulatePresetsCombo();
            }
            PresetNameBox.Visibility = Visibility.Collapsed;
        }

        private void DeletePresetButton_Click(object sender, RoutedEventArgs e)
        {
            if (PresetsCombo.SelectedItem is not AgentPreset preset) return;
            _owner._presets.Remove(preset);
            PresetsStore.Save(_owner._presets);
            PopulatePresetsCombo();
        }

        private void AgentSelectCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = AgentSelectCombo.SelectedItem as string;
            if (selected == null) return;
            AgentApiKeyBox.PasswordChanged -= AgentApiKeyBox_PasswordChanged;
            AgentApiKeyBox.Password = _owner._apiKeys.TryGetValue(selected, out var key) ? key : "";
            AgentApiKeyBox.PasswordChanged += AgentApiKeyBox_PasswordChanged;

            AgentUsernameBox.Text = _owner._agentUsernames.TryGetValue(selected, out var user) ? user : "";

            AgentPasswordBox.PasswordChanged -= AgentPasswordBox_PasswordChanged;
            AgentPasswordBox.Password = _owner._agentPasswords.TryGetValue(selected, out var pass) ? pass : "";
            AgentPasswordBox.PasswordChanged += AgentPasswordBox_PasswordChanged;

            UpdateSettingsHint();
        }

        private void AgentUsernameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var selected = AgentSelectCombo.SelectedItem as string;
            if (selected == null) return;
            if (string.IsNullOrEmpty(AgentUsernameBox.Text)) _owner._agentUsernames.Remove(selected);
            else _owner._agentUsernames[selected] = AgentUsernameBox.Text;
            _owner.SaveSettings();
        }

        private void AgentPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var selected = AgentSelectCombo.SelectedItem as string;
            if (selected == null) return;
            if (string.IsNullOrEmpty(AgentPasswordBox.Password)) _owner._agentPasswords.Remove(selected);
            else _owner._agentPasswords[selected] = AgentPasswordBox.Password;
        }

        private void AgentPasswordBox_LostFocus(object sender, RoutedEventArgs e) => _owner.SaveSettings();

        /// <summary>Κουμπιά επικόλλησης (v4.6.0, εναλλακτικό στην απορριφθείσα ιδέα ανάγνωσης αποθηκευμένων
        /// κωδικών browser - βλ. συζήτηση) - ο χρήστης αντιγράφει ο ίδιος από το δικό του password manager
        /// και επικολλά εδώ με ένα κλικ, αντί για Ctrl+V που δεν είναι πάντα προφανές σε PasswordBox.</summary>
        private void PasteApiKeyButton_Click(object sender, RoutedEventArgs e)
        {
            try { AgentApiKeyBox.Password = Clipboard.GetText(); } catch { /* άδειο/μη-κειμενικό πρόχειρο - όχι κρίσιμο */ }
        }

        private void PasteUsernameButton_Click(object sender, RoutedEventArgs e)
        {
            try { AgentUsernameBox.Text = Clipboard.GetText(); AgentUsernameBox_LostFocus(sender, e); }
            catch { /* άδειο/μη-κειμενικό πρόχειρο - όχι κρίσιμο */ }
        }

        private void PastePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            try { AgentPasswordBox.Password = Clipboard.GetText(); } catch { /* άδειο/μη-κειμενικό πρόχειρο - όχι κρίσιμο */ }
        }

        private void AgentApiKeyBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var selected = AgentSelectCombo.SelectedItem as string;
            if (selected == null) return;
            _owner._apiKeys[selected] = AgentApiKeyBox.Password;
            if (selected == "Claude") _owner._claude.ApiKey = AgentApiKeyBox.Password;
        }

        private async void AgentApiKeyBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _owner.SaveSettings();
            _owner.SyncUsageConnectionStatus();
            _owner.UpdateStatusBar();
            if (_owner.BreakdownPopup.IsOpen) _owner.RefreshBreakdownPanel();

            var selected = AgentSelectCombo.SelectedItem as string;
            if (selected != "ElevenLabs") return;

            var key = AgentApiKeyBox.Password;
            if (string.IsNullOrWhiteSpace(key)) return;

            var row = _owner._usageRows.FirstOrDefault(r => r.Name == "ElevenLabs");
            if (row == null) return;

            try
            {
                var usage = await ElevenLabsClient.GetUsageAsync(key);
                if (usage == null) return;
                row.IsConnected = true;
                row.Used = usage.CharactersUsed;
                row.Limit = usage.CharacterLimit;
                row.Unit = L.CharactersUnit;
                _owner.UpdateStatusBar();
                if (_owner.BreakdownPopup.IsOpen) _owner.RefreshBreakdownPanel();
            }
            catch
            {
                // Άκυρο κλειδί ή offline - μένει σαν "δεν έχει συνδεθεί", όχι crash.
            }
        }

        private void BuildHiveMemoryList()
        {
            HiveMemoryListPanel.Children.Clear();
            if (_owner._hiveMemory.Count == 0)
            {
                HiveMemoryListPanel.Children.Add(new TextBlock
                {
                    Text = L.HiveMemoryEmpty, FontSize = 11, TextWrapping = TextWrapping.Wrap,
                    Foreground = (Brush)FindResource("InkSoftBrush")
                });
                return;
            }

            foreach (var fact in _owner._hiveMemory)
            {
                var row = new Grid { Margin = new Thickness(0, 0, 0, 6) };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var label = new TextBlock
                {
                    Text = fact, FontSize = 11, TextWrapping = TextWrapping.Wrap,
                    VerticalAlignment = VerticalAlignment.Center, Foreground = (Brush)FindResource("InkBrush")
                };
                Grid.SetColumn(label, 0);
                row.Children.Add(label);

                var del = new Button
                {
                    Content = "✕", Width = 26, Height = 22, Margin = new Thickness(6, 0, 0, 0),
                    Background = Brushes.Transparent, BorderBrush = (Brush)FindResource("LineBrush"), BorderThickness = new Thickness(1),
                    Foreground = (Brush)FindResource("InkSoftBrush"), Cursor = System.Windows.Input.Cursors.Hand
                };
                var capturedFact = fact;
                del.Click += (s, e) =>
                {
                    _owner._hiveMemory = _owner._hiveMemory.Where(f => f != capturedFact).ToList();
                    HiveMemoryStore.Save(_owner._hiveMemory);
                    BuildHiveMemoryList();
                };
                Grid.SetColumn(del, 1);
                row.Children.Add(del);

                HiveMemoryListPanel.Children.Add(row);
            }
        }

        private void AddHiveMemoryFromBox()
        {
            var text = HiveMemoryAddBox.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;
            _owner._hiveMemory = HiveMemoryStore.Add(_owner._hiveMemory, text);
            HiveMemoryAddBox.Text = "";
            BuildHiveMemoryList();
        }

        private void HiveMemoryAddButton_Click(object sender, RoutedEventArgs e) => AddHiveMemoryFromBox();

        private void HiveMemoryAddBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter) AddHiveMemoryFromBox();
        }

        private void HiveMemoryClearButton_Click(object sender, RoutedEventArgs e)
        {
            if (_owner._hiveMemory.Count == 0) return;
            var result = MessageBox.Show(this, L.ClearAllConfirmMessage, L.ClearAllConfirmTitle,
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (result != MessageBoxResult.Yes) return;
            _owner._hiveMemory = new List<string>();
            HiveMemoryStore.Save(_owner._hiveMemory);
            BuildHiveMemoryList();
        }
    }
}
