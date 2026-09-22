using System;
using System.Collections.Generic;
using System.IO;
using Kombos.Services;
using Xunit;

namespace Kombos.Tests
{
    /// <summary>SettingsStore διαβάζει/γράφει πάντα το ΙΔΙΟ σταθερό μονοπάτι (%AppData%\Kombos\settings.json) -
    /// τα tests παίρνουν αντίγραφο ασφαλείας του υπαρκτού αρχείου πριν τρέξουν και το επαναφέρουν στο τέλος,
    /// ώστε να μην πειράξουν πραγματικά αποθηκευμένα κλειδιά/ρυθμίσεις του χρήστη σε αυτό το μηχάνημα.</summary>
    public class SettingsStoreTests : IDisposable
    {
        private readonly string _filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Kombos", "settings.json");
        private readonly string? _backup;

        public SettingsStoreTests()
        {
            _backup = File.Exists(_filePath) ? File.ReadAllText(_filePath) : null;
        }

        public void Dispose()
        {
            if (_backup != null) File.WriteAllText(_filePath, _backup);
            else if (File.Exists(_filePath)) File.Delete(_filePath);
        }

        [Fact]
        public void Save_ThenLoad_RoundTripsPlainFields()
        {
            var settings = new AppSettings
            {
                Language = "EN",
                IsDark = true,
                CentralAgent = "ChatGPT",
                CustomInstructions = "keep replies short"
            };
            SettingsStore.Save(settings);

            var loaded = SettingsStore.Load();
            Assert.Equal("EN", loaded.Language);
            Assert.True(loaded.IsDark);
            Assert.Equal("ChatGPT", loaded.CentralAgent);
            Assert.Equal("keep replies short", loaded.CustomInstructions);
        }

        [Fact]
        public void Save_EncryptsApiKeysOnDisk_NotPlaintext()
        {
            var settings = new AppSettings { ApiKeys = new Dictionary<string, string> { ["Claude"] = "sk-ant-super-secret" } };
            SettingsStore.Save(settings);

            var rawJson = File.ReadAllText(_filePath);
            Assert.DoesNotContain("sk-ant-super-secret", rawJson);
        }

        [Fact]
        public void Save_ThenLoad_RoundTripsApiKeysDecrypted()
        {
            var settings = new AppSettings { ApiKeys = new Dictionary<string, string> { ["Claude"] = "sk-ant-super-secret" } };
            SettingsStore.Save(settings);

            var loaded = SettingsStore.Load();
            Assert.Equal("sk-ant-super-secret", loaded.ApiKeys["Claude"]);
        }

        [Fact]
        public void Load_OnLegacyPlaintextApiKey_MigratesInsteadOfLosingIt()
        {
            // Προσομοιώνει ένα settings.json γραμμένο από έκδοση πριν το v2.3.0 (πριν το DPAPI) - το
            // κλειδί είναι ακόμα σε καθαρό κείμενο μέσα στο ApiKeys dictionary.
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, "{\"ApiKeys\":{\"Claude\":\"legacy-plaintext-key\"}}");

            var loaded = SettingsStore.Load();
            Assert.Equal("legacy-plaintext-key", loaded.ApiKeys["Claude"]);
        }

        [Fact]
        public void Load_WhenFileMissing_ReturnsDefaults()
        {
            if (File.Exists(_filePath)) File.Delete(_filePath);
            var loaded = SettingsStore.Load();
            Assert.Equal("EL", loaded.Language);
            Assert.False(loaded.IsDark);
        }
    }
}
