using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Kombos.Services
{
    /// <summary>Μία κοινή, μόνιμη λίστα σύντομων γεγονότων/προτιμήσεων, αποθηκευμένη τοπικά και ενέσιμη
    /// στο system prompt ΚΑΘΕ agent (Claude και κάθε OpenAI-compatible εργαλείο) - αυτό είναι η πραγματική
    /// "συλλογική μνήμη" του Κόμβου ως nexus: κάτι που μαθαίνει ένας agent το ξέρουν όλοι από εκεί και πέρα.</summary>
    public static class HiveMemoryStore
    {
        public static string FilePath => Path.Combine(
            AppPaths.Root, "hive-memory.json");

        public static List<string> Load()
        {
            try
            {
                if (!File.Exists(FilePath)) return new List<string>();
                var json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
            catch { return new List<string>(); }
        }

        public static void Save(List<string> facts)
        {
            try
            {
                var dir = Path.GetDirectoryName(FilePath)!;
                Directory.CreateDirectory(dir);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(facts));
            }
            catch { /* ένα αποτυχημένο save μνήμης δεν πρέπει ποτέ να ρίξει την εφαρμογή */ }
        }

        public static List<string> Add(List<string> facts, string fact)
        {
            fact = fact.Trim();
            if (string.IsNullOrEmpty(fact)) return facts;
            var updated = facts.Where(f => !string.Equals(f, fact, StringComparison.OrdinalIgnoreCase)).Append(fact).ToList();
            Save(updated);
            return updated;
        }
    }
}
