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
    /// <summary>Λίστα όλων των agents/εργαλείων με την κατάσταση σύνδεσής τους - ΔΕΝ κάνει ποτέ login/εγγραφή
    /// για λογαριασμό του χρήστη (το Kombos δεν εισάγει ποτέ credentials σε φόρμα ιστοσελίδας ούτε δημιουργεί
    /// λογαριασμούς). Δείχνει μόνο αν υπάρχουν ήδη αποθηκευμένα (τοπικά, DPAPI) στοιχεία αναφοράς για το κάθε
    /// εργαλείο, και ανοίγει την πραγματική σελίδα σύνδεσης/εγγραφής στον προεπιλεγμένο browser για να τα
    /// χειριστεί ο ίδιος ο χρήστης εκεί.</summary>
    public partial class AccountsWindow : Window
    {
        private readonly MainWindow _owner;

        public AccountsWindow(MainWindow owner)
        {
            InitializeComponent();
            _owner = owner;
            Owner = owner;

            AccountsTitleText.Text = L.AccountsTitle;
            AccountsHintText.Text = L.AccountsHint;
            BuildList();
            SkinChrome.ApplySquareIfClassic(_owner._skin, ShellBorder, AccountsHeader);
        }

        private void AccountsHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
        private void CloseAccountsButton_Click(object sender, RoutedEventArgs e) => Close();

        private void BuildList()
        {
            AccountsListPanel.Children.Clear();

            var entries = new[] { new ToolItem { Name = "Claude", Url = "https://claude.ai" } }
                .Concat(Catalog.Tools.Where(t => t.Name != "Claude"))
                .ToList();

            _owner.SyncUsageConnectionStatus();

            foreach (var tool in entries)
            {
                bool hasKey = _owner._apiKeys.TryGetValue(tool.Name, out var key) && !string.IsNullOrWhiteSpace(key);
                bool hasCreds = _owner._agentUsernames.ContainsKey(tool.Name) || (_owner._agentPasswords.TryGetValue(tool.Name, out var pw) && !string.IsNullOrWhiteSpace(pw));
                bool hasAny = hasKey || hasCreds;
                // Ίδιο κριτήριο με τη γραμμή κατάστασης (UsageRow.IsConnected): "συνδεδεμένος" = έχει API key,
                // ώστε τα δύο σημεία της εφαρμογής να δείχνουν πάντα την ίδια κατάσταση.
                bool isConnected = tool.Name == "Claude" ? hasKey : (_owner._usageRows.FirstOrDefault(r => r.Name == tool.Name)?.IsConnected ?? hasKey);

                var card = new Border
                {
                    BorderBrush = (Brush)FindResource("LineBrush"), BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6), Padding = new Thickness(10), Margin = new Thickness(0, 0, 0, 8)
                };
                var row = new Grid();
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var textStack = new StackPanel();
                textStack.Children.Add(new TextBlock { Text = tool.Name, FontSize = 12.5, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("InkBrush") });
                textStack.Children.Add(new TextBlock
                {
                    Text = isConnected ? L.AccountsStatusConnected : L.AccountsStatusDisconnected, FontSize = 10.5, Margin = new Thickness(0, 3, 0, 0),
                    Foreground = isConnected ? (Brush)FindResource("ClaudeBrush") : (Brush)FindResource("InkSoftBrush")
                });
                if (hasAny)
                {
                    textStack.Children.Add(new TextBlock
                    {
                        Text = L.AccountsStatusSaved, FontSize = 9.5, Margin = new Thickness(0, 2, 0, 0),
                        Foreground = (Brush)FindResource("InkSoftBrush")
                    });
                }
                Grid.SetColumn(textStack, 0);
                row.Children.Add(textStack);

                if (!string.IsNullOrWhiteSpace(tool.Url))
                {
                    var openBtn = new Button
                    {
                        Content = hasAny ? L.AccountsOpenLoginButton : L.AccountsSignUpButton, Padding = new Thickness(10, 5, 10, 5), FontSize = 10.5,
                        Background = Brushes.Transparent, BorderBrush = (Brush)FindResource("LineBrush"), BorderThickness = new Thickness(1),
                        Foreground = (Brush)FindResource("InkBrush"), Cursor = Cursors.Hand, VerticalAlignment = VerticalAlignment.Center
                    };
                    var url = tool.Url;
                    openBtn.Click += (s, e) => _owner.OpenUrl(url);
                    Grid.SetColumn(openBtn, 1);
                    row.Children.Add(openBtn);
                }

                card.Child = row;
                AccountsListPanel.Children.Add(card);
            }
        }
    }
}
