using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Kombos.Models;

namespace Kombos.Services
{
    /// <summary>Αποτέλεσμα μιας κλήσης στο Anthropic Messages API.</summary>
    public class ClaudeResult
    {
        public string ReplyText { get; set; } = "";
        /// <summary>Ορατό σκεπτικό (extended thinking), αν ζητήθηκε — αλλιώς κενό.</summary>
        public string ThinkingText { get; set; } = "";
        /// <summary>Πηγές (τίτλος/URL) από το web search tool, αν χρησιμοποιήθηκε - διπλότυπα αφαιρούνται.</summary>
        public List<(string Title, string Url)> Citations { get; set; } = new();
        public long InputTokens { get; set; }
        public long OutputTokens { get; set; }
        /// <summary>Tokens γραμμένα στην cache αυτή τη φορά (πρώτη φορά/έληξε) - χρεώνονται ελαφρώς ΠΑΝΩ από
        /// την κανονική τιμή εισόδου. 0 αν δεν υπήρξε εγγραφή.</summary>
        public long CacheCreationInputTokens { get; set; }
        /// <summary>Tokens που διαβάστηκαν ΑΠΟ την cache (χτύπημα) - χρεώνονται στο ~10% της κανονικής τιμής
        /// εισόδου. 0 αν δεν υπήρξε χτύπημα.</summary>
        public long CacheReadInputTokens { get; set; }
    }

    /// <summary>
    /// Λεπτός client για το api.anthropic.com/v1/messages. Το κλειδί API δίνεται από τον χρήστη
    /// (PasswordBox στο MainWindow) και κρατιέται μόνο in-memory — δεν αποθηκεύεται ποτέ σε δίσκο.
    /// </summary>
    public class ClaudeClient
    {
        private static readonly HttpClient Http = new();

        private const string Endpoint = "https://api.anthropic.com/v1/messages";
        private const string AnthropicVersion = "2023-06-01";
        private const string Model = "claude-sonnet-4-5";
        private const int MaxTokens = 1024;
        private const int ThinkingBudgetTokens = 6000;

        /// <summary>Κλειδί API Anthropic. Μόνο in-memory για τη διάρκεια της συνεδρίας.</summary>
        public string ApiKey { get; set; } = "";

        /// <summary>Στέλνει το ιστορικό συζήτησης στο Claude. Αν δοθεί <paramref name="image"/>, επισυνάπτεται
        /// (ως εικόνα όρασης, base64) στο ΤΕΛΕΥΤΑΙΟ μήνυμα του χρήστη — δεν ξαναστέλνεται σε επόμενες κλήσεις.
        /// Αν <paramref name="enableThinking"/> είναι true, ζητείται ορατό extended thinking (Claude πρέπει να
        /// το υποστηρίζει) — το thinking επιστρέφεται ξεχωριστά στο <see cref="ClaudeResult.ThinkingText"/>.
        /// Αν <paramref name="enableWebSearch"/> είναι true, δίνεται στο Claude το επίσημο server-side εργαλείο
        /// αναζήτησης ιστού (web_search) — αποφασίζει μόνο του πότε χρειάζεται να ψάξει στο διαδίκτυο.</summary>
        public async Task<ClaudeResult> SendAsync(string systemPrompt, List<ChatTurn> history,
            (byte[] Bytes, string MediaType)? image = null, bool enableThinking = false, CancellationToken cancellationToken = default,
            bool enableWebSearch = false, List<(string Name, string Url)>? mcpServers = null)
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
                throw new InvalidOperationException("Λείπει το κλειδί API Anthropic. Πρόσθεσέ το στο πεδίο πάνω-πάνω στο παράθυρο.");

            var messages = new List<object>(history.Count);
            for (int i = 0; i < history.Count; i++)
            {
                var (role, content) = history[i];
                bool isLastUserTurn = image != null && i == history.Count - 1 && role == "user";
                if (isLastUserTurn)
                {
                    var base64 = Convert.ToBase64String(image!.Value.Bytes);
                    messages.Add(new
                    {
                        role,
                        content = new object[]
                        {
                            new { type = "image", source = new { type = "base64", media_type = image.Value.MediaType, data = base64 } },
                            new { type = "text", text = content }
                        }
                    });
                }
                else
                {
                    messages.Add(new { role, content });
                }
            }

            var payload = new Dictionary<string, object>
            {
                ["model"] = Model,
                ["max_tokens"] = enableThinking ? MaxTokens + ThinkingBudgetTokens : MaxTokens,
                // ΚΑΝΟΝΑΣ ελάχιστου κόστους tokens (ισχύει για όλους τους agents, βλ. σχόλιο στο
                // OpenAiCompatibleClient.SendAsync): το system prompt (οδηγίες + πλήρης κατάλογος 39
                // εργαλείων σε JSON) είναι σχεδόν ίδιο σε κάθε turn της ίδιας συνομιλίας/γλώσσας - με
                // cache_control ως prompt caching breakpoint, το Anthropic API χρεώνει τα cached tokens στο
                // ~10% της κανονικής τιμής σε κάθε επόμενη κλήση (χρειάζεται ελάχιστο μήκος για να "πιάσει"
                // αλλά δεν βλάπτει ποτέ να το ζητάμε). Το system πρέπει να είναι array από content blocks
                // (όχι απλό string) για να δεχτεί cache_control.
                ["system"] = new object[]
                {
                    new { type = "text", text = systemPrompt, cache_control = new { type = "ephemeral" } }
                },
                ["messages"] = messages
            };
            if (enableThinking)
                payload["thinking"] = new { type = "enabled", budget_tokens = ThinkingBudgetTokens };
            if (enableWebSearch)
                payload["tools"] = new object[] { new { type = "web_search_20250305", name = "web_search" } };
            // Απομακρυσμένοι MCP servers - το Claude τους καλεί απευθείας μέσω server-side connector, χωρίς να
            // χρειάζεται το Kombos να τρέχει δικό του MCP client. ΣΗΜΕΙΩΣΗ: δεν έχει επιβεβαιωθεί ακόμα με
            // πραγματικό κλειδί/server - αν το API απαιτεί header "anthropic-beta", θα φανεί από σφάλμα 4xx
            // στην πρώτη πραγματική δοκιμή.
            if (mcpServers is { Count: > 0 })
                payload["mcp_servers"] = mcpServers.Select(s => new { type = "url", url = s.Url, name = s.Name }).ToArray();

            var requestJson = JsonSerializer.Serialize(payload);

            using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
            request.Headers.Add("x-api-key", ApiKey);
            request.Headers.Add("anthropic-version", AnthropicVersion);
            request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            using var response = await Http.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Anthropic API error ({(int)response.StatusCode}): {ExtractErrorMessage(responseBody)}");

            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            var text = new StringBuilder();
            var thinking = new StringBuilder();
            var citations = new List<(string Title, string Url)>();
            if (root.TryGetProperty("content", out var contentArray) && contentArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var block in contentArray.EnumerateArray())
                {
                    if (!block.TryGetProperty("type", out var typeProp)) continue;
                    var blockType = typeProp.GetString();
                    if (blockType == "text" && block.TryGetProperty("text", out var textProp))
                    {
                        text.Append(textProp.GetString());
                        if (block.TryGetProperty("citations", out var citeArray) && citeArray.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var cite in citeArray.EnumerateArray())
                            {
                                var url = cite.TryGetProperty("url", out var u) ? u.GetString() : null;
                                if (string.IsNullOrEmpty(url)) continue;
                                var title = cite.TryGetProperty("title", out var t) ? t.GetString() ?? url : url;
                                if (!citations.Any(c => c.Url == url)) citations.Add((title, url));
                            }
                        }
                    }
                    else if (blockType == "thinking" && block.TryGetProperty("thinking", out var thinkingProp))
                        thinking.Append(thinkingProp.GetString());
                }
            }

            long inputTokens = 0, outputTokens = 0, cacheCreation = 0, cacheRead = 0;
            if (root.TryGetProperty("usage", out var usage))
            {
                if (usage.TryGetProperty("input_tokens", out var inTok)) inputTokens = inTok.GetInt64();
                if (usage.TryGetProperty("output_tokens", out var outTok)) outputTokens = outTok.GetInt64();
                if (usage.TryGetProperty("cache_creation_input_tokens", out var cc)) cacheCreation = cc.GetInt64();
                if (usage.TryGetProperty("cache_read_input_tokens", out var cr)) cacheRead = cr.GetInt64();
            }

            return new ClaudeResult
            {
                ReplyText = text.ToString(),
                ThinkingText = thinking.ToString(),
                Citations = citations,
                InputTokens = inputTokens,
                OutputTokens = outputTokens,
                CacheCreationInputTokens = cacheCreation,
                CacheReadInputTokens = cacheRead
            };
        }

        private static string ExtractErrorMessage(string body)
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("error", out var err) &&
                    err.TryGetProperty("message", out var msg))
                    return msg.GetString() ?? body;
            }
            catch
            {
                // Το body δεν ήταν JSON — επέστρεψέ το ως έχει.
            }
            return body;
        }
    }
}
