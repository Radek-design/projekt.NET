using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projekt.NET.Models.Entities;
using projekt.NET.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using projekt.NET.Models;
using projekt.NET.Services;

namespace projekt.NET.Controllers
{
    public class GamesController : Controller
    {
        private readonly AppDbContext _context;

        public GamesController(AppDbContext context)
        {
            _context = context;
        }

        // Pokazuje główną listę gier
        public IActionResult Index(int? genreId)
        {
            // Dociąga do gier od razu producentów, gatunki i recenzje żeby nie robić osobnych zapytań
            var gamesQuery = _context.Games
                .Include(g => g.Producer)
                .Include(g => g.Genres)
                .Include(g => g.Reviews)
                .AsQueryable();

            // Filtrowanie po gatunku (jeśli user coś kliknął z boku)
            if (genreId.HasValue && genreId > 0)
            {
                gamesQuery = gamesQuery.Where(g => g.Genres.Any(genre => genre.Id == genreId));
            }
            // Pakuje listy słownikowe do ViewBaga dla filtrów w widoku
            ViewBag.Genres = _context.Genres.OrderBy(g => g.Name).ToList();
            ViewBag.Platforms = _context.Platforms.OrderBy(p => p.Name).ToList();
            ViewBag.Producers = _context.Producers.OrderBy(p => p.Name).ToList();
            ViewBag.SelectedGenre = genreId;

            var gamesList = gamesQuery.ToList();

            // Na piechote liczy średnią z recenzji dla każdej gry
            foreach (var game in gamesList)
            {
                game.AverageRating = game.Reviews.Any() ? game.Reviews.Average(r => r.Rating) : 0;
            }

            return View(gamesList);
        }

        // Mega kombajn do dodawania gier (tylko dla moderacji)
        [HttpPost]
        [Authorize(Roles = "Moderator")]
        public IActionResult Create(
            string title, DateTime releaseDate, string coverImagePath, string description,
            int? producerId, string? newProducerName,
            int[]? selectedPlatforms, string? newPlatforms,
            int[]? selectedGenres, string? newGenres)
        {
            // Blokuje dodanie tej samej gry drugi raz (sprawdza po tytule)
            bool gameExists = _context.Games.Any(g => g.Title.ToLower() == title.ToLower());
            if (gameExists)
            {
                TempData["ErrorMsg"] = $"Gra o tytule '{title}' znajduje się już w Twoim katalogu!";
                return RedirectToAction("Index");
            }

            var newGame = new Game
            {
                Title = title,
                ReleaseDate = releaseDate,
                CoverImagePath = coverImagePath,
                Description = description,
                AverageRating = 0
            };

            // 1. OBSŁUGA PRODUCENTA (z listy LUB nowego)
            if (!string.IsNullOrWhiteSpace(newProducerName))
            {
                var producer = _context.Producers.FirstOrDefault(p => p.Name.ToLower() == newProducerName.ToLower().Trim());
                if (producer == null)
                {
                    // Jak go nie ma w bazie, to go tworzy na poczekaniu
                    producer = new Producer { Name = newProducerName.Trim(), Country = "Nieznany" };
                    _context.Producers.Add(producer);
                    _context.SaveChanges(); // Zapisujemy żeby wygenerowało mu ID
                }
                newGame.ProducerId = producer.Id;
            }
            // Wybrany z listy rozwijanej
            else if (producerId.HasValue && producerId > 0)
            {
                newGame.ProducerId = producerId.Value;
            }
            else
            {
                TempData["ErrorMsg"] = "Musisz wybrać producenta z listy lub wpisać nowego!";
                return RedirectToAction("Index");
            }

            // 2. OBSŁUGA PLATFORM
            if (selectedPlatforms != null)
            {
                // Przypisuje zaznaczone checkboxy z platformami
                foreach (var pId in selectedPlatforms)
                {
                    var platform = _context.Platforms.Find(pId);
                    if (platform != null) newGame.Platforms.Add(platform);
                }
            }
            if (!string.IsNullOrWhiteSpace(newPlatforms))
            {
                // Dodaje też te wpisane z palca po przecinku
                var platformNames = newPlatforms.Split(',').Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p));
                foreach (var pName in platformNames)
                {
                    var platform = _context.Platforms.FirstOrDefault(p => p.Name.ToLower() == pName.ToLower());
                    if (platform == null)
                    {
                        platform = new Platform { Name = pName };
                        _context.Platforms.Add(platform); // Automatyczne dodanie do bazy
                    }
                    if (!newGame.Platforms.Contains(platform)) newGame.Platforms.Add(platform);
                }
            }

            // 3. OBSŁUGA GATUNKÓW (analogicznie do platform)
            if (selectedGenres != null)
            {
                foreach (var gId in selectedGenres)
                {
                    var genre = _context.Genres.Find(gId);
                    if (genre != null) newGame.Genres.Add(genre);
                }
            }
            if (!string.IsNullOrWhiteSpace(newGenres))
            {
                var genreNames = newGenres.Split(',').Select(g => g.Trim()).Where(g => !string.IsNullOrEmpty(g));
                foreach (var gName in genreNames)
                {
                    var genre = _context.Genres.FirstOrDefault(g => g.Name.ToLower() == gName.ToLower());
                    if (genre == null)
                    {
                        genre = new Genre { Name = gName };
                        _context.Genres.Add(genre); // Automatyczne dodanie do bazy
                    }
                    if (!newGame.Genres.Contains(genre)) newGame.Genres.Add(genre);
                }
            }

            // Zapisuje nową grę do bazy
            _context.Games.Add(newGame);
            _context.SaveChanges();

            TempData["SuccessMsg"] = $"Gra '{title}' została pomyślnie dodana do katalogu!";
            return RedirectToAction("Index");
        }

        // Szczegóły gry - wczytuje wszystko co z nią powiązane
        public IActionResult Details(int id)
        {
            var game = _context.Games
                .Include(g => g.Producer)
                .Include(g => g.Genres)
                .Include(g => g.Platforms)
                .Include(g => g.Reviews)
                    .ThenInclude(r => r.User) // Widok autorów recenzji
                .FirstOrDefault(g => g.Id == id);

            if (game == null) return NotFound();

            // Przed rzuceniem na widok odświeża jej średnią z recenzji
            game.AverageRating = game.Reviews.Any() ? game.Reviews.Average(r => r.Rating) : 0;

            return View(game);
        }

        // Akcja do dodawania recenzji przez zwykłych userów
        [HttpPost]
        [Authorize]
        public IActionResult AddReview(int gameId, int rating, string content)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Challenge();

            var review = new Review
            {
                GameId = gameId,
                UserId = userId,
                Rating = rating,
                Content = content ?? string.Empty,
                CreatedAt = DateTime.Now
            };

            _context.Reviews.Add(review);
            _context.SaveChanges();

            // --- ZMIANA OSOBA 1: Wywołanie procedury składowanej ---
            // Zamiast ręcznie wyliczać średnią, zlecamy to bazie danych.
            _context.Database.ExecuteSqlRaw("CALL sp_UpdateGameAverageRating({0})", gameId);
            // --------------------------------------------------------

            return RedirectToAction("Details", new { id = gameId });
        }

        // Usuwanie niechcianych recenzji (tylko mod)
        [HttpPost]
        [Authorize(Roles = "Moderator")]
        public IActionResult DeleteReview(int reviewId, int gameId)
        {
            var review = _context.Reviews.Find(reviewId);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                _context.SaveChanges();

                // --- ZMIANA OSOBA 1: Wywołanie procedury składowanej ---
                // Aktualizujemy średnią ocen po usunięciu recenzji
                _context.Database.ExecuteSqlRaw("CALL sp_UpdateGameAverageRating({0})", gameId);
                // --------------------------------------------------------
            }
            return RedirectToAction("Details", new { id = gameId });
        }

        // Twarde usunięcie gry z systemu
        [HttpPost]
        [Authorize(Roles = "Moderator")]
        public IActionResult DeleteGame(int id)
        {
            var game = _context.Games.Find(id);
            if (game != null)
            {
                var title = game.Title;
                _context.Games.Remove(game);
                _context.SaveChanges();
                TempData["SuccessMsg"] = $"Gra '{title}' oraz wszystkie powiązane z nią dane zostały pomyślnie usunięte z bazy.";
            }
            else
            {
                TempData["ErrorMsg"] = "Nie znaleziono gry do usunięcia.";
            }

            return RedirectToAction("Index");
        }

        // Funkcja komunikująca się z RAWG przez AJAX
        [HttpGet]
        [Authorize(Roles = "Moderator")]
        public async Task<IActionResult> ImportFromRawg(string name, [FromServices] RawgService rawgService)
        {
            if (string.IsNullOrEmpty(name)) return BadRequest();

            var result = await rawgService.SearchGameAsync(name);
            if (result == null) return NotFound();

            // Zwracamy dane jako JSON do formularza na froncie
            return Json(new
            {
                title = result.Name,
                releaseDate = result.Released,
                coverUrl = result.Background_Image,
                rating = result.Rating,
                platforms = result.Platforms?.Select(p => p.Platform?.Name),
                genres = result.Genres?.Select(g => g.Name)
            });
        }
    }
}