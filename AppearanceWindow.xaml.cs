using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Kombos.Localization;
using Kombos.Models;
using Kombos.Services;

namespace Kombos
{
    /// <summary>Αποκλειστικό δευτερεύον Window μόνο για Εμφάνιση/Θέματα/Skins (ξεχωριστό από τις γενικές
    /// Ρυθμίσεις) - δεν κλείνει μόνο του όταν το κύριο παράθυρο χάνει το focus.</summary>
    public partial class AppearanceWindow : Window
    {
        private readonly MainWindow _owner;
        private bool _suppressSkinChange;
        private bool _suppressBgStyleChange;

        public AppearanceWindow(MainWindow owner)
        {
            InitializeComponent();
            _owner = owner;
            Owner = owner;

            ApplyTexts();
            SkinChrome.ApplySquareIfClassic(_owner._skin, ShellBorder, AppearanceHeader);
        }

        private void AppearanceHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
        private void CloseAppearanceButton_Click(object sender, RoutedEventArgs e) => Close();

        internal void ApplyTexts()
        {
            AppearanceTitleText.Text = L.SectionAppearance;
            ThemeModeLabelText.Text = L.ThemeModeLabel;
            ThemeModeButton.Content = L.ThemeModeButtonText(_owner._themeMode);
            SkinLabelText.Text = L.SkinLabel;
            TransparencyLabelText.Text = L.TransparencyLabel;
            TransparencyButton.Content = L.TransparencyButtonText(_owner._transparency);
            BgStyleLabelText.Text = L.BgStyleLabel;
            SidebarPosLabelText.Text = L.SidebarPosLabel;
            SidebarPosButton.Content = L.SidebarPosButtonText(_owner._sidebarRight);
            ChatTextScaleLabelText.Text = L.ChatTextScaleLabel;
            ChatTextScaleButton.Content = L.ChatTextScaleButtonText(_owner._chatTextScale);
            AppearanceLiveNoteText.Text = L.AppearanceLiveNote;
            PopulateSkinCombo();
            PopulateBgStyleCombo();
        }

        private void PopulateBgStyleCombo()
        {
            _suppressBgStyleChange = true;
            var styles = Enum.GetValues(typeof(BgAnimStyle)).Cast<BgAnimStyle>().ToList();
            BgStyleCombo.ItemsSource = styles.Select(s => L.BgStyleName(s)).ToList();
            BgStyleCombo.SelectedIndex = styles.IndexOf(_owner._bgStyle);
            _suppressBgStyleChange = false;
        }

        private void BgStyleCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_suppressBgStyleChange) return;
            var styles = Enum.GetValues(typeof(BgAnimStyle)).Cast<BgAnimStyle>().ToList();
            if (BgStyleCombo.SelectedIndex < 0 || BgStyleCombo.SelectedIndex >= styles.Count) return;
            _owner._bgStyle = styles[BgStyleCombo.SelectedIndex];
            _owner.ApplyAppearance();
            _owner.SaveSettings();
        }

        private void PopulateSkinCombo()
        {
            _suppressSkinChange = true;
            var skins = Enum.GetValues(typeof(WindowsSkin)).Cast<WindowsSkin>().ToList();
            SkinCombo.ItemsSource = skins.Select(s => L.SkinName(s)).ToList();
            SkinCombo.SelectedIndex = skins.IndexOf(_owner._skin);
            _suppressSkinChange = false;
        }

        private void SkinCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_suppressSkinChange) return;
            var skins = Enum.GetValues(typeof(WindowsSkin)).Cast<WindowsSkin>().ToList();
            if (SkinCombo.SelectedIndex < 0 || SkinCombo.SelectedIndex >= skins.Count) return;
            _owner._skin = skins[SkinCombo.SelectedIndex];
            _owner.ApplyAppearance();
            _owner.SaveSettings();
        }

        private void ThemeModeButton_Click(object sender, RoutedEventArgs e)
        {
            _owner._themeMode = _owner._themeMode switch
            {
                ThemeMode.Light => ThemeMode.Dark,
                ThemeMode.Dark => ThemeMode.System,
                _ => ThemeMode.Light
            };
            _owner.ApplyThemeMode();
            ThemeModeButton.Content = L.ThemeModeButtonText(_owner._themeMode);
            _owner.SaveSettings();
        }

        private void TransparencyButton_Click(object sender, RoutedEventArgs e)
        {
            _owner._transparency = _owner._transparency switch
            {
                TransparencyLevel.None => TransparencyLevel.Low,
                TransparencyLevel.Low => TransparencyLevel.Medium,
                _ => TransparencyLevel.None
            };
            _owner.ApplyAppearance();
            TransparencyButton.Content = L.TransparencyButtonText(_owner._transparency);
            _owner.SaveSettings();
        }

        private void SidebarPosButton_Click(object sender, RoutedEventArgs e)
        {
            _owner._sidebarRight = !_owner._sidebarRight;
            _owner.ApplySidebarPosition();
            SidebarPosButton.Content = L.SidebarPosButtonText(_owner._sidebarRight);
            _owner.SaveSettings();
        }

        private void ChatTextScaleButton_Click(object sender, RoutedEventArgs e)
        {
            _owner._chatTextScale = _owner._chatTextScale switch
            {
                ChatTextScale.Small => ChatTextScale.Normal,
                ChatTextScale.Normal => ChatTextScale.Large,
                _ => ChatTextScale.Small
            };
            ChatTextScaleButton.Content = L.ChatTextScaleButtonText(_owner._chatTextScale);
            _owner.SaveSettings();
        }
    }
}
