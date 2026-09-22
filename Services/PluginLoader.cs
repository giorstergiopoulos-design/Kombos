using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

using Kombos.Plugins;

namespace Kombos.Services
{
    public class LoadedPlugin
    {
        public string Name { get; init; } = "";
        public string Version { get; init; } = "";
        public string FolderName { get; init; } = "";
        public IKombosPlugin Instance { get; init; } = null!;
    }

    /// <summary>Ίδιο μοτίβο με το επίσημο tutorial της Microsoft για .NET plugins: όταν ο δικός μας resolver
    /// δεν βρει το assembly (π.χ. είναι το ίδιο το Kombos.dll - ο κεντρικός host, όχι dependency του plugin),
    /// το Load() επιστρέφει null και το runtime πέφτει αυτόματα πίσω στο Default context, δηλαδή ΞΑΝΑΧΡΗΣΙΜΟΠΟΙΕΙ
    /// το ήδη φορτωμένο Kombos.dll του host αντί να φορτώσει δεύτερο, ασύμβατο αντίγραφο (κρίσιμο - αλλιώς το
    /// IKombosPlugin του plugin δεν θα ταίριαζε τύπο με το IKombosPlugin του host).</summary>
    internal class PluginLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;

        public PluginLoadContext(string name, string pluginDllPath) : base(name, isCollectible: true)
        {
            _resolver = new AssemblyDependencyResolver(pluginDllPath);
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var path = _resolver.ResolveAssemblyToPath(assemblyName);
            return path != null ? LoadFromAssemblyPath(path) : null;
        }
    }

    /// <summary>Φορτώνει plugins από %AppData%\Kombos\plugins\&lt;φάκελος&gt;\*.dll, το καθένα στο δικό του
    /// AssemblyLoadContext (isolation ανά plugin, resolver για τα δικά του dependencies) - ένα σπασμένο ή
    /// κακογραμμένο plugin ΔΕΝ πρέπει ποτέ να ρίξει το ίδιο το Kombos, οπότε κάθε βήμα φόρτωσης είναι
    /// τυλιγμένο σε try/catch με καταγραφή αντί για throw.</summary>
    public static class PluginLoader
    {
        public static string PluginsRoot => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Kombos", "plugins");

        /// <summary>Επίσημα plugins που έρχονται ΜΑΖΙ με την εγκατάσταση του Kombos (φάκελος PluginsBundled
        /// δίπλα στο .exe) - η πηγή της γκαλερί "Λήψη &amp; Ενσωμάτωση" στις Ρυθμίσεις. "Εγκατάσταση" απλά
        /// αντιγράφει από εδώ στο PluginsRoot· δεν χρειάζεται δίκτυο, άρα κανένα ζήτημα εμπιστοσύνης σε
        /// κώδικα από το διαδίκτυο.</summary>
        public static string BundledPluginsRoot => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PluginsBundled");

        /// <summary>Ονόματα φακέλων μέσα στο BundledPluginsRoot που περιέχουν έστω ένα .dll.</summary>
        public static List<string> GetBundledPluginFolders()
        {
            try
            {
                if (!Directory.Exists(BundledPluginsRoot)) return new List<string>();
                return Directory.GetDirectories(BundledPluginsRoot)
                    .Where(d => Directory.GetFiles(d, "*.dll").Length > 0)
                    .Select(d => Path.GetFileName(d)!)
                    .ToList();
            }
            catch { return new List<string>(); }
        }

        /// <summary>Αντιγράφει ένα bundled plugin στο ενεργό PluginsRoot - ενεργοποιείται μετά από
        /// επανεκκίνηση (ο ήδη τρέχων PluginLoader.LoadAll έχει ήδη τρέξει μία φορά στην εκκίνηση).</summary>
        public static void InstallBundledPlugin(string folderName)
        {
            var source = Path.Combine(BundledPluginsRoot, folderName);
            var dest = Path.Combine(PluginsRoot, folderName);
            Directory.CreateDirectory(dest);
            foreach (var file in Directory.GetFiles(source, "*.dll"))
                File.Copy(file, Path.Combine(dest, Path.GetFileName(file)), overwrite: true);
        }

        /// <summary>Nexus Marketplace (v4.5.0) - γενική εγκατάσταση: αντιγράφει τα δοσμένα .dll σε νέο
        /// φάκελο κάτω από το PluginsRoot, ίδιο αποτέλεσμα με το InstallBundledPlugin αλλά για ΟΠΟΙΑΔΗΠΟΤΕ
        /// πηγή .dll (τοπικός φάκελος ή κατεβασμένο Git repo) - βλ. Services/PluginMarketplace.cs.</summary>
        public static void InstallFromDllFiles(string pluginName, IEnumerable<string> dllPaths)
        {
            var dest = Path.Combine(PluginsRoot, pluginName);
            Directory.CreateDirectory(dest);
            foreach (var file in dllPaths)
                File.Copy(file, Path.Combine(dest, Path.GetFileName(file)), overwrite: true);
        }

        /// <summary>Πλήρης αφαίρεση ενός εγκατεστημένου plugin (bundled ή τρίτου) από το PluginsRoot.</summary>
        public static void UninstallPlugin(string folderName)
        {
            var dir = Path.Combine(PluginsRoot, folderName);
            if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
        }

        public static List<LoadedPlugin> LoadAll(IKombosHost host, ISet<string> disabledFolders)
        {
            var loaded = new List<LoadedPlugin>();
            try
            {
                if (!Directory.Exists(PluginsRoot)) return loaded;

                foreach (var dir in Directory.GetDirectories(PluginsRoot))
                {
                    var folderName = Path.GetFileName(dir);
                    if (disabledFolders.Contains(folderName)) continue;

                    var dll = Directory.GetFiles(dir, "*.dll").FirstOrDefault();
                    if (dll == null) continue;

                    try
                    {
                        var alc = new PluginLoadContext(folderName, dll);
                        var asm = alc.LoadFromAssemblyPath(dll);
                        var pluginType = asm.GetTypes().FirstOrDefault(t =>
                            typeof(IKombosPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                        if (pluginType == null)
                        {
                            host.Log($"Plugin '{folderName}': δεν βρέθηκε κλάση που υλοποιεί IKombosPlugin.");
                            continue;
                        }

                        if (Activator.CreateInstance(pluginType) is not IKombosPlugin plugin) continue;
                        plugin.Initialize(host);
                        loaded.Add(new LoadedPlugin { Name = plugin.Name, Version = plugin.Version, FolderName = folderName, Instance = plugin });
                    }
                    catch (Exception ex)
                    {
                        host.Log($"Plugin '{folderName}' απέτυχε να φορτωθεί: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                host.Log($"Plugin loader: {ex.Message}");
            }
            return loaded;
        }
    }
}
