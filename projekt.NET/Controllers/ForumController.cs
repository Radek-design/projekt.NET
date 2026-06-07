using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projekt.NET.Data;
using System.Linq;

namespace projekt.NET.Controllers
{
    public class ForumController : Controller
    {
        private readonly AppDbContext _context;

        public ForumController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Pobieramy prawdziwe statystyki z bazy
            ViewBag.UserCount = _context.Users.Count();

            // Założenie: Posiadasz model ForumPost i DbSet<ForumPost> w AppDbContext.
            // Jeśli nie, zakomentuj te linie póki nie stworzysz tabeli ForumPosts.
            ViewBag.PostCount = _context.ForumPosts.Count();

            // Pobieramy posty z dołączonymi danymi autora
            var posts = _context.ForumPosts
                .Include(f => f.User)
                .OrderByDescending(f => f.CreatedAt)
                .ToList();

            return View(posts);
        }
    }
}