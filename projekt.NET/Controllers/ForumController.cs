using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using projekt.NET.Data;
using projekt.NET.Models;
using projekt.NET.Models.Entities;

namespace projekt.NET.Controllers
{
    public class ForumController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ForumController(AppDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
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
        // Zmiana: usunięto znak '?' przy IFormFile, brak tego znaku jest bezpieczniejszy dla kompilatora
        public async Task<IActionResult> CreatePost(string title, string content, string topic, IFormFile imageFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            string uploadedImagePath = null;

            if (imageFile != null && imageFile.Length > 0)
            {
                // Zapisujemy pliki do folderu wwwroot/uploads/forum
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "forum");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                uploadedImagePath = "/uploads/forum/" + uniqueFileName;
            }

            var post = new ForumPost
            {
                Title = title,
                Content = content,
                Topic = topic,
                ImagePath = uploadedImagePath, // Zapis ścieżki
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

        public async Task<IActionResult> Details(int id)
        {
            var post = await _context.ForumPosts
                .Include(f => f.User)
                .Include(f => f.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (post == null) return NotFound();

            return View(post);
        }

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