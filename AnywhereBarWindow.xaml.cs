using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;

using Kombos.Localization;

namespace Kombos
{
    /// <summary>Kombos Anywhere (v4.5.0) - global-hotkey (Ctrl+Alt+Space, βλ. MainWindow.RegisterGlobalHotkey)
    /// mini quick-capture bar. Στέλνει ΜΙΑ αυτόνομη ερώτηση στον κεντρικό agent χωρίς να αγγίζει την
    /// τρέχουσα κύρια συζήτηση (βλ. MainWindow.QuickAskAsync) - πατώντας "Προσθήκη στη συζήτηση" το Q&amp;A
    /// καταγράφεται κανονικά στο ιστορικό. Κλείνει με Esc ή μόλις χάσει το focus (σαν launcher, όχι
    /// κανονικό παράθυρο).</summary>
    public partial class AnywhereBarWindow : Window
    {
        private readonly MainWindow _owner;
        private CancellationTokenSource? _cts;
        private string _lastPrompt = "";
        private string _lastReply = "";

        public AnywhereBarWindow(MainWindow owner)
        {
            InitializeComponent();
            _owner = owner;

            AnywhereHintText.Text = L.AnywhereHint;
            AnywhereAddToChatButton.Content = L.AnywhereAddToChat;

            var workArea = SystemParameters.WorkArea;
            Left = workArea.Left + (workArea.Width - Width) / 2;
            Top = workArea.Top + 120;

            Loaded += (s, e) => AnywhereInputBox.Focus();
        }

        private void AnywhereBarWindow_Deactivated(object sender, EventArgs e) => Close();

        private void AnywhereBarWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
        }

        private async void AnywhereInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            var prompt = AnywhereInputBox.Text.Trim();
            if (string.IsNullOrEmpty(prompt)) return;

            e.Handled = true;
            AnywhereInputBox.IsEnabled = false;
            AnywhereResultBorder.Visibility = Visibility.Visible;
            AnywhereResultText.Text = L.AnywhereThinking;
            AnywhereAddToChatButton.Visibility = Visibility.Collapsed;

            _cts = new CancellationTokenSource();
            try
            {
                var reply = await _owner.QuickAskAsync(prompt, _cts.Token);
                _lastPrompt = prompt;
                _lastReply = reply;
                AnywhereResultText.Text = reply;
                AnywhereAddToChatButton.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                AnywhereResultText.Text = L.SomethingWentWrong(ex.Message);
            }
            finally
            {
                AnywhereInputBox.IsEnabled = true;
                AnywhereInputBox.Focus();
            }
        }

        private void AnywhereAddToChatButton_Click(object sender, RoutedEventArgs e)
        {
            _owner.ShowAndActivate();
            _owner.AppendQuickAskToConversation(_lastPrompt, _lastReply);
            Close();
        }
    }
}
