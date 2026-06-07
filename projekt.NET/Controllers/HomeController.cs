using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projekt.NET.Models;
using projekt.NET.Models.Entities;
using projekt.NET.Data;
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

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        private static List<NewsItem> _newsList = new List<NewsItem>();

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, AppDbContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
        }

        public IActionResult Index()
        {
            // Pobieramy same premiery, bez dołączania gry (bo już nie ma powiązania)
            ViewBag.Premieres = _context.Premieres.OrderBy(p => p.ReleaseDate).ToList();

            ViewBag.Platforms = _context.Platforms.OrderBy(p => p.Name).ToList();
            ViewBag.Genres = _context.Genres.OrderBy(g => g.Name).ToList();

            return View(_newsList);
        }

        [HttpPost]
        public IActionResult AddPremiere(string title, DateTime releaseDate, int[] selectedPlatforms, int[] selectedGenres)
        {
            if (!User.IsInRole("Moderator")) return Challenge();

            // Pobieramy nazwy wybranych platform i gatunków z bazy
            var platformsList = selectedPlatforms != null
                ? _context.Platforms.Where(p => selectedPlatforms.Contains(p.Id)).Select(p => p.Name).ToList()
                : new List<string>();

            var genresList = selectedGenres != null
                ? _context.Genres.Where(g => selectedGenres.Contains(g.Id)).Select(g => g.Name).ToList()
                : new List<string>();

            // Tworzymy NIEZALEŻNĄ premierę (nie tworzy to już nowej Gry!)
            var newPremiere = new Premiere
            {
                Title = title,
                ReleaseDate = releaseDate,
                Platforms = platformsList.Any() ? string.Join(", ", platformsList) : "Brak",
                Genres = genresList.Any() ? string.Join(", ", genresList) : "Brak"
            };

            _context.Premieres.Add(newPremiere);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> GenerateAiNews()
        {
            var apiKey = _configuration["AiSettings:GeminiApiKey"];
            if (string.IsNullOrEmpty(apiKey)) return RedirectToAction("Index");

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);

            var prompt = "Jesteś ekspertem branży gier wideo. Wygeneruj 2 najnowsze newsy ze świata gier. " +
                         "Zwróć wynik TYLKO jako czystą tablicę JSON w formacie: " +
                         "[{\"Title\": \"tytuł newsa\", \"Summary\": \"treść...\", \"Source\": \"np. IGN, Gry-Online\"}]";

            var requestBody = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } },
                generationConfig = new { temperature = 0.7, responseMimeType = "application/json" }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash:generateContent", content);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(jsonResponse);
                try
                {
                    var resultText = document.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text").GetString();

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
                    _logger.LogError($"Błąd parsowania JSON: {ex.Message}");
                }
            }
            return RedirectToAction("Index");
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}