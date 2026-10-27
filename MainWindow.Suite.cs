using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Kombos.Localization;
using Kombos.Services;

namespace Kombos
{
    /// <summary>v4.9.0: οδηγός καλωσορίσματος ανά έκδοση, βιτρίνα "Η οικογένεια Opsis" και ειδοποίηση νέας έκδοσης.
    /// Όλα τα overlays κλείνουν με το ίδιο εφέ: μικραίνουν και πετάνε μέσα στο κινούμενο εικονίδιο της εφαρμογής
    /// (<c>HeaderIcon</c>), όπως η κουρτίνα της εκκίνησης.</summary>
    public partial class MainWindow
    {
        private static readonly HttpClient SuiteHttp = new() { Timeout = TimeSpan.FromSeconds(30) };

        private string _guideSeenVersion = "";
        private bool _guideDontShow;
        private bool _suiteOffered;
        internal bool _notifyNewVersions = true;
        private bool _flyInProgress;

        /// <summary>"major.minor" της τρέχουσας έκδοσης - ο οδηγός ξαναεμφανίζεται σε κάθε νέα μείζονα/ελάσσονα.</summary>
        private static string GuideMinor
        {
            get
            {
                var parts = L.AppVersion.Split('.');
                return parts.Length >= 2 ? parts[0] + "." + parts[1] : L.AppVersion;
            }
        }

        private bool GuideDue => !_guideDontShow && _guideSeenVersion != GuideMinor;

        /// <summary>Σε χαμηλής ισχύος PC / όταν οι animations είναι κλειστές στα Windows, μόνο απλά fades.</summary>
        private static bool SimpleAnimations => !SystemParameters.ClientAreaAnimation || RenderCapability.Tier >> 16 == 0;

        // ---------- Κοινό εφέ κλεισίματος: scale + move + fade (~400 ms) προς το εικονίδιο της εφαρμογής ----------
        private void FlyIntoHeaderIcon(Panel overlay, FrameworkElement content, Action done, UIElement? alsoFade = null)
        {
            if (_flyInProgress) return;
            _flyInProgress = true;

            bool simple = SimpleAnimations || !HeaderIcon.IsVisible;
            var dur = TimeSpan.FromMilliseconds(simple ? 160 : 420);
            var originalBg = overlay.Background;
            var bg = new SolidColorBrush(originalBg is SolidColorBrush sb ? sb.Color : Colors.Transparent);
            overlay.Background = bg;

            var scale = new ScaleTransform(1, 1);
            var move = new TranslateTransform(0, 0);
            var group = new TransformGroup();
            group.Children.Add(scale);
            group.Children.Add(move);
            content.RenderTransformOrigin = new Point(0.5, 0.5);
            content.RenderTransform = group;

            void Finish()
            {
                overlay.Visibility = Visibility.Collapsed;
                overlay.Background = originalBg;
                content.RenderTransform = Transform.Identity;
                content.Opacity = 1;
                if (alsoFade != null) { alsoFade.BeginAnimation(OpacityProperty, null); alsoFade.Opacity = 1; alsoFade.Visibility = Visibility.Collapsed; }
                _flyInProgress = false;
                done();
            }

            var ease = new CubicEase { EasingMode = EasingMode.EaseInOut };
            var fade = new DoubleAnimation(1, 0, dur);
            bg.BeginAnimation(Brush.OpacityProperty, new DoubleAnimation(1, 0, dur));
            alsoFade?.BeginAnimation(OpacityProperty, new DoubleAnimation(1, 0, dur));
            content.BeginAnimation(OpacityProperty, fade);

            if (simple)
            {
                fade.Completed += (s, e) => Finish();
                return;
            }

            overlay.UpdateLayout();
            var from = content.TranslatePoint(new Point(content.ActualWidth / 2, content.ActualHeight / 2), overlay);
            var to = HeaderIcon.TranslatePoint(new Point(HeaderIcon.Width / 2, HeaderIcon.Height / 2), overlay);
            double target = Math.Max(0.03, HeaderIcon.Width / Math.Max(content.ActualWidth, 1));
            var sx = new DoubleAnimation(1, target, dur) { EasingFunction = ease };
            sx.Completed += (s, e) => Finish();
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, sx);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(1, target, dur) { EasingFunction = ease });
            move.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(0, to.X - from.X, dur) { EasingFunction = ease });
            move.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(0, to.Y - from.Y, dur) { EasingFunction = ease });
        }

        // ---------- Βιτρίνα "Η οικογένεια Opsis" ----------
        private SuiteCatalog? _suiteCatalog;
        private readonly Dictionary<string, LatestRelease?> _suiteLatest = new();
        private readonly Dictionary<string, string> _suiteStatus = new();
        private bool _suiteRefreshing;

        private void OfferSuiteOnce()
        {
            if (_suiteOffered) return;
            _suiteOffered = true;
            SaveSettings();
            ShowSuite();
        }

        internal void ShowSuite()
        {
            if (SuiteOverlay.Visibility == Visibility.Visible) return;
            SuiteTitleText.Text = SuiteStrings.SuiteTitle;
            SuiteSubtitleText.Text = SuiteStrings.SuiteSubtitle;
            SuiteFooterText.Text = SuiteStrings.SuiteFooter;
            SuiteCloseButton.Content = SuiteStrings.SuiteClose;
            _suiteCatalog ??= SuiteCatalogLoader.LoadCachedOrEmbedded();
            RenderSuiteCards(null);
            SuiteOverlay.Visibility = Visibility.Visible;
            SuiteContent.Opacity = 1;
            if (!SimpleAnimations)
                SuiteContent.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(220)));
            SuiteCloseButton.Focus();
            _ = RefreshSuiteAsync();
        }

        private void HideSuite()
        {
            if (SuiteOverlay.Visibility != Visibility.Visible) return;
            FlyIntoHeaderIcon(SuiteOverlay, SuiteContent, () => { });
        }

        private void SuiteClose_Click(object sender, RoutedEventArgs e) => HideSuite();
        private void ClassicGuideMenuItem_Click(object sender, RoutedEventArgs e) => ShowOnboarding(isReplay: true);
        private void ClassicSuiteMenuItem_Click(object sender, RoutedEventArgs e) => ShowSuite();

        /// <summary>Ανανεώνει τον κατάλογο (max 1/24ωρο) και τις τελευταίες εκδόσεις κάθε εφαρμογής, μετά ξαναζωγραφίζει.</summary>
        private async Task RefreshSuiteAsync()
        {
            if (_suiteRefreshing) return;
            _suiteRefreshing = true;
            try
            {
                if (await SuiteCatalogLoader.RefreshAsync(SuiteHttp))
                    _suiteCatalog = SuiteCatalogLoader.LoadCachedOrEmbedded();
                var catalog = _suiteCatalog!;
                var releases = await Task.WhenAll(catalog.Apps.Select(a => ReleaseDownloader.GetLatestAsync(SuiteHttp, a.Repo)));
                for (int i = 0; i < catalog.Apps.Count; i++) _suiteLatest[catalog.Apps[i].Id] = releases[i];
                var states = await Task.Run(() =>
                {
                    var probe = new WindowsInstallProbe();
                    return catalog.Apps.ToDictionary(a => a.Id, a => InstallDetector.Detect(a, probe, _suiteLatest.GetValueOrDefault(a.Id)?.Tag));
                });
                if (SuiteOverlay.Visibility == Visibility.Visible) RenderSuiteCards(states);
            }
            catch { /* καμία ειδοποίηση σφάλματος - η βιτρίνα δουλεύει και με το ενσωματωμένο αντίγραφο */ }
            finally { _suiteRefreshing = false; }
        }

        private static readonly Color[] SuiteColors =
        {
            Color.FromRgb(0xC1, 0x63, 0x3A), Color.FromRgb(0x2E, 0x86, 0xAB), Color.FromRgb(0x8E, 0x5E, 0xB5), Color.FromRgb(0x3F, 0x9E, 0x6B),
            Color.FromRgb(0xD0, 0x8B, 0x1F), Color.FromRgb(0xC1, 0x63, 0x3A), Color.FromRgb(0x4B, 0x6C, 0xB7)
        };

        private void RenderSuiteCards(Dictionary<string, InstallInfo>? states)
        {
            SuiteCardsPanel.Children.Clear();
            var catalog = _suiteCatalog;
            if (catalog == null) return;
            var lang = SuiteStrings.CatalogLang;
            for (int i = 0; i < catalog.Apps.Count; i++)
                SuiteCardsPanel.Children.Add(BuildSuiteCard(catalog.Apps[i], SuiteColors[i % SuiteColors.Length], lang,
                    states != null && states.TryGetValue(catalog.Apps[i].Id, out var st) ? st : null));
        }

        private FrameworkElement BuildSuiteCard(SuiteApp app, Color color, string lang, InstallInfo? info)
        {
            bool isSelf = app.Id == "kombos";
            var latest = _suiteLatest.GetValueOrDefault(app.Id);
            var card = new Border
            {
                Width = 360, Margin = new Thickness(8), Padding = new Thickness(18, 16, 18, 16), CornerRadius = new CornerRadius(10),
                BorderThickness = new Thickness(isSelf ? 1.5 : 1)
            };
            card.SetResourceReference(Border.BackgroundProperty, "PanelBrush");
            card.SetResourceReference(Border.BorderBrushProperty, isSelf ? "ClaudeBrush" : "LineBrush");

            var stack = new StackPanel();
            var head = new Grid();
            head.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            head.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            head.Children.Add(BuildSuiteIcon(app.Name, color));
            var titles = new StackPanel { Margin = new Thickness(14, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(titles, 1);
            var name = new TextBlock { Text = app.Name, FontSize = 17, FontWeight = FontWeights.Bold, FontFamily = new FontFamily("Bahnschrift"), TextWrapping = TextWrapping.Wrap };
            name.SetResourceReference(TextBlock.ForegroundProperty, "InkBrush");
            var tag = new TextBlock { Text = app.TaglineFor(lang), FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 2, 0, 0) };
            tag.SetResourceReference(TextBlock.ForegroundProperty, "InkSoftBrush");
            titles.Children.Add(name); titles.Children.Add(tag);
            head.Children.Add(titles);
            stack.Children.Add(head);

            foreach (var cap in app.CapabilitiesFor(lang))
            {
                var row = new Grid { Margin = new Thickness(0, 5, 0, 0) };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                var dot = new TextBlock { Text = "•", FontSize = 12.5 };
                dot.SetResourceReference(TextBlock.ForegroundProperty, "ClaudeBrush");
                var t = new TextBlock { Text = cap, FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0) };
                t.SetResourceReference(TextBlock.ForegroundProperty, "InkBrush");
                Grid.SetColumn(t, 1);
                row.Children.Add(dot); row.Children.Add(t);
                stack.Children.Add(row);
            }

            // Κατάσταση εγκατάστασης
            string state;
            if (isSelf && (info == null || info.State != InstallState.UpdateAvailable)) state = SuiteStrings.YouAreHere + " · v" + L.AppVersion;
            else if (info == null) state = SuiteStrings.Checking;
            else state = info.State switch
            {
                InstallState.NotInstalled => SuiteStrings.NotInstalled,
                InstallState.UpdateAvailable => SuiteStrings.UpdateAvailable(info.InstalledVersion, latest?.Tag ?? ""),
                _ => SuiteStrings.Installed(info.InstalledVersion)
            };
            if (_suiteStatus.TryGetValue(app.Id, out var busy)) state = busy;
            var stateText = new TextBlock { Text = state, FontSize = 12, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 12, 0, 0) };
            stateText.SetResourceReference(TextBlock.ForegroundProperty, info?.State == InstallState.UpdateAvailable || isSelf ? "ClaudeBrush" : "InkSoftBrush");
            stack.Children.Add(stateText);

            var buttons = new WrapPanel { Margin = new Thickness(0, 10, 0, 0) };
            if (info?.ExePath != null && !isSelf)
                buttons.Children.Add(SuiteButton(SuiteStrings.Open, true, () => OpenSuiteApp(info.ExePath)));
            if (info != null && (info.State == InstallState.NotInstalled && !isSelf || info.State == InstallState.UpdateAvailable))
                buttons.Children.Add(SuiteButton(info.State == InstallState.UpdateAvailable ? SuiteStrings.Update : SuiteStrings.Install, info.ExePath == null,
                    () => _ = InstallSuiteAppAsync(app)));
            buttons.Children.Add(SuiteButton(SuiteStrings.Details, false, () => OpenExternalUrl(app.RepoUrl)));
            stack.Children.Add(buttons);

            card.Child = stack;
            return card;
        }

        private static Button SuiteButton(string text, bool primary, Action click)
        {
            var b = new Button { Content = text, Padding = new Thickness(14, 6, 14, 6), Margin = new Thickness(0, 0, 8, 6), FontSize = 12, Cursor = System.Windows.Input.Cursors.Hand };
            if (primary) { b.SetResourceReference(Control.BackgroundProperty, "ClaudeBrush"); b.Foreground = Brushes.White; b.BorderThickness = new Thickness(0); }
            else
            {
                b.Background = Brushes.Transparent; b.BorderThickness = new Thickness(1);
                b.SetResourceReference(Control.BorderBrushProperty, "LineBrush"); b.SetResourceReference(Control.ForegroundProperty, "InkBrush");
            }
            b.Click += (s, e) => click();
            return b;
        }

        /// <summary>Κινούμενο εικονίδιο κάρτας: κύκλος με το αρχικό γράμμα που "ανασαίνει" + δακτύλιος που περιστρέφεται
        /// (απλό στατικό εικονίδιο σε χαμηλής ισχύος PC / όταν οι animations είναι κλειστές).</summary>
        private static FrameworkElement BuildSuiteIcon(string appName, Color color)
        {
            var host = new Grid { Width = 52, Height = 52, VerticalAlignment = VerticalAlignment.Top };
            var ring = new Ellipse { Width = 52, Height = 52, StrokeThickness = 2, Stroke = new SolidColorBrush(color), StrokeDashArray = new DoubleCollection { 3, 3 }, Opacity = 0.55, RenderTransformOrigin = new Point(0.5, 0.5) };
            var rot = new RotateTransform(0);
            ring.RenderTransform = rot;
            var disc = new Ellipse { Width = 40, Height = 40, Fill = new SolidColorBrush(color), RenderTransformOrigin = new Point(0.5, 0.5) };
            var pulse = new ScaleTransform(1, 1);
            disc.RenderTransform = pulse;
            var letter = new TextBlock
            {
                Text = new string(appName.Replace("Opsis ", "").Take(1).ToArray()).ToUpperInvariant(), Foreground = Brushes.White, FontSize = 20,
                FontWeight = FontWeights.Bold, FontFamily = new FontFamily("Bahnschrift"), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
            };
            host.Children.Add(ring); host.Children.Add(disc); host.Children.Add(letter);
            if (!SimpleAnimations)
            {
                rot.BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation(0, 360, TimeSpan.FromSeconds(9)) { RepeatBehavior = RepeatBehavior.Forever });
                var p = new DoubleAnimation(0.94, 1.06, TimeSpan.FromSeconds(1.6)) { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever, EasingFunction = new SineEase() };
                pulse.BeginAnimation(ScaleTransform.ScaleXProperty, p);
                pulse.BeginAnimation(ScaleTransform.ScaleYProperty, p);
            }
            return host;
        }

        private void OpenSuiteApp(string exe)
        {
            try { Process.Start(new ProcessStartInfo(exe) { UseShellExecute = true, WorkingDirectory = System.IO.Path.GetDirectoryName(exe) ?? "" }); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Kombos"); }
        }

        /// <summary>Ανοίγει μόνο https προς GitHub - ποτέ αυθαίρετο URL από εξωτερικά δεδομένα.</summary>
        private static void OpenExternalUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var u) || !SuiteCatalog.IsAllowedHost(u)) return;
            try { Process.Start(new ProcessStartInfo(u.AbsoluteUri) { UseShellExecute = true }); } catch { }
        }

        /// <summary>Λήψη installer: ΜΟΝΟ μετά από επιβεβαίωση (όνομα/μέγεθος/πηγή), με έλεγχο SHA-256 που ΑΠΟΡΡΙΠΤΕΙ
        /// σε ασυμφωνία. Ο installer δεν εκτελείται ποτέ αυτόματα· αν δεν υπάρχει δημοσιευμένο hash, ανοίγει η σελίδα του release.</summary>
        private async Task InstallSuiteAppAsync(SuiteApp app)
        {
            var releasePage = $"{app.RepoUrl}/releases/latest";
            try
            {
                var rel = _suiteLatest.GetValueOrDefault(app.Id) ?? await ReleaseDownloader.GetLatestAsync(SuiteHttp, app.Repo);
                var asset = rel == null ? null : ReleaseDownloader.PickInstaller(rel, app.InstallerAsset);
                if (rel == null || asset == null) { OpenExternalUrl(releasePage); return; }

                var hash = await ReleaseDownloader.ResolveExpectedHashAsync(SuiteHttp, rel, asset, app.ChecksumAsset);
                if (hash == null)
                {
                    if (MessageBox.Show(this, SuiteStrings.NoChecksum, app.Name, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                        OpenExternalUrl(rel.HtmlUrl.Length > 0 ? rel.HtmlUrl : releasePage);
                    return;
                }

                var size = asset.Size > 0 ? $"{asset.Size / 1048576.0:0.0} MB" : "?";
                var source = new Uri(asset.Url).Host + " / " + SuiteCatalog.Owner + "/" + app.Repo;
                if (MessageBox.Show(this, SuiteStrings.ConfirmDownload(asset.Name, size, source), app.Name, MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;

                var downloads = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                var progress = new Progress<double>(p => { _suiteStatus[app.Id] = SuiteStrings.Downloading((int)(p * 100)); RenderSuiteCards(null); });
                string path;
                try { path = await ReleaseDownloader.DownloadVerifiedAsync(SuiteHttp, asset, hash, downloads, progress); }
                catch (ChecksumMismatchException)
                {
                    _suiteStatus.Remove(app.Id);
                    MessageBox.Show(this, SuiteStrings.ChecksumRefused, app.Name, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                _suiteStatus.Remove(app.Id);
                MessageBox.Show(this, SuiteStrings.DownloadVerified(path), app.Name, MessageBoxButton.OK, MessageBoxImage.Information);
                try { Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{path}\"") { UseShellExecute = true }); } catch { }
            }
            catch (Exception ex)
            {
                _suiteStatus.Remove(app.Id);
                MessageBox.Show(this, SuiteStrings.DownloadFailed(ex.Message), app.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally { _ = RefreshSuiteAsync(); }
        }

        // ---------- Ειδοποίηση νέας έκδοσης ----------
        private DispatcherTimer? _updateTimer;
        private UpdateNotifier? _updateNotifier;
        private ReleaseInfo? _pendingUpdate;

        /// <summary>Πρώτος έλεγχος ~30 s μετά την εκκίνηση, μετά κάθε 24 ώρες όσο τρέχει η εφαρμογή. Δεν μπλοκάρει ποτέ.</summary>
        private void StartUpdateWatcher()
        {
            var repo = string.IsNullOrWhiteSpace(_updateCheckRepo) ? SuiteCatalog.Owner + "/Kombos" : _updateCheckRepo.Trim();
            // Δοκιμαστικοί διακόπτες (μόνο μεταβλητές περιβάλλοντος, ποτέ UI): ψεύτικη "τρέχουσα" έκδοση και σύντομος πρώτος έλεγχος.
            var testCurrent = Environment.GetEnvironmentVariable("KOMBOS_TEST_CURRENT_VERSION");
            _updateNotifier = new UpdateNotifier(SuiteHttp, new UpdateNoticeStore(), repo, string.IsNullOrWhiteSpace(testCurrent) ? L.AppVersion : testCurrent);
            _updateTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(string.IsNullOrWhiteSpace(testCurrent) ? 30 : 3) };
            _updateTimer.Tick += async (s, e) =>
            {
                _updateTimer!.Interval = TimeSpan.FromHours(24);
                if (!_notifyNewVersions) return;
                var info = await _updateNotifier!.CheckAsync();
                if (info == null) return;
                _pendingUpdate = info;
                _updateNotifier.MarkNotified(info.Version);
                UpdateToastText.Text = SuiteStrings.UpdateToast(info.Version);
                UpdateToastOpenButton.Content = SuiteStrings.WhatsNew;
                UpdateToastLaterButton.Content = SuiteStrings.Later;
                UpdateToast.Visibility = Visibility.Visible;
                if (!IsActive || !IsVisible)
                    _trayIcon?.ShowNotification("Κόμβος", SuiteStrings.BalloonText(info.Version), () => Dispatcher.Invoke(() => OpenExternalUrl(info.HtmlUrl)));
            };
            _updateTimer.Start();
        }

        /// <summary>Δοκιμαστικός διακόπτης KOMBOS_TEST_OPEN=settings|suite|guide: ανοίγει το αντίστοιχο παράθυρο στην εκκίνηση
        /// (για αυτοματοποιημένα screenshots). Χωρίς τη μεταβλητή δεν κάνει τίποτα.</summary>
        private void RunTestOpenHook()
        {
            var what = Environment.GetEnvironmentVariable("KOMBOS_TEST_OPEN");
            if (string.IsNullOrWhiteSpace(what)) return;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                switch (what.Trim().ToLowerInvariant())
                {
                    case "settings": OpenSettingsWindow("behavior"); break;
                    case "settings-about": OpenSettingsWindow("about"); break;
                    case "suite": ShowSuite(); break;
                    case "guide": ShowOnboarding(isReplay: true); break;
                }
            }), DispatcherPriority.ApplicationIdle);
        }

        private void UpdateToastOpen_Click(object sender, RoutedEventArgs e)
        {
            if (_pendingUpdate != null) OpenExternalUrl(_pendingUpdate.HtmlUrl);
            UpdateToast.Visibility = Visibility.Collapsed;
        }

        private void UpdateToastLater_Click(object sender, RoutedEventArgs e) => UpdateToast.Visibility = Visibility.Collapsed;
    }
}
