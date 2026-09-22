using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Kombos.Models;

namespace Kombos.Services
{
    /// <summary>
    /// Γενικός client για οποιοδήποτε πάροχο εκθέτει OpenAI-compatible /chat/completions endpoint
    /// (OpenAI, Mistral, DeepSeek, xAI/Grok, Perplexity, Gemini μέσω openai-compat layer κ.ά.).
    /// Επιτρέπει στον χρήστη να ορίσει οποιοδήποτε από αυτά τα εργαλεία ως κεντρικό agent αντί του Claude.
    /// </summary>
    public static class OpenAiCompatibleClient
    {
        private static readonly HttpClient Http = new();

        public static async Task<ClaudeResult> SendAsync(string baseUrl, string model, string apiKey,
            string systemPrompt, List<ChatTurn> history, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Λείπει το κλειδί API για αυτό το εργαλείο.");

            // ΚΑΝΟΝΑΣ ελάχιστου κόστους tokens (ισχύει για κάθε agent, βλ. αντίστοιχο σχόλιο στο
            // ClaudeClient.SendAsync): OpenAI/GPT και DeepSeek κάνουν ΑΥΤΟΜΑΤΟ prompt caching στο δικό τους
            // API όταν το πρόθεμα ενός request (εδώ: το system message) είναι byte-for-byte ίδιο με προηγούμενο
            // request - καμία αλλαγή κώδικα δεν χρειάζεται, αρκεί να ΜΗΝ ξαναχτίζουμε το systemPrompt με
            // διαφορετική σειρά keys/whitespace μεταξύ κλήσεων της ίδιας κατάστασης (βλ. BuildCatalogJson/
            // L.SystemPrompt - είναι ήδη ντετερμινιστικά). Gemini/Mistral/Grok δεν έχουν αντίστοιχο μηχανισμό
            // εκτεθειμένο σε αυτό το ενοποιημένο chat-completions endpoint προς το παρόν.
            var messages = new List<object> { new { role = "system", content = systemPrompt } };
            foreach (var (role, content) in history)
                messages.Add(new { role, content });

            var payload = new { model, messages, max_tokens = 1024 };
            var requestJson = JsonSerializer.Serialize(payload);

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl.TrimEnd('/')}/chat/completions");
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            using var response = await Http.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"API error ({(int)response.StatusCode}): {ExtractErrorMessage(responseBody)}");

            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            string text = "";
            if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var first = choices[0];
                if (first.TryGetProperty("message", out var msg) && msg.TryGetProperty("content", out var contentProp))
                    text = contentProp.GetString() ?? "";
            }

            long inputTokens = 0, outputTokens = 0;
            if (root.TryGetProperty("usage", out var usage))
            {
                if (usage.TryGetProperty("prompt_tokens", out var p)) inputTokens = p.GetInt64();
                if (usage.TryGetProperty("completion_tokens", out var c)) outputTokens = c.GetInt64();
            }

            return new ClaudeResult { ReplyText = text, InputTokens = inputTokens, OutputTokens = outputTokens };
        }

        private static string ExtractErrorMessage(string body)
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("error", out var err))
                {
                    if (err.ValueKind == JsonValueKind.Object && err.TryGetProperty("message", out var msg))
                        return msg.GetString() ?? body;
                    if (err.ValueKind == JsonValueKind.String)
                        return err.GetString() ?? body;
                }
            }
            catch { /* not JSON - return raw body */ }
            return body;
        }
    }
}
