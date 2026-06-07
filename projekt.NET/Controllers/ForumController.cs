using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projekt.NET.Data;
using projekt.NET.Models;
using projekt.NET.Models.Entities;

namespace projekt.NET.Controllers
{
    public class ForumController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ForumController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string topicFilter)
        {
            // Prawdziwe statystyki prosto z bazy danych
            ViewBag.UserCount = await _context.Users.CountAsync();
            ViewBag.PostCount = await _context.ForumPosts.CountAsync();

            // Tematy do listy wyboru przy tworzeniu wpisu (pobieramy wszystkie gry)
            ViewBag.Games = await _context.Games.OrderBy(g => g.Title).ToListAsync();
            ViewBag.CurrentFilter = topicFilter;

            var query = _context.ForumPosts.Include(f => f.User).AsQueryable();

            // Filtrowanie po wybranym temacie
            if (!string.IsNullOrEmpty(topicFilter))
            {
                query = query.Where(p => p.Topic == topicFilter);
            }

            var allPosts = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();

            // LOGIKA WIDOCZNOŚCI POSTÓW:
            bool isMod = User.IsInRole("Moderator");
            var userId = _userManager.GetUserId(User);

            var visiblePosts = allPosts.Where(p =>
                isMod ||                                         // Moderator widzi wszystko
                p.IsApproved ||                                  // Wszyscy widzą zatwierdzone wpisy
                p.IsDeletedByModerator ||                        // Wszyscy widzą powiadomienie o usunięciu
                (!p.IsApproved && p.UserId == userId)            // Autor widzi swój własny post (jako oczekujący)
            ).ToList();

            return View(visiblePosts);
        }

        // AKCJA: Tworzenie wpisu przez użytkownika
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePost(string title, string content, string topic)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var post = new ForumPost
            {
                Title = title,
                Content = content,
                Topic = topic,
                UserId = user.Id,
                CreatedAt = DateTime.Now,
                IsApproved = false, // Wymaga Twoich wytycznych: DOMYŚLNIE CZEKA NA ZATWIERDZENIE
                IsDeletedByModerator = false
            };

            _context.ForumPosts.Add(post);
            await _context.SaveChangesAsync();

            TempData["ForumMsg"] = "Twój wpis został dodany i oczekuje na zatwierdzenie przez moderatora.";
            return RedirectToAction("Index");
        }

        // AKCJA MODERATORA: Zatwierdzanie wpisu
        [HttpPost]
        [Authorize(Roles = "Moderator")]
        public async Task<IActionResult> ApprovePost(int id)
        {
            var post = await _context.ForumPosts.FindAsync(id);
            if (post != null)
            {
                post.IsApproved = true;
                await _context.SaveChangesAsync();
                TempData["ForumMsg"] = "Wpis został pomyślnie zatwierdzony i jest teraz widoczny publicznie.";
            }
            return RedirectToAction("Index");
        }

        // AKCJA MODERATORA: Usuwanie wpisu (Oflagowanie)
        [HttpPost]
        [Authorize(Roles = "Moderator")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.ForumPosts.FindAsync(id);
            if (post != null)
            {
                post.IsDeletedByModerator = true;
                await _context.SaveChangesAsync();
                TempData["ForumMsg"] = "Wpis został usunięty (ukryty dla użytkowników).";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(int postId, string content)
        {
            var user = await _userManager.GetUserAsync(User);
            var comment = new ForumComment
            {
                ForumPostId = postId,
                Content = content,
                UserId = user.Id,
                CreatedAt = DateTime.Now
            };

            _context.ForumComments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = postId }); // Musisz stworzyć akcję Details
        }

        public async Task<IActionResult> Details(int id)
        {
            var post = await _context.ForumPosts
                .Include(f => f.User)
                .Include(f => f.Comments)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(f => f.Id == id);

            return View(post);
        }
    }
}