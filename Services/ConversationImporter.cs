using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using Kombos.Models;

namespace Kombos.Services
{
    /// <summary>Εισαγωγή συζητήσεων από αρχεία εξαγωγής ChatGPT/Claude.ai (v4.5.0) - ΤΟΠΙΚΟ parsing ενός JSON
    /// αρχείου που ο χρήστης ήδη έχει (μέσω του "Data export" της κάθε υπηρεσίας) - καμία ζωντανή κλήση
    /// API/scraping, δεν υπάρχει ούτε θα μπορούσε να υπάρξει επίσημο API γι' αυτό. Αναγνωρίζει το σχήμα από
    /// τα κλειδιά JSON: το ChatGPT export έχει ανά συζήτηση ένα δέντρο "mapping" (κόμβοι με parent/children),
    /// το Claude.ai export έχει μια ήδη-ταξινομημένη λίστα "chat_messages". Best-effort: μια συζήτηση με
    /// απροσδόκητο σχήμα απλά παραλείπεται αντί να ρίξει όλη την εισαγωγή.</summary>
    public static class ConversationImporter
    {
        public static List<Conversation> ImportFromJson(string json)
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var items = root.ValueKind == JsonValueKind.Array
                ? root.EnumerateArray().ToList()
                : new List<JsonElement> { root };

            var result = new List<Conversation>();
            foreach (var item in items)
            {
                try
                {
                    Conversation? conv = null;
                    if (item.TryGetProperty("mapping", out _)) conv = ImportChatGptConversation(item);
                    else if (item.TryGetProperty("chat_messages", out _)) conv = ImportClaudeConversation(item);

                    if (conv != null && conv.History.Count > 0) result.Add(conv);
                }
                catch { /* μία κατεστραμμένη εγγραφή δεν πρέπει να ρίξει όλη την εισαγωγή */ }
            }
            return result;
        }

        /// <summary>Το ChatGPT export αποθηκεύει τα μηνύματα σαν δέντρο (κλαδιά/επεξεργασίες) αντί για
        /// γραμμική λίστα. Αντί να ακολουθήσουμε parent/children (πολύπλοκο, εύκολο να σπάσει σε edge cases),
        /// παίρνουμε ΟΛΟΥΣ τους κόμβους με πραγματικό μήνυμα και τους ταξινομούμε κατά create_time - δίνει
        /// τη σωστή γραμμική σειρά ανάγνωσης για το happy-path (καμία επεξεργασία branch), που καλύπτει τη
        /// συντριπτική πλειοψηφία πραγματικών συζητήσεων.</summary>
        private static Conversation ImportChatGptConversation(JsonElement item)
        {
            var title = item.TryGetProperty("title", out var t) && t.ValueKind == JsonValueKind.String ? t.GetString() ?? "" : "";
            var turns = new List<(double Time, ChatTurn Turn)>();

            if (item.TryGetProperty("mapping", out var mapping) && mapping.ValueKind == JsonValueKind.Object)
            {
                foreach (var node in mapping.EnumerateObject())
                {
                    if (!node.Value.TryGetProperty("message", out var msg) || msg.ValueKind != JsonValueKind.Object) continue;
                    if (!msg.TryGetProperty("author", out var author) || author.ValueKind != JsonValueKind.Object) continue;

                    var role = author.TryGetProperty("role", out var r) ? r.GetString() ?? "" : "";
                    if (role != "user" && role != "assistant") continue;

                    string text = "";
                    if (msg.TryGetProperty("content", out var content) &&
                        content.TryGetProperty("parts", out var parts) && parts.ValueKind == JsonValueKind.Array)
                    {
                        text = string.Join("\n", parts.EnumerateArray()
                            .Where(p => p.ValueKind == JsonValueKind.String)
                            .Select(p => p.GetString()));
                    }
                    if (string.IsNullOrWhiteSpace(text)) continue;

                    double time = msg.TryGetProperty("create_time", out var ct) && ct.ValueKind == JsonValueKind.Number ? ct.GetDouble() : 0;
                    turns.Add((time, new ChatTurn { Role = role, Content = text }));
                }
            }

            return new Conversation
            {
                Id = Guid.NewGuid().ToString("N"),
                Title = string.IsNullOrWhiteSpace(title) ? "ChatGPT import" : title,
                History = turns.OrderBy(x => x.Time).Select(x => x.Turn).ToList(),
                UpdatedAtUtc = DateTime.UtcNow.ToString("O")
            };
        }

        private static Conversation ImportClaudeConversation(JsonElement item)
        {
            var title = item.TryGetProperty("name", out var n) && n.ValueKind == JsonValueKind.String ? n.GetString() ?? "" : "";
            var turns = new List<ChatTurn>();

            if (item.TryGetProperty("chat_messages", out var messages) && messages.ValueKind == JsonValueKind.Array)
            {
                foreach (var m in messages.EnumerateArray())
                {
                    var sender = m.TryGetProperty("sender", out var s) ? s.GetString() ?? "" : "";
                    var text = m.TryGetProperty("text", out var tx) && tx.ValueKind == JsonValueKind.String ? tx.GetString() ?? "" : "";
                    if (string.IsNullOrWhiteSpace(text)) continue;
                    turns.Add(new ChatTurn { Role = sender == "human" ? "user" : "assistant", Content = text });
                }
            }

            return new Conversation
            {
                Id = Guid.NewGuid().ToString("N"),
                Title = string.IsNullOrWhiteSpace(title) ? "Claude.ai import" : title,
                History = turns,
                UpdatedAtUtc = DateTime.UtcNow.ToString("O")
            };
        }
    }
}
