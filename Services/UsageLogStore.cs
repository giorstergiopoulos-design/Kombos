using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using Kombos.Models;

namespace Kombos.Services
{
    /// <summary>Τοπικό ιστορικό χρήσης tokens ανά συνεδρία (v4.5.0, analytics dashboard) - πάντα τοπικά
    /// (%AppData%\Kombos\usage-log.json), καμία τηλεμετρία/αποστολή πουθενά. Κρατάει μόνο τις τελευταίες
    /// 200 συνεδρίες ώστε το αρχείο να μη μεγαλώνει επ' αόριστον.</summary>
    public static class UsageLogStore
    {
        private const int MaxEntries = 200;

        private static string FilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Kombos", "usage-log.json");

        public static List<UsageLogEntry> Load()
        {
            try
            {
                if (!File.Exists(FilePath)) return new List<UsageLogEntry>();
                return JsonSerializer.Deserialize<List<UsageLogEntry>>(File.ReadAllText(FilePath)) ?? new List<UsageLogEntry>();
            }
            catch { return new List<UsageLogEntry>(); }
        }

        public static void Append(UsageLogEntry entry)
        {
            try
            {
                var entries = Load();
                entries.Add(entry);
                if (entries.Count > MaxEntries) entries = entries.Skip(entries.Count - MaxEntries).ToList();
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(entries));
            }
            catch { /* best-effort, ίδιο με τα υπόλοιπα stores */ }
        }
    }
}
