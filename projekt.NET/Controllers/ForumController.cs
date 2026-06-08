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
            ViewBag.UserCount = await _context.Users.CountAsync();
            ViewBag.PostCount = await _context.ForumPosts.CountAsync();
            ViewBag.Games = await _context.Games.OrderBy(g => g.Title).ToListAsync();
            ViewBag.CurrentFilter = topicFilter;

            var query = _context.ForumPosts.Include(f => f.User).AsQueryable();

            if (!string.IsNullOrEmpty(topicFilter)) query = query.Where(p => p.Topic == topicFilter);

            var allPosts = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();

            bool isMod = User.IsInRole("Moderator");
            var userId = _userManager.GetUserId(User);

            var visiblePosts = allPosts.Where(p =>
                isMod || p.IsApproved || p.IsDeletedByModerator || (!p.IsApproved && p.UserId == userId)
            ).ToList();

            return View(visiblePosts);
        }

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
                IsApproved = false,
                IsDeletedByModerator = false
            };

            _context.ForumPosts.Add(post);
            await _context.SaveChangesAsync();

            TempData["ForumMsg"] = "Twój wpis został dodany i oczekuje na zatwierdzenie przez moderatora.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Moderator")]
        public async Task<IActionResult> ApprovePost(int id)
        {
            var post = await _context.ForumPosts.FindAsync(id);
            if (post != null) { post.IsApproved = true; await _context.SaveChangesAsync(); TempData["ForumMsg"] = "Wpis zatwierdzony publicznie."; }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Moderator")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.ForumPosts.FindAsync(id);
            if (post != null) { post.IsDeletedByModerator = true; await _context.SaveChangesAsync(); TempData["ForumMsg"] = "Wpis oznaczony jako usunięty."; }
            return RedirectToAction("Index");
        }

        // --- NOWE FUNKCJE DLA KOMENTARZY ---

        // 1. Podstrona szczegółów dyskusji z komentarzami
        public async Task<IActionResult> Details(int id)
        {
            var post = await _context.ForumPosts
                .Include(f => f.User)
                .Include(f => f.Comments)
                    .ThenInclude(c => c.User) // Wczytuje autorów poszczególnych komentarzy
                .FirstOrDefaultAsync(f => f.Id == id);

            if (post == null) return NotFound();

            return View(post);
        }

        // 2. Dodawanie komentarza (automatyczna akceptacja)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(int postId, string content)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null && !string.IsNullOrWhiteSpace(content))
            {
                var comment = new ForumComment
                {
                    ForumPostId = postId,
                    Content = content,
                    UserId = user.Id,
                    CreatedAt = DateTime.Now
                };
                _context.ForumComments.Add(comment);
                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = "Komentarz został opublikowany.";
            }
            return RedirectToAction("Details", new { id = postId });
        }

        // 3. Usuwanie komentarza przez Moderatora
        [HttpPost]
        [Authorize(Roles = "Moderator")]
        public async Task<IActionResult> DeleteComment(int commentId, int postId)
        {
            var comment = await _context.ForumComments.FindAsync(commentId);
            if (comment != null)
            {
                _context.ForumComments.Remove(comment);
                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = "Komentarz został usunięty.";
            }
            return RedirectToAction("Details", new { id = postId });
        }
    }
}