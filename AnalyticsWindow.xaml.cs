using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Kombos.Localization;
using Kombos.Services;

namespace Kombos
{
    /// <summary>Τοπικό analytics dashboard (v4.5.0) - χρήση tokens ανά agent/συνεδρία, ζωγραφισμένο με απλά
    /// WPF σχήματα (Rectangle σε Canvas), ΚΑΜΙΑ εξωτερική βιβλιοθήκη γραφημάτων. Δύο μέρη: (1) ένα ιστορικό
    /// γράφημα ράβδων με το σύνολο tokens ανά προηγούμενη πραγματικά-κλεισμένη συνεδρία (βλ.
    /// Services/UsageLogStore.cs - ΟΧΙ ημερήσια κοκκομετρία, βλ. σχόλιο στο UsageLogEntry), και (2) η
    /// αναλυτική ανά-εργαλείο κατανομή της ΤΡΕΧΟΥΣΑΣ συνεδρίας.</summary>
    public partial class AnalyticsWindow : Window
    {
        private readonly MainWindow _owner;

        public AnalyticsWindow(MainWindow owner)
        {
            InitializeComponent();
            _owner = owner;
            Owner = owner;

            AnalyticsTitleText.Text = L.AnalyticsTitle;
            SessionsLabelText.Text = L.AnalyticsSessionsLabel;
            SessionsHintText.Text = L.AnalyticsSessionsHint;
            CurrentSessionLabelText.Text = L.AnalyticsCurrentSessionLabel;
            SessionsEmptyText.Text = L.AnalyticsNoHistory;

            SkinChrome.ApplySquareIfClassic(_owner._skin, ShellBorder, AnalyticsHeader);

            SessionsChartCanvas.SizeChanged += (s, e) => DrawSessionsChart();
            BuildCurrentSessionPanel();
        }

        private void AnalyticsHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
        private void CloseAnalyticsButton_Click(object sender, RoutedEventArgs e) => Close();

        private void DrawSessionsChart()
        {
            SessionsChartCanvas.Children.Clear();
            var width = SessionsChartCanvas.ActualWidth;
            var height = SessionsChartCanvas.ActualHeight;
            if (width <= 0 || height <= 0) return;

            var entries = UsageLogStore.Load().TakeLast(20).ToList();
            if (entries.Count == 0)
            {
                SessionsEmptyText.Visibility = Visibility.Visible;
                return;
            }
            SessionsEmptyText.Visibility = Visibility.Collapsed;

            var totals = entries.Select(e => e.PerToolTokens.Values.Sum()).ToList();
            var max = Math.Max(1, totals.Max());
            var barGap = 4.0;
            var barWidth = Math.Max(3, (width - barGap * (entries.Count - 1)) / entries.Count);

            for (int i = 0; i < entries.Count; i++)
            {
                var barHeight = Math.Max(2, totals[i] / (double)max * (height - 4));
                var bar = new System.Windows.Shapes.Rectangle
                {
                    Width = barWidth, Height = barHeight,
                    RadiusX = 2, RadiusY = 2,
                    ToolTip = $"{DateTime.Parse(entries[i].TimestampUtc).ToLocalTime():dd/MM HH:mm} — {totals[i]:N0} tokens"
                };
                bar.SetResourceReference(System.Windows.Shapes.Shape.FillProperty, "ClaudeBrush");
                Canvas.SetLeft(bar, i * (barWidth + barGap));
                Canvas.SetTop(bar, height - barHeight);
                SessionsChartCanvas.Children.Add(bar);
            }
        }

        private void BuildCurrentSessionPanel()
        {
            CurrentSessionPanel.Children.Clear();
            var rows = _owner._usageRows.Where(r => r.IsConnected || r.Used > 0).ToList();
            var max = Math.Max(1, rows.Count == 0 ? 1 : rows.Max(r => r.Used));

            foreach (var row in rows)
            {
                var line = new Grid { Margin = new Thickness(0, 0, 0, 8) };
                line.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
                line.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                line.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var name = new TextBlock { Text = row.Name, FontSize = 11, VerticalAlignment = VerticalAlignment.Center };
                name.SetResourceReference(TextBlock.ForegroundProperty, "InkBrush");
                Grid.SetColumn(name, 0);

                var track = new Border { Height = 10, CornerRadius = new CornerRadius(5), Margin = new Thickness(6, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center };
                track.SetResourceReference(Border.BackgroundProperty, "PanelBrush");
                var fillWidthFraction = row.Used / (double)max;
                var fill = new Border { CornerRadius = new CornerRadius(5), HorizontalAlignment = HorizontalAlignment.Left };
                fill.SetResourceReference(Border.BackgroundProperty, "ClaudeBrush");
                track.SizeChanged += (s, e) => fill.Width = Math.Max(0, track.ActualWidth * fillWidthFraction);
                track.Child = fill;
                Grid.SetColumn(track, 1);

                var value = new TextBlock { Text = $"{row.Used:N0}", FontSize = 10.5, VerticalAlignment = VerticalAlignment.Center };
                value.SetResourceReference(TextBlock.ForegroundProperty, "InkSoftBrush");
                Grid.SetColumn(value, 2);

                line.Children.Add(name);
                line.Children.Add(track);
                line.Children.Add(value);
                CurrentSessionPanel.Children.Add(line);
            }

            if (rows.Count == 0)
            {
                CurrentSessionPanel.Children.Add(new TextBlock
                {
                    Text = L.AnalyticsNoUsageYet, FontSize = 10.5, TextWrapping = TextWrapping.Wrap,
                    Foreground = (Brush)FindResource("InkSoftBrush")
                });
            }
        }
    }
}
