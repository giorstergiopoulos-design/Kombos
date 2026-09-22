using System;
using System.Threading.Tasks;

using Kombos.Models;

namespace Kombos.Plugins
{
    /// <summary>Contract για plugins τρίτων του Kombos. Σκόπιμα ΔΕΝ εκθέτει τύπους WPF (Window/Button κ.λπ.) -
    /// το WPF έχει γνωστά προβλήματα με πολλαπλά AssemblyLoadContext ταυτόχρονα, οπότε ένα plugin είναι καθαρά
    /// λογική/δεδομένα· η ίδια η Kombos αποφασίζει πώς θα το αποδώσει οπτικά.</summary>
    public interface IKombosPlugin
    {
        string Name { get; }
        string Version { get; }
        void Initialize(IKombosHost host);
    }

    /// <summary>Ό,τι επιτρέπεται σε ένα plugin να κάνει μέσα στο Kombos.</summary>
    public interface IKombosHost
    {
        /// <summary>Προσθέτει ένα νέο εργαλείο στον κατάλογο (εμφανίζεται σαν κανονικό πλακίδιο).</summary>
        void RegisterTool(ToolItem tool);

        /// <summary>Καταχωρεί μια εντολή "/όνομα όρισμα" - όταν ο χρήστης τη στείλει στο chat, καλείται το
        /// handler (με το κείμενο μετά την εντολή) αντί να πάει στον Claude, και η επιστρεφόμενη τιμή
        /// εμφανίζεται σαν φούσκα απάντησης.</summary>
        void RegisterSlashCommand(string command, Func<string, Task<string>> handler);

        void Log(string message);
    }
}
