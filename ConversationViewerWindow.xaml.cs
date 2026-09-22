using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Kombos.Localization;
using Kombos.Models;
using Kombos.Services;

namespace Kombos
{
    /// <summary>"Multi-window" (v4.5.0) - αποσπά μια συζήτηση σε δικό της, ανεξάρτητο top-level παράθυρο για
    /// σύγκριση δίπλα-δίπλα με την κύρια συζήτηση. Σκόπιμα ΜΟΝΟ ανάγνωση (δεν στέλνει νέα μηνύματα, δεν
    /// αγγίζει το _history/API keys) - μια δεύτερη πλήρως-διαδραστική μηχανή συνομιλίας θα διπλασίαζε
    /// κατάσταση (rate limits, αρχείο ιστορικού) χωρίς πραγματικό όφελος για τη χρήση "σύγκριση".</summary>
    public partial class ConversationViewerWindow : Window
    {
        private readonly MainWindow _owner;
        private readonly Conversation _conversation;

        public ConversationViewerWindow(MainWindow owner, Conversation conversation)
        {
            InitializeComponent();
            _owner = owner;
            _conversation = conversation;
            Owner = owner;

            ViewerTitleText.Text = conversation.Title;
            CopyAllButton.Content = L.CopyAllAsMarkdown;
            SkinChrome.ApplySquareIfClassic(_owner._skin, ShellBorder, ViewerHeader);

            Left = owner.Left + 40;
            Top = owner.Top + 40;

            BuildLog();
        }

        private void ViewerHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
        private void CloseViewerButton_Click(object sender, RoutedEventArgs e) => Close();

        private void BuildLog()
        {
            ViewerLogPanel.Children.Clear();
            foreach (var turn in _conversation.History)
            {
                bool isUser = turn.Role == "user";
                var row = new StackPanel
                {
                    Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10),
                    HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left
                };
                var bubble = new Border
                {
                    MaxWidth = 380, Padding = new Thickness(10, 8, 10, 8), CornerRadius = new CornerRadius(8), BorderThickness = new Thickness(1)
                };
                bubble.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
                bubble.SetResourceReference(Border.BackgroundProperty, isUser ? "ClaudeBrush" : "PaperBrush");

                var content = new StackPanel();
                _owner.RenderMarkdown(content, turn.Content, isUser);
                bubble.Child = content;
                row.Children.Add(bubble);
                ViewerLogPanel.Children.Add(row);
            }
        }

        private void CopyAllButton_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            foreach (var turn in _conversation.History)
                sb.AppendLine($"**{turn.Role}:**\n{turn.Content}\n");
            try { Clipboard.SetText(sb.ToString()); CopyAllButton.Content = L.CopiedLabel; }
            catch { /* clipboard busy - not critical */ }
        }
    }
}
