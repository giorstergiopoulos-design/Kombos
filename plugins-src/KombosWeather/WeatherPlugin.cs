using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using Kombos.Plugins;

namespace KombosWeather
{
    /// <summary>Δείγμα plugin - καταχωρεί την εντολή "/weather <πόλη>" που χρησιμοποιεί το δωρεάν, χωρίς
    /// κλειδί API, Open-Meteo geocoding + forecast API. Αποδεικνύει το contract IKombosPlugin χωρίς να
    /// χρειάζεται κανένα μυστικό/κλειδί.</summary>
    public class WeatherPlugin : IKombosPlugin
    {
        public string Name => "Πρόγνωση Καιρού";
        public string Version => "1.0.0";

        private static readonly HttpClient Http = new();

        public void Initialize(IKombosHost host)
        {
            host.RegisterSlashCommand("/weather", GetWeatherAsync);
            host.Log("Weather plugin initialized.");
        }

        private static async Task<string> GetWeatherAsync(string city)
        {
            city = city.Trim();
            if (string.IsNullOrEmpty(city))
                return "Χρήση: /weather <πόλη> (π.χ. /weather Αθήνα)";

            var geoUrl = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(city)}&count=1&language=el";
            using var geoResp = await Http.GetAsync(geoUrl);
            var geoJson = await geoResp.Content.ReadAsStringAsync();
            if (!geoResp.IsSuccessStatusCode) return $"Geocoding API error {(int)geoResp.StatusCode}: {geoJson}";
            using var geoDoc = JsonDocument.Parse(geoJson);
            if (!geoDoc.RootElement.TryGetProperty("results", out var results) || results.ValueKind != JsonValueKind.Array || results.GetArrayLength() == 0)
                return $"Δεν βρέθηκε η πόλη \"{city}\".";

            var place = results[0];
            if (!place.TryGetProperty("latitude", out var latEl) || !place.TryGetProperty("longitude", out var lonEl))
                return $"Απρόσμενη μορφή απάντησης geocoding: {place.GetRawText()}";
            var lat = latEl.GetDouble();
            var lon = lonEl.GetDouble();
            var placeName = place.TryGetProperty("name", out var nameEl) ? nameEl.GetString() ?? city : city;

            var wUrl = $"https://api.open-meteo.com/v1/forecast?latitude={lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}&longitude={lon.ToString(System.Globalization.CultureInfo.InvariantCulture)}&current=temperature_2m,relative_humidity_2m,wind_speed_10m";
            using var wResp = await Http.GetAsync(wUrl);
            var wJson = await wResp.Content.ReadAsStringAsync();
            if (!wResp.IsSuccessStatusCode) return $"Forecast API error {(int)wResp.StatusCode}: {wJson}";
            using var wDoc = JsonDocument.Parse(wJson);
            if (!wDoc.RootElement.TryGetProperty("current", out var current))
                return $"Απρόσμενη μορφή απάντησης forecast: {wDoc.RootElement.GetRawText()}";

            var temp = current.TryGetProperty("temperature_2m", out var t) ? t.GetDouble() : (double?)null;
            var humidity = current.TryGetProperty("relative_humidity_2m", out var h) ? h.GetDouble() : (double?)null;
            var wind = current.TryGetProperty("wind_speed_10m", out var w) ? w.GetDouble() : (double?)null;

            return $"🌤 {placeName}: {temp}°C, υγρασία {humidity}%, άνεμος {wind} km/h.";
        }
    }
}
