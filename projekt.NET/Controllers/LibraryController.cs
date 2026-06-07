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

        public LibraryController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return View(new List<UserGame>());
            }

            var myGames = _context.UserGames
                .Include(ug => ug.Game)
                .Where(ug => ug.UserId == userId)
                .ToList();

            return View(myGames);
        }

        [HttpPost]
        public IActionResult AddGame(int gameId, string status, int? rating, int? playTimeHours) // Dodano '?' przy int, aby uodpornić na pusty input
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Challenge();
            }

            var alreadyExists = _context.UserGames.Any(ug => ug.UserId == userId && ug.GameId == gameId);
            if (alreadyExists)
            {
                ModelState.AddModelError("", "Ta gra jest już w Twojej bibliotece.");
                return RedirectToAction("Index");
            }

            // Jeśli status to "Planuje", zerujemy oceny i czas
            if (status == "Planuje")
            {
                rating = null;
                playTimeHours = 0;
            }

            var userGame = new UserGame
            {
                UserId = userId,
                GameId = gameId,
                Status = status ?? "Planuje",
                Rating = rating,
                PlayTimeHours = playTimeHours ?? 0
            };

            _context.UserGames.Add(userGame);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}