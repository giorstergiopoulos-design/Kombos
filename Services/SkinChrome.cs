using System.Windows;
using System.Windows.Controls;

using Kombos.Models;

namespace Kombos.Services
{
    /// <summary>Μικρό βοηθητικό για δευτερεύοντα παράθυρα (Settings/Agent/Appearance) ώστε το skin "Windows
    /// Classic" να τα κάνει πραγματικά τετράγωνα (καμία στρογγυλεμένη γωνία), ακριβώς όπως ήταν τα πραγματικά
    /// παράθυρα των Windows 95/98/2000 - χωρίς να χρειάζεται να επαναλαμβάνεται η ίδια λογική σε κάθε
    /// ξεχωριστό code-behind. Τα χρώματα (navy τίτλου, γκρι φόντο) έρχονται αυτόματα σε ΟΛΑ τα παράθυρα μέσω
    /// του global resource override στο MainWindow.ApplySkinPalette() - δεν χρειάζεται να οριστούν εδώ.</summary>
    public static class SkinChrome
    {
        public static void ApplySquareIfClassic(WindowsSkin skin, Border shellBorder, Border headerBorder)
        {
            if (skin != WindowsSkin.Classic) return;
            shellBorder.CornerRadius = new CornerRadius(0);
            headerBorder.CornerRadius = new CornerRadius(0);
        }
    }
}
