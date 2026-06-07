using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projekt.NET.Models;
using projekt.NET.Models.Entities;
using System.Linq;
using System.Security.Claims;
using projekt.NET.Data;
namespace projekt.NET.Controllers
{
    public class LibraryController : Controller
    {
        private readonly AppDbContext _context;

        // Wstrzykujemy bazę danych przez konstruktor
        public LibraryController(AppDbContext context)
        {
            _context = context;
        }

        // Widok główny biblioteki - wyświetla gry zalogowanego użytkownika
        public IActionResult Index()
        {
            // Pobieramy ID aktualnie zalogowanego użytkownika (z Identity)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                // Jeśli użytkownik nie jest zalogowany, możemy pokazać pustą listę lub przekierować do logowania
                return View(new List<UserGame>());
            }

            // Pobieramy z bazy gry użytkownika, dołączając dane o samej grze (Include), aby mieć dostęp do Tytułu i Okładki
            var myGames = _context.UserGames
                .Include(ug => ug.Game)
                .Where(ug => ug.UserId == userId)
                .ToList();

            return View(myGames);
        }

        // Akcja dodawania gry do biblioteki użytkownika
        [HttpPost]
        public IActionResult AddGame(int gameId, string status, int? rating, int playTimeHours)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Challenge(); // Wymaga zalogowania
            }

            // Sprawdzamy, czy użytkownik nie ma już tej gry w swojej bibliotece
            var alreadyExists = _context.UserGames.Any(ug => ug.UserId == userId && ug.GameId == gameId);
            if (alreadyExists)
            {
                ModelState.AddModelError("", "Ta gra jest już w Twojej bibliotece.");
                return RedirectToAction("Index");
            }

            // Tworzymy nowy obiekt powiązania
            var userGame = new UserGame
            {
                UserId = userId,
                GameId = gameId,
                Status = status ?? "W trakcie",
                Rating = rating,
                PlayTimeHours = playTimeHours
            };

            // Baza danych automatycznie wygeneruje nowe ID dla wpisu
            _context.UserGames.Add(userGame);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}