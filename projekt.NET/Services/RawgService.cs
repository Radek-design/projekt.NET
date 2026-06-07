using System.Text.Json;

namespace projekt.NET.Services
{
    // Serwis do komunikacji z RAWG API - zewnętrzne API z bazą gier
    public class RawgService
    {
        private readonly HttpClient _httpClient;
        // Klucz API pobieramy z appsettings.json
        private readonly string _apiKey;

        public RawgService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["RawgApi:ApiKey"] ?? "";
        }

        // Wyszukuje grę po nazwie i zwraca jej dane z RAWG
        public async Task<RawgGameResult?> SearchGameAsync(string gameName)
        {
            var url = $"https://api.rawg.io/api/games?key={_apiKey}&search={Uri.EscapeDataString(gameName)}&page_size=1";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<RawgResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return data?.Results?.FirstOrDefault();
        }
    }

    // Klasy do deserializacji odpowiedzi z RAWG API
    public class RawgResponse
    {
        public List<RawgGameResult>? Results { get; set; }
    }

    public class RawgGameResult
    {
        public string? Name { get; set; }
        public string? Released { get; set; }
        public string? Background_Image { get; set; }
        public double Rating { get; set; }
        public List<RawgPlatform>? Platforms { get; set; }
        public List<RawgGenre>? Genres { get; set; }
    }

    public class RawgPlatform
    {
        public RawgPlatformDetail? Platform { get; set; }
    }

    public class RawgPlatformDetail
    {
        public string? Name { get; set; }
    }

    public class RawgGenre
    {
        public string? Name { get; set; }
    }
}