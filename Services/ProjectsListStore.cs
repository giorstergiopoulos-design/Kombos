using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

using Kombos.Models;

namespace Kombos.Services
{
    /// <summary>Αποθηκεύει τη λίστα ονομασμένων Projects (v3.9.0, στυλ Claude Desktop) - ξεχωριστό αρχείο από
    /// τις ίδιες τις συζητήσεις (conversations.json), ίδιο μοτίβο με το ConversationStore ώστε να ακολουθεί
    /// τον ίδιο προαιρετικό φάκελο συγχρονισμού όταν έχει οριστεί.</summary>
    public static class ProjectsListStore
    {
        private static string ResolveFilePath(string? customFolder)
        {
            var folder = string.IsNullOrWhiteSpace(customFolder)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Kombos")
                : customFolder;
            return Path.Combine(folder, "projects-list.json");
        }

        public static List<KombosProject> Load(string? customFolder = null)
        {
            try
            {
                var path = ResolveFilePath(customFolder);
                if (!File.Exists(path)) return new List<KombosProject>();
                return JsonSerializer.Deserialize<List<KombosProject>>(File.ReadAllText(path)) ?? new List<KombosProject>();
            }
            catch { return new List<KombosProject>(); }
        }

        public static void Save(List<KombosProject> projects, string? customFolder = null)
        {
            try
            {
                var path = ResolveFilePath(customFolder);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, JsonSerializer.Serialize(projects));
            }
            catch { /* best-effort, ίδιο με το ConversationStore */ }
        }
    }
}
