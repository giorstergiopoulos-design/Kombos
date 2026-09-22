using System.Collections.Generic;
using Kombos.Models;

namespace Kombos.Data
{
    /// <summary>Δική μας πρωτότυπη "Βιβλιοθήκη Prompts" (v4.1.0) - η ΙΔΕΑ μιας γκαλερί έτοιμων prompts/
    /// "assistants" υπάρχει σε αρκετές εφαρμογές ανοιχτού κώδικα (π.χ. Cherry Studio, LobeChat, NextChat's
    /// "Masks")· εδώ είναι αρχική υλοποίηση με δικό μας, τοπικό JSON-free dataset - κανένα prompt δεν είναι
    /// αντιγραμμένο από αλλού, ώστε να μην υπάρχει κανένα θέμα άδειας (κάποιες από αυτές τις εφαρμογές είναι
    /// AGPL/custom license, ασύμβατες με αντιγραφή κώδικα/περιεχομένου). 100% τοπικό, μηδενικό κόστος tokens
    /// μέχρι ο χρήστης να στείλει ένα prompt.</summary>
    public static class PromptLibrary
    {
        public static readonly List<PromptLibraryItem> Items = new()
        {
            new PromptLibraryItem
            {
                CategoryKey = "text",
                Title = "Σύντομο, ευγενικό email", TitleEn = "Short, polite email", TitleFr = "E-mail court et poli", TitleDe = "Kurze, höfliche E-Mail",
                Prompt = "Ξαναγράψε το παρακάτω email πιο σύντομο και ευγενικό, κρατώντας το ίδιο νόημα:\n\n",
                PromptEn = "Rewrite the email below to be shorter and more polite, keeping the same meaning:\n\n",
                PromptFr = "Réécris l'e-mail ci-dessous pour qu'il soit plus court et plus poli, en gardant le même sens :\n\n",
                PromptDe = "Schreibe die folgende E-Mail kürzer und höflicher, ohne den Sinn zu verändern:\n\n"
            },
            new PromptLibraryItem
            {
                CategoryKey = "text",
                Title = "Περίληψη σε 5 σημεία", TitleEn = "Summary in 5 points", TitleFr = "Résumé en 5 points", TitleDe = "Zusammenfassung in 5 Punkten",
                Prompt = "Κάνε περίληψη του παρακάτω κειμένου σε 5 σύντομα bullet points:\n\n",
                PromptEn = "Summarize the text below into 5 short bullet points:\n\n",
                PromptFr = "Résume le texte ci-dessous en 5 points brefs :\n\n",
                PromptDe = "Fasse den folgenden Text in 5 kurzen Stichpunkten zusammen:\n\n"
            },
            new PromptLibraryItem
            {
                CategoryKey = "code",
                Title = "Εξήγηση κώδικα βήμα-βήμα", TitleEn = "Explain code step by step", TitleFr = "Expliquer le code étape par étape", TitleDe = "Code Schritt für Schritt erklären",
                Prompt = "Εξήγησε τι κάνει αυτός ο κώδικας, βήμα-βήμα, σε απλά λόγια:\n\n```\n\n```",
                PromptEn = "Explain what this code does, step by step, in plain language:\n\n```\n\n```",
                PromptFr = "Explique ce que fait ce code, étape par étape, en termes simples :\n\n```\n\n```",
                PromptDe = "Erkläre Schritt für Schritt in einfachen Worten, was dieser Code macht:\n\n```\n\n```"
            },
            new PromptLibraryItem
            {
                CategoryKey = "code",
                Title = "Εντοπισμός & διόρθωση bug", TitleEn = "Find & fix a bug", TitleFr = "Trouver et corriger un bug", TitleDe = "Bug finden & beheben",
                Prompt = "Αυτός ο κώδικας πετάει το εξής σφάλμα: [περιέγραψε το σφάλμα]. Βρες την πιθανή αιτία και πρότεινε διόρθωση:\n\n```\n\n```",
                PromptEn = "This code throws the following error: [describe the error]. Find the likely cause and suggest a fix:\n\n```\n\n```",
                PromptFr = "Ce code renvoie l'erreur suivante : [décris l'erreur]. Trouve la cause probable et propose une correction :\n\n```\n\n```",
                PromptDe = "Dieser Code wirft folgenden Fehler: [Fehler beschreiben]. Finde die wahrscheinliche Ursache und schlage eine Korrektur vor:\n\n```\n\n```"
            },
            new PromptLibraryItem
            {
                CategoryKey = "research",
                Title = "Σύγκριση επιλογών σε πίνακα", TitleEn = "Compare options in a table", TitleFr = "Comparer des options dans un tableau", TitleDe = "Optionen in einer Tabelle vergleichen",
                Prompt = "Σύγκρινε τις παρακάτω επιλογές σε πίνακα με κριτήρια κόστος / χρόνο / ρίσκο:\n\n",
                PromptEn = "Compare the options below in a table using cost / time / risk as criteria:\n\n",
                PromptFr = "Compare les options ci-dessous dans un tableau selon coût / temps / risque :\n\n",
                PromptDe = "Vergleiche die folgenden Optionen in einer Tabelle nach Kosten / Zeit / Risiko:\n\n"
            },
            new PromptLibraryItem
            {
                CategoryKey = "research",
                Title = "Ερωτήσεις πριν αποφασίσω", TitleEn = "Questions before I decide", TitleFr = "Questions avant de décider", TitleDe = "Fragen vor der Entscheidung",
                Prompt = "Πριν αποφασίσω για το εξής θέμα, ποιες είναι οι 5 πιο σημαντικές ερωτήσεις που πρέπει να απαντήσω πρώτα;\n\nΘέμα: ",
                PromptEn = "Before I decide on the following, what are the 5 most important questions I should answer first?\n\nTopic: ",
                PromptFr = "Avant de décider sur le sujet suivant, quelles sont les 5 questions les plus importantes à répondre d'abord ?\n\nSujet : ",
                PromptDe = "Bevor ich mich entscheide, was sind die 5 wichtigsten Fragen, die ich zuerst beantworten sollte?\n\nThema: "
            },
            new PromptLibraryItem
            {
                CategoryKey = "design",
                Title = "Ιδέες χρωματικής παλέτας", TitleEn = "Color palette ideas", TitleFr = "Idées de palette de couleurs", TitleDe = "Ideen für Farbpaletten",
                Prompt = "Πρότεινε 3 συνδυασμούς χρωμάτων (με hex codes) για το παρακάτω project, με σύντομη αιτιολόγηση για τον καθένα:\n\nProject: ",
                PromptEn = "Suggest 3 color combinations (with hex codes) for the project below, with a short rationale for each:\n\nProject: ",
                PromptFr = "Propose 3 combinaisons de couleurs (avec codes hex) pour le projet ci-dessous, avec une brève justification pour chacune :\n\nProjet : ",
                PromptDe = "Schlage 3 Farbkombinationen (mit Hex-Codes) für das folgende Projekt vor, mit kurzer Begründung für jede:\n\nProjekt: "
            },
            new PromptLibraryItem
            {
                CategoryKey = "design",
                Title = "Κριτική διεπαφής", TitleEn = "UI critique", TitleFr = "Critique d'interface", TitleDe = "UI-Kritik",
                Prompt = "Κάνε κριτική στην παρακάτω περιγραφή διεπαφής σαν έμπειρος designer, εστιάζοντας σε αναγνωσιμότητα και οπτική ιεραρχία:\n\n",
                PromptEn = "Critique the interface description below like an experienced designer, focusing on readability and visual hierarchy:\n\n",
                PromptFr = "Critique la description d'interface ci-dessous comme un designer expérimenté, en te concentrant sur la lisibilité et la hiérarchie visuelle :\n\n",
                PromptDe = "Kritisiere die folgende UI-Beschreibung wie ein erfahrener Designer, mit Fokus auf Lesbarkeit und visuelle Hierarchie:\n\n"
            },
            new PromptLibraryItem
            {
                CategoryKey = "agents",
                Title = "Σχέδιο αυτοματισμού", TitleEn = "Automation plan", TitleFr = "Plan d'automatisation", TitleDe = "Automatisierungsplan",
                Prompt = "Θέλω να αυτοματοποιήσω την παρακάτω διαδικασία. Πρότεινε βήματα και εργαλεία που θα ταίριαζαν:\n\nΔιαδικασία: ",
                PromptEn = "I want to automate the following process. Suggest steps and tools that would fit:\n\nProcess: ",
                PromptFr = "Je veux automatiser le processus suivant. Propose des étapes et des outils adaptés :\n\nProcessus : ",
                PromptDe = "Ich möchte den folgenden Prozess automatisieren. Schlage passende Schritte und Werkzeuge vor:\n\nProzess: "
            },
            new PromptLibraryItem
            {
                CategoryKey = "agents",
                Title = "Task σε βήματα", TitleEn = "Break task into steps", TitleFr = "Découper une tâche en étapes", TitleDe = "Aufgabe in Schritte zerlegen",
                Prompt = "Σπάσε το παρακάτω task σε μικρά, εκτελέσιμα βήματα με εκτίμηση χρόνου για το καθένα:\n\nTask: ",
                PromptEn = "Break the task below into small, actionable steps with a time estimate for each:\n\nTask: ",
                PromptFr = "Découpe la tâche ci-dessous en petites étapes actionnables, avec une estimation de temps pour chacune :\n\nTâche : ",
                PromptDe = "Zerlege die folgende Aufgabe in kleine, umsetzbare Schritte mit einer Zeitschätzung für jeden:\n\nAufgabe: "
            }
        };
    }
}
