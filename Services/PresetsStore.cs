using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

using Kombos.Models;

namespace Kombos.Services
{
    /// <summary>Αποθηκεύει τα Presets/Personas (v4.5.0) - πάντα στο τοπικό %AppData%\Kombos, ΟΧΙ στον
    /// προαιρετικό φάκελο συγχρονισμού των συνομιλιών· ένα preset είναι προσωπική ρύθμιση agent, όχι κοινόχρηστο
    /// περιεχόμενο συζήτησης.</summary>
    public static class PresetsStore
    {
        private static string ResolveFilePath() => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Kombos", "presets-list.json");

        public static List<AgentPreset> Load()
        {
            try
            {
                var path = ResolveFilePath();
                if (!File.Exists(path)) return new List<AgentPreset>();
                return JsonSerializer.Deserialize<List<AgentPreset>>(File.ReadAllText(path)) ?? new List<AgentPreset>();
            }
            catch { return new List<AgentPreset>(); }
        }

        public static void Save(List<AgentPreset> presets)
        {
            try
            {
                var path = ResolveFilePath();
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, JsonSerializer.Serialize(presets));
            }
            catch { /* best-effort, ίδιο με τα υπόλοιπα stores */ }
        }
    }
}
