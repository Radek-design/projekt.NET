using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using projekt.NET.Models;

namespace projekt.NET.Controllers
{
    public class NewsItem
    {
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public DateTime PublishDate { get; set; }
        public string? ImageUrl { get; set; }
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

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Zapamiętane: Przykładowe wiadomości (w przyszłości podsumowywane automatycznie przez AI)
            var newsList = new List<NewsItem>
            {
                new NewsItem {
                    Title = "Wielki krok w stronę wirtualnej rzeczywistości",
                    Summary = "Zapowiedziano gogle nowej generacji oferujące rewolucyjną rozdzielczość oraz zaawansowane śledzenie ruchu oczu, co całkowicie odmieni rozgrywkę w grach RPG.",
                    PublishDate = DateTime.Now.AddDays(-1),
                    ImageUrl = "https://images.igdb.com/igdb/image/upload/t_720p/co671u.jpg"
                },
                new NewsItem {
                    Title = "Kontynuacja kultowej strategii oficjalnie potwierdzona",
                    Summary = "Studio deweloperskie ogłosiło powrót do legendarnego uniwersum. Premiera planowana jest na koniec przyszłego roku, a beta testy ruszą już niedługo.",
                    PublishDate = DateTime.Now.AddDays(-3),
                    ImageUrl = "https://images.igdb.com/igdb/image/upload/t_720p/co1rfi.jpg"
                }
            };

            // Nadchodzące premiery gier w tym roku (Prawa strona)
            var upcomingReleases = new List<UpcomingRelease>
            {
                new UpcomingRelease { Title = "Grand Theft Auto VI", ReleaseDate = "Jesień 2026", Platforms = "PS5, Xbox Series X/S" },
                new UpcomingRelease { Title = "Ghost of Yōtei", ReleaseDate = "Październik 2026", Platforms = "PS5" },
                new UpcomingRelease { Title = "Metroid Prime 4: Beyond", ReleaseDate = "Listopad 2026", Platforms = "Nintendo Switch" },
                new UpcomingRelease { Title = "Death Stranding 2: On The Beach", ReleaseDate = "Grudzień 2026", Platforms = "PS5" }
            };

            ViewBag.UpcomingReleases = upcomingReleases;

            return View(newsList);
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