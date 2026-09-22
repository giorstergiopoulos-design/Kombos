using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace Kombos.Services
{
    public class ProjectFileEntry
    {
        public string RelativePath { get; set; } = "";
        public string Hash { get; set; } = "";
        public long LastModifiedTicks { get; set; }
    }

    public class ProjectIndex
    {
        public string RootPath { get; set; } = "";
        public List<ProjectFileEntry> Files { get; set; } = new();
        public DateTime LastIndexedUtc { get; set; }
    }

    /// <summary>Τοπικό ευρετήριο ενός project-φακέλου - δέντρο αρχείων + hash/ημερομηνία ανά αρχείο, ΟΧΙ
    /// περιεχόμενο. Αντί ο agent να διαβάζει ΟΛΟΚΛΗΡΟ το project σε κάθε μήνυμα (ό,τι έκανε το παλιό
    /// "εισαγωγή φακέλου"), βλέπει μόνο το δέντρο (φτηνό σε tokens) και το Kombos φέρνει στοχευμένα μόνο το
    /// συγκεκριμένο αρχείο που χρειάζεται (βλ. MainWindow.ReadProjectFileAsContext). Re-index μετά από μια
    /// μικρή αλλαγή σαρώνει ΜΟΝΟ τα αρχεία που άλλαξαν (σύγκριση hash), όχι όλο το δέντρο από την αρχή.</summary>
    public static class ProjectMemoryStore
    {
        private static readonly string[] ImportableExtensions =
        {
            ".txt", ".md", ".json", ".yaml", ".yml", ".xml", ".csv",
            ".cs", ".xaml", ".py", ".js", ".jsx", ".ts", ".tsx", ".html", ".css",
            ".java", ".kt", ".go", ".rb", ".php", ".c", ".cpp", ".h", ".hpp", ".sql", ".sh", ".csproj", ".sln"
        };
        private static readonly string[] ExcludedDirNames =
        {
            "bin", "obj", "node_modules", ".git", ".vs", "dist", "build", "packages", ".idea"
        };

        private static string IndexRoot => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Kombos", "projects");

        private static string IndexFilePath(string projectRootPath)
        {
            var hash = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(projectRootPath.ToLowerInvariant())))[..16];
            return Path.Combine(IndexRoot, hash, "index.json");
        }

        private static IEnumerable<string> EnumerateProjectFiles(string rootPath) =>
            Directory.EnumerateFiles(rootPath, "*", SearchOption.AllDirectories)
                .Where(f => ImportableExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .Where(f => !f.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                               .Any(seg => ExcludedDirNames.Contains(seg, StringComparer.OrdinalIgnoreCase)));

        /// <summary>Χτίζει ή ενημερώνει το ευρετήριο ενός project - rehash μόνο για αρχεία των οποίων άλλαξε
        /// η ημερομηνία τροποποίησης από την τελευταία φορά.</summary>
        public static ProjectIndex BuildOrUpdateIndex(string rootPath)
        {
            var existing = LoadIndex(rootPath);
            var existingByPath = existing?.Files.ToDictionary(f => f.RelativePath) ?? new Dictionary<string, ProjectFileEntry>();

            var index = new ProjectIndex { RootPath = rootPath, LastIndexedUtc = DateTime.UtcNow };
            foreach (var file in EnumerateProjectFiles(rootPath))
            {
                var relPath = Path.GetRelativePath(rootPath, file);
                var mtimeTicks = File.GetLastWriteTimeUtc(file).Ticks;

                if (existingByPath.TryGetValue(relPath, out var prior) && prior.LastModifiedTicks == mtimeTicks)
                {
                    index.Files.Add(prior);
                    continue;
                }

                string hash;
                try { hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file)))[..16]; }
                catch { continue; }
                index.Files.Add(new ProjectFileEntry { RelativePath = relPath, Hash = hash, LastModifiedTicks = mtimeTicks });
            }

            SaveIndex(index);
            return index;
        }

        public static ProjectIndex? LoadIndex(string rootPath)
        {
            try
            {
                var path = IndexFilePath(rootPath);
                if (!File.Exists(path)) return null;
                return JsonSerializer.Deserialize<ProjectIndex>(File.ReadAllText(path));
            }
            catch { return null; }
        }

        private static void SaveIndex(ProjectIndex index)
        {
            try
            {
                var path = IndexFilePath(index.RootPath);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, JsonSerializer.Serialize(index));
            }
            catch { /* ένα αποτυχημένο save ευρετηρίου δεν πρέπει να ρίξει την εφαρμογή */ }
        }

        /// <summary>Το δέντρο αρχείων σε συμπαγή μορφή κειμένου - αυτό (όχι το περιεχόμενο) στέλνεται στον
        /// agent ώστε να ξέρει τι υπάρχει στο project και να ζητήσει στοχευμένα το συγκεκριμένο αρχείο.</summary>
        public static string BuildTreeText(ProjectIndex index) =>
            string.Join("\n", index.Files.Select(f => f.RelativePath).OrderBy(p => p, StringComparer.OrdinalIgnoreCase));
    }
}
