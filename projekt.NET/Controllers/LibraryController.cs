using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projekt.NET.Data;
using projekt.NET.Models;
using projekt.NET.Models.Entities;
using System.Security.Claims;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace projekt.NET.Controllers
{
    [Authorize]
    public class LibraryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LibraryController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? genreId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var query = _context.UserGames
                .Include(ug => ug.Game)
                .ThenInclude(g => g.Genres)
                .Where(ug => ug.UserId == user.Id);

            if (genreId.HasValue && genreId > 0)
            {
                query = query.Where(ug => ug.Game.Genres.Any(g => g.Id == genreId));
            }

            var myGames = await query.ToListAsync();

            ViewBag.Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
            ViewBag.SelectedGenre = genreId;
            ViewBag.ProfilePicture = user.ProfilePictureUrl;

            // Statystyki - Oceny wyciągamy teraz tylko z RECENZJI użytkownika
            var userReviews = await _context.Reviews.Where(r => r.UserId == user.Id).ToListAsync();

            ViewBag.TotalGames = myGames.Count;
            ViewBag.TotalTime = myGames.Sum(ug => ug.PlayTimeHours);
            ViewBag.HighestRating = userReviews.Any() ? userReviews.Max(r => r.Rating) : 0;
            ViewBag.LowestRating = userReviews.Any() ? userReviews.Min(r => r.Rating) : 0;

            return View(myGames);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfilePicture(string avatarUrl)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null && !string.IsNullOrEmpty(avatarUrl))
            {
                user.ProfilePictureUrl = avatarUrl;
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction("Index");
        }

        // Usunięto parametr i logikę zapisywania Ratingu!
        [HttpPost]
        public IActionResult AddGame(int gameId, string status, int? playTimeHours)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var alreadyExists = _context.UserGames.Any(ug => ug.UserId == userId && ug.GameId == gameId);
            if (alreadyExists) return RedirectToAction("Index");

            if (status == "Planuje") { playTimeHours = 0; }

            var userGame = new UserGame
            {
                UserId = userId,
                GameId = gameId,
                Status = status ?? "Planuje",
                Rating = null, // Ocena tylko z poziomu recenzji
                PlayTimeHours = playTimeHours ?? 0
            };

            _context.UserGames.Add(userGame);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // Usunięto edycję Ratingu
        [HttpPost]
        public async Task<IActionResult> EditGame(int gameId, string status, int playTimeHours)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userGame = await _context.UserGames.FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GameId == gameId);

            if (userGame != null)
            {
                userGame.Status = status;
                userGame.PlayTimeHours = status == "Planuje" ? 0 : playTimeHours;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // GENEROWANIE RAPORTU DO PLIKU PDF
        public async Task<IActionResult> GenerateReport()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var games = await _context.UserGames.Include(ug => ug.Game).Where(ug => ug.UserId == userId).ToListAsync();
            var reviews = await _context.Reviews.Include(r => r.Game).Where(r => r.UserId == userId).ToListAsync();

            // Konfiguracja darmowej licencji QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Text("Moj Raport Biblioteki Gier")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Darken3);

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(x =>
                    {
                        x.Spacing(10);

                        // Podsumowanie
                        x.Item().Text("Podsumowanie Profilu:").SemiBold().FontSize(14);
                        x.Item().Text($"Liczba gier: {games.Count} | Laczny czas gry: {games.Sum(g => g.PlayTimeHours)} h");

                        // Lista Gier
                        x.Item().PaddingTop(10).Text("Gry w Twojej Bibliotece:").SemiBold().FontSize(14);
                        foreach (var g in games)
                        {
                            x.Item().Text($"- Tytul: {g.Game.Title} | Status: {g.Status} | Czas gry: {g.PlayTimeHours}h");
                        }

                        // Recenzje
                        x.Item().PaddingTop(15).Text("Napisane Recenzje:").SemiBold().FontSize(14);
                        if (reviews.Any())
                        {
                            foreach (var r in reviews)
                            {
                                x.Item().PaddingBottom(5).Column(rc =>
                                {
                                    rc.Item().Text($"Gra: {r.Game.Title} (Ocena: {r.Rating}/10)").SemiBold();
                                    rc.Item().Text($"\"{r.Content}\"").Italic();
                                });
                            }
                        }
                        else
                        {
                            x.Item().Text("Brak napisanych recenzji.");
                        }
                    });
                });
            });

            byte[] pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", "Raport_Biblioteki_Gier.pdf");
        }

        public async Task<IActionResult> MyReviews()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reviews = await _context.Reviews.Include(r => r.Game).Where(r => r.UserId == userId).OrderByDescending(r => r.CreatedAt).ToListAsync();
            return View(reviews);
        }

        public async Task<IActionResult> Screenshots(int? gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var query = _context.Screenshots.Include(s => s.Game).Where(s => s.UserId == userId);

            if (gameId.HasValue && gameId > 0) query = query.Where(s => s.GameId == gameId);

            ViewBag.Games = await _context.Games.OrderBy(g => g.Title).ToListAsync();
            ViewBag.SelectedGame = gameId;

            return View(await query.OrderByDescending(s => s.CreatedAt).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> AddScreenshot(string imagePath, int gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null && !string.IsNullOrEmpty(imagePath))
            {
                var screenshot = new Screenshot { UserId = userId, GameId = gameId, ImagePath = imagePath, CreatedAt = DateTime.Now };
                _context.Screenshots.Add(screenshot);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Screenshots");
        }
    }
}