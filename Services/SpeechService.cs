using System;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;

namespace Kombos.Services
{
    /// <summary>Φωνή και προς τις δύο κατευθύνσεις (v4.5.0) - 100% τοπικό/δωρεάν μέσω των ενσωματωμένων
    /// μηχανισμών φωνής των Windows, καμία κλήση cloud/API key. Δεν είναι πραγματική αμφίδρομη συνομιλία σε
    /// πραγματικό χρόνο (χρειάζεται πληρωμένο realtime voice API) - εδώ είναι push-to-talk εισαγωγή ΚΑΙ
    /// ξεχωριστή ανάγνωση απάντησης, το ρεαλιστικό τοπικό/δωρεάν αντίστοιχο.</summary>
    public static class SpeechService
    {
        private static SpeechSynthesizer? _synth;
        private static SpeechRecognitionEngine? _recognizer;

        private static readonly Regex MarkdownNoise = new(@"[*_`#]+|~~", RegexOptions.Compiled);

        /// <summary>Αφαιρεί τα πιο κοινά markdown σύμβολα ώστε η ανάγνωση να μην προφέρει κυριολεκτικά
        /// αστερίσκους/κάγκελα - όχι πλήρης markdown parser, μόνο καθαρισμός για ομιλία.</summary>
        private static string StripMarkdownForSpeech(string text) => MarkdownNoise.Replace(text, "");

        public static bool TrySpeak(string text)
        {
            try
            {
                _synth ??= new SpeechSynthesizer();
                _synth.SpeakAsyncCancelAll();
                _synth.SpeakAsync(StripMarkdownForSpeech(text));
                return true;
            }
            catch
            {
                return false; // π.χ. δεν υπάρχει εγκατεστημένη φωνή στο σύστημα - όχι κρίσιμο
            }
        }

        public static void StopSpeaking()
        {
            try { _synth?.SpeakAsyncCancelAll(); } catch { /* δεν έτρεχε ήδη τίποτα - ασφαλές no-op */ }
        }

        /// <summary>Ξεκινά αναγνώριση ΕΝΟΣ utterance (push-to-talk, όχι συνεχής ακρόαση). Καλεί το onResult με
        /// το αναγνωρισμένο κείμενο όταν ολοκληρωθεί, ή με null αν αποτύχει/ακυρωθεί - ο καλών αποφασίζει τι
        /// σημαίνει null (π.χ. επαναφορά εικονιδίου μικροφώνου).</summary>
        public static bool TryStartListening(Action<string?> onResult)
        {
            try
            {
                StopListening();
                _recognizer = new SpeechRecognitionEngine();
                _recognizer.LoadGrammar(new DictationGrammar());
                _recognizer.SetInputToDefaultAudioDevice();
                _recognizer.SpeechRecognized += (s, e) => onResult(e.Result?.Text);
                _recognizer.RecognizeCompleted += (s, e) =>
                {
                    if (e.Error != null || e.Cancelled) onResult(null);
                    _recognizer?.Dispose();
                    _recognizer = null;
                };
                _recognizer.RecognizeAsync(RecognizeMode.Single);
                return true;
            }
            catch
            {
                return false; // π.χ. δεν υπάρχει μικρόφωνο/speech recognizer εγκατεστημένος στα Windows
            }
        }

        public static void StopListening()
        {
            try { _recognizer?.RecognizeAsyncStop(); _recognizer?.Dispose(); }
            catch { /* δεν έτρεχε ήδη τίποτα */ }
            finally { _recognizer = null; }
        }
    }
}
