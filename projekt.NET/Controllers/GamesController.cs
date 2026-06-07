using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projekt.NET.Models.Entities;
using projekt.NET.Data;
namespace projekt.NET.Controllers
{
    public class GamesController : Controller
    {
        private readonly AppDbContext _context;

        public GamesController(AppDbContext context)
        {
            _context = context;
        }

        // Akcja wyświetlająca wszystkie gry wraz z filtrowaniem
        public IActionResult Index(int? genreId)
        {
            // Pobieramy gry z bazy wraz z dołączonymi danymi (relacjami) o Producencie i Gatunkach
            var gamesQuery = _context.Games
                .Include(g => g.Producer)
                .Include(g => g.Genres)
                .AsQueryable();

            // Jeżeli użytkownik wybrał gatunek (genreId), filtrujemy wyniki
            if (genreId.HasValue && genreId > 0)
            {
                // .Any() przechodzi po liście gatunków przypisanych do danej gry
                gamesQuery = gamesQuery.Where(g => g.Genres.Any(genre => genre.Id == genreId));
            }

            // Przekazujemy listę wszystkich gatunków do widoku (do wyświetlenia w elemencie <select>)
            
            ViewBag.Genres = _context.Genres.OrderBy(g => g.Name).ToList();
            ViewBag.SelectedGenre = genreId;

            var gamesList = gamesQuery.ToList();

            return View(gamesList);
        }
    }
}