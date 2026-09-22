using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kombos.Services
{
    public class ElevenLabsUsage
    {
        public long CharactersUsed { get; set; }
        public long CharacterLimit { get; set; }
    }

    /// <summary>Ελάχιστος client για το πραγματικό usage endpoint του ElevenLabs (GET /v1/user),
    /// ώστε η γραμμή κατάστασης να δείχνει πραγματικά στοιχεία χρήσης αντί για "δεν έχει συνδεθεί".</summary>
    public static class ElevenLabsClient
    {
        private static readonly HttpClient Http = new();
        private const string Endpoint = "https://api.elevenlabs.io/v1/user";

        public static async Task<ElevenLabsUsage?> GetUsageAsync(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey)) return null;

            using var request = new HttpRequestMessage(HttpMethod.Get, Endpoint);
            request.Headers.Add("xi-api-key", apiKey);

            using var response = await Http.SendAsync(request).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return null;

            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (!root.TryGetProperty("subscription", out var sub)) return null;

            long used = sub.TryGetProperty("character_count", out var u) ? u.GetInt64() : 0;
            long limit = sub.TryGetProperty("character_limit", out var l) ? l.GetInt64() : 0;

            return new ElevenLabsUsage { CharactersUsed = used, CharacterLimit = limit };
        }
    }
}
