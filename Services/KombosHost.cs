using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using Kombos.Data;
using Kombos.Models;
using Kombos.Plugins;

namespace Kombos.Services
{
    /// <summary>Η μόνη "πόρτα" που έχουν τα plugins προς το Kombos - βλ. IKombosHost.</summary>
    public class KombosHost : IKombosHost
    {
        internal readonly Dictionary<string, Func<string, Task<string>>> SlashCommands = new(StringComparer.OrdinalIgnoreCase);

        public void RegisterTool(ToolItem tool) => Catalog.Tools.Add(tool);

        public void RegisterSlashCommand(string command, Func<string, Task<string>> handler)
        {
            var key = command.StartsWith('/') ? command : "/" + command;
            SlashCommands[key] = handler;
        }

        public void Log(string message) => Debug.WriteLine($"[Plugin] {message}");
    }
}
