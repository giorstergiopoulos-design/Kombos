using System;
using System.IO;
using System.Linq;

namespace Kombos.Services
{
    /// <summary>Plugin SDK v2 (v4.5.0) - "Νέο Plugin" wizard: γεννά ένα έτοιμο-προς-build .csproj + starter
    /// .cs που υλοποιεί το IKombosPlugin, χωρίς το χειροκίνητο copy-paste του παλιού _ref\Kombos.dll pattern.
    /// Ο χρήστης ανοίγει τον φάκελο, χτίζει με "dotnet build", και αντιγράφει το .dll στο
    /// PluginLoader.PluginsRoot (ή το εγκαθιστά κατευθείαν μέσω του τοπικού Nexus Marketplace).</summary>
    public static class PluginScaffolder
    {
        public static string ProjectsRoot => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Kombos", "plugin-projects");

        public static string Scaffold(string pluginName, string kombosExePath)
        {
            var safeName = new string(pluginName.Where(char.IsLetterOrDigit).ToArray());
            if (string.IsNullOrEmpty(safeName)) safeName = "MyKombosPlugin";

            var projectDir = Path.Combine(ProjectsRoot, safeName);
            Directory.CreateDirectory(projectDir);

            var csprojPath = Path.Combine(projectDir, $"{safeName}.csproj");
            File.WriteAllText(csprojPath,
                "<Project Sdk=\"Microsoft.NET.Sdk\">\n" +
                "  <PropertyGroup>\n" +
                "    <TargetFramework>net10.0-windows</TargetFramework>\n" +
                "    <Nullable>enable</Nullable>\n" +
                "    <ImplicitUsings>enable</ImplicitUsings>\n" +
                "  </PropertyGroup>\n" +
                "  <ItemGroup>\n" +
                "    <!-- Reference στο ήδη εγκατεστημένο Kombos.exe - δεν χρειάζεται NuGet πακέτο. -->\n" +
                "    <Reference Include=\"Kombos\">\n" +
                $"      <HintPath>{kombosExePath}</HintPath>\n" +
                "      <Private>false</Private>\n" +
                "    </Reference>\n" +
                "  </ItemGroup>\n" +
                "</Project>\n");

            var csPath = Path.Combine(projectDir, $"{safeName}Plugin.cs");
            var command = safeName.ToLowerInvariant();
            File.WriteAllText(csPath,
                "using Kombos.Plugins;\n\n" +
                $"namespace {safeName}\n" +
                "{\n" +
                $"    public class {safeName}Plugin : IKombosPlugin\n" +
                "    {\n" +
                $"        public string Name => \"{pluginName}\";\n" +
                "        public string Version => \"1.0.0\";\n\n" +
                "        public void Initialize(IKombosHost host)\n" +
                "        {\n" +
                "            // Παράδειγμα: μια τοπική εντολή, μηδενικό κόστος tokens - τρέχει εξ ολοκλήρου\n" +
                "            // εδώ, ΔΕΝ πάει στον agent. Αντικατέστησε τη λογική με τη δική σου.\n" +
                $"            host.RegisterSlashCommand(\"/{command}\", async arg =>\n" +
                "            {\n" +
                "                await System.Threading.Tasks.Task.CompletedTask;\n" +
                $"                return $\"Γεια από το {{Name}}! Είπες: {{arg}}\";\n" +
                "            });\n" +
                "        }\n" +
                "    }\n" +
                "}\n");

            return projectDir;
        }
    }
}
