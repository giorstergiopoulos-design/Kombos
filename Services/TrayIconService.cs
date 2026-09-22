using System;
using System.Reflection;
using System.Windows.Forms;

namespace Kombos.Services
{
    /// <summary>Εικονίδιο στην περιοχή ειδοποιήσεων των Windows (system tray) - επιτρέπει στο Kombos να "τρέχει
    /// στο παρασκήνιο" (κλείσιμο του παραθύρου = απόκρυψη, όχι έξοδος) με γρήγορη πρόσβαση μέσω δεξί-κλικ/διπλό
    /// κλικ. Ξεχωριστό αρχείο ώστε το `using System.Windows.Forms;` να μην συγκρούεται με τα WPF ομώνυμα
    /// (MessageBox, Clipboard κ.ά.) που ήδη χρησιμοποιεί το MainWindow.</summary>
    public class TrayIconService : IDisposable
    {
        private readonly NotifyIcon _icon;
        private readonly ToolStripMenuItem _openItem;
        private readonly ToolStripMenuItem _newChatItem;
        private readonly ToolStripMenuItem _accountsItem;
        private readonly ToolStripMenuItem _exitItem;

        public event Action? OpenRequested;
        public event Action? NewChatRequested;
        public event Action? AccountsRequested;
        public event Action? ExitRequested;

        public TrayIconService(string tooltip, string openText, string newChatText, string accountsText, string exitText)
        {
            System.Drawing.Icon appIcon;
            try
            {
                appIcon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location)
                          ?? System.Drawing.SystemIcons.Application;
            }
            catch { appIcon = System.Drawing.SystemIcons.Application; }

            _openItem = new ToolStripMenuItem(openText);
            _newChatItem = new ToolStripMenuItem(newChatText);
            _accountsItem = new ToolStripMenuItem(accountsText);
            _exitItem = new ToolStripMenuItem(exitText);
            _openItem.Click += (s, e) => OpenRequested?.Invoke();
            _newChatItem.Click += (s, e) => NewChatRequested?.Invoke();
            _accountsItem.Click += (s, e) => AccountsRequested?.Invoke();
            _exitItem.Click += (s, e) => ExitRequested?.Invoke();

            var menu = new ContextMenuStrip();
            menu.Items.Add(_openItem);
            menu.Items.Add(_newChatItem);
            menu.Items.Add(_accountsItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(_exitItem);

            _icon = new NotifyIcon
            {
                Icon = appIcon,
                Text = Truncate(tooltip),
                Visible = true,
                ContextMenuStrip = menu
            };
            _icon.DoubleClick += (s, e) => OpenRequested?.Invoke();
            _icon.BalloonTipClicked += (s, e) => OpenRequested?.Invoke();
        }

        /// <summary>Ειδοποίηση (balloon) όταν φτάνει απάντηση ενώ το παράθυρο δεν είναι στο προσκήνιο - κλικ
        /// πάνω της φέρνει το Kombos μπροστά, ίδια λογική με διπλό κλικ στο εικονίδιο.</summary>
        public void ShowNotification(string title, string text)
        {
            try
            {
                _icon.BalloonTipTitle = title;
                _icon.BalloonTipText = Truncate(text, 240);
                _icon.ShowBalloonTip(4000);
            }
            catch { /* όχι κρίσιμο αν αποτύχει (π.χ. Focus Assist/Do Not Disturb) */ }
        }

        /// <summary>Ενημέρωση κειμένων μετά από αλλαγή γλώσσας.</summary>
        public void ApplyTexts(string tooltip, string openText, string newChatText, string accountsText, string exitText)
        {
            _icon.Text = Truncate(tooltip);
            _openItem.Text = openText;
            _newChatItem.Text = newChatText;
            _accountsItem.Text = accountsText;
            _exitItem.Text = exitText;
        }

        // Το NotifyIcon.Text έχει ιστορικό όριο 63 χαρακτήρων σε παλιότερα Windows - κόβουμε προληπτικά.
        private static string Truncate(string s, int max = 63) => s.Length > max ? s[..(max - 1)] + "…" : s;

        public void Dispose()
        {
            _icon.Visible = false;
            _icon.Dispose();
        }
    }
}
