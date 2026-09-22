using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Input;

using Kombos.Localization;
using Kombos.Services;

namespace Kombos
{
    /// <summary>"Artifact live preview" (v4.5.0) - απόδοση HTML/SVG/Markdown μέσα σε δικό του παράθυρο, όχι
    /// μόνο λίστα κώδικα. Markdown αποδίδεται ΤΟΠΙΚΑ με το ήδη υπάρχον RenderMarkdown (ίδιο rendering με τις
    /// φούσκες συνομιλίας) - χωρίς κανένα νέο dependency. HTML/SVG χρειάζονται πραγματικό browser engine για
    /// σωστό rendering· αντί για ένα νέο, βαρύ WebView2 runtime dependency, χρησιμοποιείται το ήδη
    /// διαθέσιμο <see cref="System.Windows.Forms.WebBrowser"/> (μέσω WindowsFormsHost, ήδη διαθέσιμο αφού το
    /// .csproj έχει UseWindowsForms=true) - παλαιότερο MSHTML engine, αρκετό για βασικό HTML/CSS/SVG χωρίς
    /// νέο εξωτερικό runtime να εγκατασταθεί στον χρήστη.</summary>
    public partial class ArtifactPreviewWindow : Window
    {
        private readonly MainWindow _owner;

        public ArtifactPreviewWindow(MainWindow owner, string language, string code)
        {
            InitializeComponent();
            _owner = owner;
            Owner = owner;

            PreviewTitleText.Text = L.ArtifactPreviewTitle;
            SkinChrome.ApplySquareIfClassic(_owner._skin, ShellBorder, PreviewHeader);

            var lang = language.ToLowerInvariant();
            if (lang is "markdown" or "md")
            {
                var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Padding = new Thickness(18) };
                var stack = new StackPanel();
                _owner.RenderMarkdown(stack, code, isUser: false);
                scroll.Content = stack;
                scroll.SetResourceReference(Control.BackgroundProperty, "PanelBrush");
                PreviewContentHost.Children.Add(scroll);
            }
            else
            {
                var html = lang == "svg"
                    ? $"<html><body style='margin:0;background:#fff'>{code}</body></html>"
                    : code;
                var browser = new System.Windows.Forms.WebBrowser { ScriptErrorsSuppressed = true };
                var host = new WindowsFormsHost { Child = browser };
                PreviewContentHost.Children.Add(host);
                browser.DocumentText = html;
            }
        }

        private void PreviewHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
        private void ClosePreviewButton_Click(object sender, RoutedEventArgs e) => Close();
    }
}
