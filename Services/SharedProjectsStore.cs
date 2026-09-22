using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace Kombos.Services
{
    public class SharedProjectMeta
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Status { get; set; } = "Pending";
        public string OriginMachine { get; set; } = "";
        public DateTime LastUpdatedUtc { get; set; }
        public int FileCount { get; set; }
    }

    /// <summary>ΣΗΜΑΝΤΙΚΟ: δεν υπάρχει δημόσιο API για τα "Projects" του claude.ai (ή αντίστοιχα προϊόντα άλλων
    /// AI) - Kombos ΔΕΝ μπορεί να διαβάσει/συγχρονίσει πραγματικά projects από εκεί, και δεν επιχειρεί καμία
    /// μη-επίσημη προσέγγιση (scraping/session cookies) γιατί θα ήταν εύθραυστο και ενδεχομένως αντίθετο με
    /// τους όρους χρήσης. Αυτό εδώ είναι κάτι πραγματικό και εφικτό: τα ΔΙΚΑ ΤΟΥ "Project Memory" projects του
    /// Kombos (v3.6.0), δημοσιευμένα σαν ZIP snapshot μέσα στον ήδη υπάρχοντα φάκελο συγχρονισμού συζητήσεων
    /// (v2.9.0, OneDrive/Dropbox/δίκτυο) - κάθε υπολογιστής με τον ΙΔΙΟ φάκελο βλέπει τη λίστα, την κατάσταση
    /// (Pending/InProgress/Completed) και μπορεί να κατεβάσει το ZIP τοπικά.</summary>
    public static class SharedProjectsStore
    {
        private static string ProjectsRoot(string syncFolder) => Path.Combine(syncFolder, "kombos-shared-projects");

        public static string MakeId(string projectRootPath) =>
            Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(projectRootPath.ToLowerInvariant())))[..16];

        /// <summary>Φτιάχνει ένα ZIP snapshot του project (μόνο τα ίδια αρχεία που ήδη παρακολουθεί το τοπικό
        /// ευρετήριο - βλ. ProjectMemoryStore) και το τοποθετεί στον φάκελο συγχρονισμού μαζί με τα metadata
        /// του (όνομα, κατάσταση, υπολογιστής προέλευσης, ημερομηνία).</summary>
        public static SharedProjectMeta PublishSnapshot(string syncFolder, string projectRootPath, string displayName, string status)
        {
            var id = MakeId(projectRootPath);
            var dir = Path.Combine(ProjectsRoot(syncFolder), id);
            Directory.CreateDirectory(dir);

            var zipPath = Path.Combine(dir, "snapshot.zip");
            if (File.Exists(zipPath)) File.Delete(zipPath);

            var index = ProjectMemoryStore.BuildOrUpdateIndex(projectRootPath);
            using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                foreach (var file in index.Files)
                {
                    var fullPath = Path.Combine(projectRootPath, file.RelativePath);
                    if (File.Exists(fullPath)) zip.CreateEntryFromFile(fullPath, file.RelativePath.Replace('\\', '/'));
                }
            }

            var meta = new SharedProjectMeta
            {
                Id = id,
                Name = displayName,
                Status = status,
                OriginMachine = Environment.MachineName,
                LastUpdatedUtc = DateTime.UtcNow,
                FileCount = index.Files.Count
            };
            File.WriteAllText(Path.Combine(dir, "meta.json"), JsonSerializer.Serialize(meta));
            return meta;
        }

        public static List<SharedProjectMeta> ListShared(string syncFolder)
        {
            var result = new List<SharedProjectMeta>();
            try
            {
                var root = ProjectsRoot(syncFolder);
                if (!Directory.Exists(root)) return result;
                foreach (var dir in Directory.GetDirectories(root))
                {
                    var metaPath = Path.Combine(dir, "meta.json");
                    if (!File.Exists(metaPath)) continue;
                    try
                    {
                        var meta = JsonSerializer.Deserialize<SharedProjectMeta>(File.ReadAllText(metaPath));
                        if (meta != null) result.Add(meta);
                    }
                    catch { /* κατεστραμμένο/ημιτελές meta.json από ταυτόχρονη εγγραφή - παράβλεψη */ }
                }
            }
            catch { /* π.χ. ο φάκελος συγχρονισμού δεν είναι προσβάσιμος αυτή τη στιγμή (offline δίσκος) */ }
            return result.OrderByDescending(m => m.LastUpdatedUtc).ToList();
        }

        public static void UpdateStatus(string syncFolder, string id, string status)
        {
            var metaPath = Path.Combine(ProjectsRoot(syncFolder), id, "meta.json");
            if (!File.Exists(metaPath)) return;
            try
            {
                var meta = JsonSerializer.Deserialize<SharedProjectMeta>(File.ReadAllText(metaPath));
                if (meta == null) return;
                meta.Status = status;
                meta.LastUpdatedUtc = DateTime.UtcNow;
                File.WriteAllText(metaPath, JsonSerializer.Serialize(meta));
            }
            catch { /* βλ. σχόλιο πιο πάνω */ }
        }

        public static void DownloadSnapshot(string syncFolder, string id, string destZipPath)
        {
            var zipPath = Path.Combine(ProjectsRoot(syncFolder), id, "snapshot.zip");
            File.Copy(zipPath, destZipPath, overwrite: true);
        }

        public static void RemoveShared(string syncFolder, string id)
        {
            var dir = Path.Combine(ProjectsRoot(syncFolder), id);
            if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
        }
    }
}
