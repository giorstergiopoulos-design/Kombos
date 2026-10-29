using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Kombos.Localization;
using Kombos.Services;
using Xunit;

namespace Kombos.Tests
{
    public class SuiteIconTests
    {
        [Fact]
        public void EveryCatalogueAppResolvesToAnEmbeddedIcon()
        {
            var cat = SuiteCatalogLoader.LoadEmbedded();
            foreach (var app in cat.Apps)
            {
                Assert.True(SuiteIcons.Has(app.Id), $"no embedded icon for {app.Id}");
                var bmp = SuiteIcons.Load(app.Id, 40);
                Assert.NotNull(bmp);
                Assert.True(bmp!.PixelWidth >= 40, $"{app.Id}: only {bmp.PixelWidth}px available for 40px");
            }
        }

        [Fact]
        public void HigherDpiPicksALargerFrame_AndUnknownAppFallsBack()
        {
            var small = SuiteIcons.Load("opsis", 40)!;
            var big = SuiteIcons.Load("opsis", 80)!;      // 200 % DPI
            Assert.True(big.PixelWidth >= 80);
            Assert.True(big.PixelWidth >= small.PixelWidth);
            Assert.False(SuiteIcons.Has("future-app"));
            Assert.Null(SuiteIcons.Load("future-app", 40));
        }
    }

    public class SuiteLocalisationTests
    {
        /// <summary>Κάθε νέο string της βιτρίνας/οδηγού/ειδοποίησης υπάρχει στις 4 γλώσσες: όχι κενό, καμία "αγγλική" αντιγραφή
        /// στα EL/FR/DE, και τα placeholders (αριθμοί/ονόματα) περνούν σε κάθε γλώσσα.</summary>
        [Fact]
        public void AllSuiteStringsExistInAllFourLanguages()
        {
            var original = L.Current;
            try
            {
                var props = typeof(SuiteStrings).GetProperties(BindingFlags.Public | BindingFlags.Static).Where(p => p.PropertyType == typeof(string) && p.Name != "CatalogLang").ToList();
                var methods = typeof(SuiteStrings).GetMethods(BindingFlags.Public | BindingFlags.Static).Where(m => m.ReturnType == typeof(string) && !m.IsSpecialName).ToList();
                Assert.True(props.Count >= 15 && methods.Count >= 6);

                string Value(Lang lang, Func<string> f) { L.Current = lang; return f(); }
                foreach (var p in props)
                {
                    var en = Value(Lang.EN, () => (string)p.GetValue(null)!);
                    Assert.False(string.IsNullOrWhiteSpace(en), p.Name);
                    foreach (var lang in new[] { Lang.EL, Lang.FR, Lang.DE })
                    {
                        var v = Value(lang, () => (string)p.GetValue(null)!);
                        Assert.False(string.IsNullOrWhiteSpace(v), $"{p.Name}/{lang} empty");
                        bool sameWordOk = p.Name == "SuiteButton" || (p.Name == "Details" && lang == Lang.DE); // "Details" ist auch deutsch
                        if (!sameWordOk) Assert.NotEqual(en, v);
                    }
                }
                foreach (var m in methods)
                {
                    var args = m.GetParameters().Select(pr => pr.ParameterType == typeof(int) ? (object)42 : "<<ARG>>").ToArray();
                    foreach (var lang in new[] { Lang.EL, Lang.EN, Lang.FR, Lang.DE })
                    {
                        var v = Value(lang, () => (string)m.Invoke(null, args)!);
                        Assert.False(string.IsNullOrWhiteSpace(v), $"{m.Name}/{lang}");
                        Assert.True(v.Contains("<<ARG>>") || v.Contains("42") || args.Length == 0, $"{m.Name}/{lang} lost its placeholder");
                    }
                }
            }
            finally { L.Current = original; }
        }

        [Fact]
        public void CatalogueHasAllFourLanguagesForEveryApp()
        {
            foreach (var a in SuiteCatalogLoader.LoadEmbedded().Apps)
                foreach (var l in new[] { "el", "en", "fr", "de" })
                {
                    Assert.True(a.Tagline.ContainsKey(l), $"{a.Id} tagline {l}");
                    Assert.True(a.Capabilities.TryGetValue(l, out var c) && c.Length == a.Capabilities["en"].Length, $"{a.Id} capabilities {l}");
                }
        }
    }

    public class DownloadClientTests
    {
        [Fact]
        public void DownloadClientHasNoTotalTimeout_SoLargeInstallersAreNotCutOff() =>
            Assert.Equal(Timeout.InfiniteTimeSpan, ReleaseDownloader.CreateDownloadClient().Timeout);
    }
}
