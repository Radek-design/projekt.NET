using Microsoft.AspNetCore.Mvc;
using projekt.NET.Data;
using projekt.NET.Models;

namespace projekt.NET.Controllers
{
    public class LibraryController : Controller
    {
        // Widok główny biblioteki
        public IActionResult Index()
        {
            return View(DataStorage.MyGames);
        }

        // Akcja dodawania nowej gry (odbiera dane z formularza)
        [HttpPost]
        public IActionResult AddGame(UserGame newGame)
        {
            // Proste generowanie ID i przypisanie domyślnego obrazka, jeśli brak
            newGame.Id = DataStorage.MyGames.Count > 0 ? DataStorage.MyGames.Max(g => g.Id) + 1 : 1;

            if (string.IsNullOrWhiteSpace(newGame.ImageUrl))
            {
                newGame.ImageUrl = "https://images.igdb.com/igdb/image/upload/t_cover_big/nocover.png";
            }

            DataStorage.MyGames.Add(newGame);

            // Po dodaniu odświeżamy stronę
            return RedirectToAction("Index");
        }
    }
}