using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kombos.Services
{
    /// <summary>Πραγματική δημιουργία εικόνας μέσα στη συνομιλία (v4.5.0, /image) - OpenAI Images API
    /// (DALL-E), χρησιμοποιεί το ήδη αποθηκευμένο κλειδί ChatGPT/OpenAI (MainWindow._apiKeys["ChatGPT"]) -
    /// όχι απλή παραπομπή σε εξωτερικό εργαλείο. Η URL που επιστρέφει η OpenAI λήγει μετά από περίπου μία
    /// ώρα, οπότε η εικόνα κατεβαίνει αμέσως τοπικά (%AppData%\Kombos\images) ώστε να μένει μόνιμα ορατή
    /// μέσα στην αποθηκευμένη συζήτηση.</summary>
    public static class OpenAiImageClient
    {
        private static readonly HttpClient Http = new();

        public static async Task<string> GenerateAndSaveAsync(string apiKey, string prompt)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/images/generations");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            var body = JsonSerializer.Serialize(new { model = "dall-e-3", prompt, n = 1, size = "1024x1024" });
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            using var response = await Http.SendAsync(request);
            var responseText = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(ExtractErrorMessage(responseText) ?? $"HTTP {(int)response.StatusCode}");

            using var doc = JsonDocument.Parse(responseText);
            var url = doc.RootElement.GetProperty("data")[0].GetProperty("url").GetString()
                ?? throw new InvalidOperationException("Η απάντηση της OpenAI δεν περιείχε URL εικόνας.");

            var imageBytes = await Http.GetByteArrayAsync(url);
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Kombos", "images");
            Directory.CreateDirectory(folder);
            var filePath = Path.Combine(folder, $"{Guid.NewGuid():N}.png");
            await File.WriteAllBytesAsync(filePath, imageBytes);
            return filePath;
        }

        private static string? ExtractErrorMessage(string responseText)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseText);
                return doc.RootElement.TryGetProperty("error", out var err) && err.TryGetProperty("message", out var msg)
                    ? msg.GetString() : null;
            }
            catch { return null; }
        }
    }
}
