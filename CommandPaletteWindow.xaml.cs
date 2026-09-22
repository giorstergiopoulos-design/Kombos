using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Kombos.Data;
using Kombos.Localization;
using Kombos.Models;

namespace Kombos
{
    internal class PaletteItem
    {
        public string Label { get; init; } = "";
        public string Category { get; init; } = "";
        public Action Execute { get; init; } = () => { };
    }

    /// <summary>Command Palette (Ctrl+K) - γρήγορη, φιλτραρισμένη αναζήτηση σε εργαλεία/εντολές/ρυθμίσεις σε
    /// ένα ενιαίο popup, ίδιο πνεύμα με το command palette του VS Code / Claude Desktop. Καθαρά τοπικό UI,
    /// μηδενικό κόστος tokens - απλώς εκτελεί ήδη υπάρχουσες ενέργειες του κύριου παραθύρου.</summary>
    public partial class CommandPaletteWindow : Window
    {
        private readonly MainWindow _owner;
        private List<PaletteItem> _allItems = new();

        public CommandPaletteWindow(MainWindow owner)
        {
            InitializeComponent();
            _owner = owner;
            Owner = owner;

            PaletteHintText.Text = L.CommandPaletteHint;
            BuildItems();
            RefreshResults("");
            Loaded += (s, e) => PaletteSearchBox.Focus();
        }

        private void BuildItems()
        {
            _allItems = new List<PaletteItem>();

            foreach (var tool in Catalog.Tools)
            {
                var capturedTool = tool;
                _allItems.Add(new PaletteItem { Label = tool.Name, Category = L.CommandPaletteCategoryTool, Execute = () => _owner.ShowDetails(capturedTool) });
            }

            foreach (var command in _owner._pluginHost.SlashCommands.Keys.OrderBy(k => k))
            {
                var capturedCommand = command;
                _allItems.Add(new PaletteItem
                {
                    Label = capturedCommand,
                    Category = L.CommandPaletteCategoryCommand,
                    Execute = () => { _owner.ChatInputBox.Text = capturedCommand + " "; _owner.ChatInputBox.Focus(); _owner.ChatInputBox.CaretIndex = _owner.ChatInputBox.Text.Length; }
                });
            }

            _allItems.Add(new PaletteItem { Label = L.NewChatButton, Category = L.CommandPaletteCategoryAction, Execute = () => _owner.NewChatButton_Click(_owner, new RoutedEventArgs()) });
            _allItems.Add(new PaletteItem { Label = L.SectionAgent, Category = L.CommandPaletteCategoryAction, Execute = () => _owner.OpenAgentSettingsWindow() });
            _allItems.Add(new PaletteItem { Label = L.SectionBehavior, Category = L.CommandPaletteCategoryAction, Execute = () => _owner.OpenSettingsWindow("behavior") });
            _allItems.Add(new PaletteItem { Label = L.SectionProject, Category = L.CommandPaletteCategoryAction, Execute = () => _owner.OpenSettingsWindow("project") });
            _allItems.Add(new PaletteItem { Label = L.SectionPlugins, Category = L.CommandPaletteCategoryAction, Execute = () => _owner.OpenSettingsWindow("plugins") });
            _allItems.Add(new PaletteItem { Label = L.SectionAppearance, Category = L.CommandPaletteCategoryAction, Execute = () => _owner.AppearanceButton_Click(_owner, new RoutedEventArgs()) });
            _allItems.Add(new PaletteItem { Label = L.ThemeToggleLabel, Category = L.CommandPaletteCategoryAction, Execute = () => _owner.ThemeToggleButton_Click(_owner, new RoutedEventArgs()) });
            _allItems.Add(new PaletteItem { Label = L.HistoryTitle, Category = L.CommandPaletteCategoryAction, Execute = () => _owner.ProjectsSidebarButton_Click(_owner, new RoutedEventArgs()) });
            _allItems.Add(new PaletteItem { Label = L.ArtifactsTitle, Category = L.CommandPaletteCategoryAction, Execute = () => _owner.ArtifactsButton_Click(_owner, new RoutedEventArgs()) });
        }

        private void RefreshResults(string query)
        {
            IEnumerable<PaletteItem> filtered = string.IsNullOrWhiteSpace(query)
                ? _allItems
                : _allItems.Where(i => i.Label.Contains(query, StringComparison.OrdinalIgnoreCase) || i.Category.Contains(query, StringComparison.OrdinalIgnoreCase));

            PaletteResultsList.ItemsSource = filtered.Take(60).ToList();
            if (PaletteResultsList.Items.Count > 0) PaletteResultsList.SelectedIndex = 0;
        }

        private void PaletteSearchBox_TextChanged(object sender, TextChangedEventArgs e) => RefreshResults(PaletteSearchBox.Text);

        private void ExecuteSelected()
        {
            if (PaletteResultsList.SelectedItem is not PaletteItem item) return;
            Close();
            item.Execute();
        }

        private void PaletteResultsList_MouseDoubleClick(object sender, MouseButtonEventArgs e) => ExecuteSelected();

        private void CommandPaletteWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    e.Handled = true;
                    Close();
                    break;
                case Key.Enter:
                    e.Handled = true;
                    ExecuteSelected();
                    break;
                case Key.Down:
                    e.Handled = true;
                    if (PaletteResultsList.SelectedIndex < PaletteResultsList.Items.Count - 1) PaletteResultsList.SelectedIndex++;
                    PaletteResultsList.ScrollIntoView(PaletteResultsList.SelectedItem);
                    break;
                case Key.Up:
                    e.Handled = true;
                    if (PaletteResultsList.SelectedIndex > 0) PaletteResultsList.SelectedIndex--;
                    PaletteResultsList.ScrollIntoView(PaletteResultsList.SelectedItem);
                    break;
            }
        }

        private void CommandPaletteWindow_Deactivated(object sender, EventArgs e) => Close();
    }
}
