using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using projekt.NET.Models;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace projekt.NET.Controllers
{
    public class NewsItem
    {
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public DateTime PublishDate { get; set; }
        public string? ImageUrl { get; set; }
        public string? Source { get; set; }
    }

    public class UpcomingRelease
    {
        public string Title { get; set; } = string.Empty;
        public string ReleaseDate { get; set; } = string.Empty;
        public string Platforms { get; set; } = string.Empty;
    }

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        private static List<NewsItem> _newsList = new List<NewsItem>();
        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
             var upcomingReleases = new List<UpcomingRelease>
             {
                 new UpcomingRelease { Title = "Grand Theft Auto VI", ReleaseDate = "Jesień 2026", Platforms = "PS5, Xbox Series X/S" },
                 new UpcomingRelease { Title = "Ghost of Yōtei", ReleaseDate = "Październik 2026", Platforms = "PS5" },
                 new UpcomingRelease { Title = "Metroid Prime 4: Beyond", ReleaseDate = "Listopad 2026", Platforms = "Nintendo Switch" },
                 new UpcomingRelease { Title = "Death Stranding 2: On The Beach", ReleaseDate = "Grudzień 2026", Platforms = "PS5" }
             };

            ViewBag.UpcomingReleases = upcomingReleases;

            // Przekazujemy wygenerowany tekst do widoku
            return View(_newsList);
        }
        [HttpPost]
        public async Task<IActionResult> GenerateAiNews()
        {
            // Pobieramy klucz dla Gemini
            var apiKey = _configuration["AiSettings:GeminiApiKey"];
            if (string.IsNullOrEmpty(apiKey)) return RedirectToAction("Index");

            using var client = new HttpClient();

            client.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);

            var prompt = "Jesteś ekspertem branży gier wideo. Wygeneruj 2 najnowsze newsy ze świata gier. " +
                         "Zwróć wynik TYLKO jako czystą tablicę JSON w formacie: " +
                         "[{\"Title\": \"tytuł newsa\", \"Summary\": \"treść...\", \"Source\": \"np. IGN, Gry-Online\"}]";

            // Struktura zapytania specyficzna dla Gemini
            var requestBody = new
            {
                contents = new[]
                {
            new
            {
                parts = new[] { new { text = prompt } }
            }
        },
                generationConfig = new
                {
                    temperature = 0.7,
                    responseMimeType = "application/json"
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            // używamy modelu gemini-3.5-flash
            var response = await client.PostAsync("https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash:generateContent", content);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(jsonResponse);

                try
                {
                    // Wyciąganie tekstu z odpowiedzi Gemini
                    var resultText = document.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text").GetString();

                    // Dekodujemy JSON 
                    if (!string.IsNullOrEmpty(resultText))
                    {
                        var generatedNews = JsonSerializer.Deserialize<List<NewsItem>>(resultText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (generatedNews != null)
                        {
                            foreach (var item in generatedNews) item.PublishDate = DateTime.Now;
                            _newsList = generatedNews;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Błąd parsowania JSON od Gemini: {ex.Message}");
                }
            }
            else
            {
                // Przydatne podczas testów
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Błąd Gemini API: {error}");
            }

            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}