using Microsoft.AspNetCore.Mvc;
using projekt.NET.Models;

namespace projekt.NET.Controllers
{
    public class LibraryController : Controller
    {
        // Tymczasowa symulacja bazy danych
        private static List<UserGame> _myGames = new List<UserGame>
        {
            new UserGame { Id = 1, Title = "Wiedźmin 3: Dziki Gon", Status = "Ukończona", Rating = 10, PlayTimeHours = 120, ImageUrl = "https://images.igdb.com/igdb/image/upload/t_cover_big/co1wyy.jpg" },
            new UserGame { Id = 2, Title = "Cyberpunk 2077", Status = "W trakcie", Rating = 8, PlayTimeHours = 45, ImageUrl = "https://images.igdb.com/igdb/image/upload/t_cover_big/co2mvt.jpg" },
            new UserGame { Id = 3, Title = "Gothic", Status = "Ukończona", Rating = 9, PlayTimeHours = 60, ImageUrl = "https://images.igdb.com/igdb/image/upload/t_cover_big/co2k9x.jpg" }
        };

        // Widok główny biblioteki
        public IActionResult Index()
        {
            return View(_myGames);
        }

        // Akcja dodawania nowej gry (odbiera dane z formularza)
        [HttpPost]
        public IActionResult AddGame(UserGame newGame)
        {
            // Proste generowanie ID i przypisanie domyślnego obrazka, jeśli brak
            newGame.Id = _myGames.Count > 0 ? _myGames.Max(g => g.Id) + 1 : 1;

            if (string.IsNullOrWhiteSpace(newGame.ImageUrl))
            {
                newGame.ImageUrl = "https://images.igdb.com/igdb/image/upload/t_cover_big/nocover.png";
            }

            _myGames.Add(newGame);

            // Po dodaniu odświeżamy stronę
            return RedirectToAction("Index");
        }
    }
}