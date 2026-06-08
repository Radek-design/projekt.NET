using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projekt.NET.Models.Entities;
using projekt.NET.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using projekt.NET.Models;

namespace projekt.NET.Controllers
{
    public class GamesController : Controller
    {
        private readonly AppDbContext _context;

        public GamesController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? genreId)
        {
            var gamesQuery = _context.Games
                .Include(g => g.Producer)
                .Include(g => g.Genres)
                .Include(g => g.Reviews)
                .AsQueryable();

            if (genreId.HasValue && genreId > 0)
            {
                gamesQuery = gamesQuery.Where(g => g.Genres.Any(genre => genre.Id == genreId));
            }

            ViewBag.Genres = _context.Genres.OrderBy(g => g.Name).ToList();
            ViewBag.Platforms = _context.Platforms.OrderBy(p => p.Name).ToList();
            ViewBag.Producers = _context.Producers.OrderBy(p => p.Name).ToList();
            ViewBag.SelectedGenre = genreId;

            var gamesList = gamesQuery.ToList();

            foreach (var game in gamesList)
            {
                game.AverageRating = game.Reviews.Any() ? game.Reviews.Average(r => r.Rating) : 0;
            }

            return View(gamesList);
        }

        [HttpPost]
        [Authorize(Roles = "Moderator")]
        public IActionResult Create(string title, DateTime releaseDate, string coverImagePath, string description, int producerId, int[] selectedPlatforms, int[] selectedGenres)
        {
            // ZABEZPIECZENIE: Sprawdza czy gra (bez zwracania uwagi na wielkość liter) już istnieje!
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
                ProducerId = producerId,
                AverageRating = 0
            };

            if (selectedPlatforms != null)
            {
                foreach (var pId in selectedPlatforms)
                {
                    var platform = _context.Platforms.Find(pId);
                    if (platform != null) newGame.Platforms.Add(platform);
                }
            }

            if (selectedGenres != null)
            {
                foreach (var gId in selectedGenres)
                {
                    var genre = _context.Genres.Find(gId);
                    if (genre != null) newGame.Genres.Add(genre);
                }
            }

            _context.Games.Add(newGame);
            _context.SaveChanges();

            TempData["SuccessMsg"] = $"Gra '{title}' została pomyślnie dodana do katalogu!";
            return RedirectToAction("Index");
        }

        public IActionResult Details(int id)
        {
            var game = _context.Games
                .Include(g => g.Producer)
                .Include(g => g.Genres)
                .Include(g => g.Platforms)
                .Include(g => g.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefault(g => g.Id == id);

            if (game == null) return NotFound();

            game.AverageRating = game.Reviews.Any() ? game.Reviews.Average(r => r.Rating) : 0;

            return View(game);
        }

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

            var game = _context.Games.Include(g => g.Reviews).FirstOrDefault(g => g.Id == gameId);
            if (game != null)
            {
                game.AverageRating = game.Reviews.Any() ? game.Reviews.Average(r => r.Rating) : 0;
                _context.SaveChanges();
            }

            return RedirectToAction("Details", new { id = gameId });
        }

        [HttpPost]
        [Authorize(Roles = "Moderator")]
        public IActionResult DeleteReview(int reviewId, int gameId)
        {
            var review = _context.Reviews.Find(reviewId);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                _context.SaveChanges();

                var game = _context.Games.Include(g => g.Reviews).FirstOrDefault(g => g.Id == gameId);
                if (game != null)
                {
                    game.AverageRating = game.Reviews.Any() ? game.Reviews.Average(r => r.Rating) : 0;
                    _context.SaveChanges();
                }
            }
            return RedirectToAction("Details", new { id = gameId });
        }

        [HttpPost]
        [Authorize(Roles = "Moderator")]
        public IActionResult DeleteGame(int id)
        {
            var game = _context.Games.Find(id);
            if (game != null)
            {
                var title = game.Title;

                // Usunięcie gry (dzięki relacjom w bazie, Entity Framework automatycznie 
                // usunie też przypisane do niej recenzje i usunie ją z bibliotek graczy)
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
    }
}