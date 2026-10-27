using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

using Kombos.Models;

namespace Kombos.Services
{
    /// <summary>Αποθηκεύει τη λίστα συζητήσεων του χρήστη σε %AppData%\Kombos\conversations.json από
    /// προεπιλογή, ή σε έναν φάκελο της επιλογής του χρήστη (π.χ. OneDrive/Dropbox/δικτυακό δίσκο) - αυτό
    /// είναι το μοντέλο "συνεργασίας ανάμεσα σε υπολογιστές" του Κόμβου: μοιράσου το ΙΔΙΟ αρχείο
    /// conversations.json μέσω ενός φακέλου συγχρονισμένου εξωτερικά, χωρίς το Κόμβος να χρειάζεται δικό του
    /// server/backend. Δεν γίνεται merge ταυτόχρονων αλλαγών - το τελευταίο Save() που γράφεται κερδίζει,
    /// όπως ακριβώς και κάθε άλλο αρχείο μέσα σε έναν φάκελο συγχρονισμού cloud.</summary>
    public static class ConversationStore
    {
        private static readonly string DefaultFilePath = Path.Combine(
            AppPaths.Root, "conversations.json");

        private static string ResolveFilePath(string? customFolder) =>
            string.IsNullOrWhiteSpace(customFolder) ? DefaultFilePath : Path.Combine(customFolder, "conversations.json");

        public static List<Conversation> Load(string? customFolder = null)
        {
            try
            {
                var filePath = ResolveFilePath(customFolder);
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    var list = JsonSerializer.Deserialize<List<Conversation>>(json);
                    if (list != null) return list;
                }
            }
            catch { /* corrupt/missing file - start with an empty list */ }
            return new List<Conversation>();
        }

        public static void Save(List<Conversation> conversations, string? customFolder = null)
        {
            try
            {
                var filePath = ResolveFilePath(customFolder);
                var dir = Path.GetDirectoryName(filePath)!;
                Directory.CreateDirectory(dir);
                var json = JsonSerializer.Serialize(conversations, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
            catch { /* best-effort - not critical if it fails */ }
        }
    }
}
