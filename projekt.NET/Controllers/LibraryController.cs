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
using Microsoft.AspNetCore.Hosting; // Wymagane do plików
using System.IO; // Wymagane do plików

namespace projekt.NET.Controllers
{
    [Authorize]
    public class LibraryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment; // Pozwala pobrać ścieżkę do wwwroot

        public LibraryController(AppDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
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

            var userReviews = await _context.Reviews.Where(r => r.UserId == user.Id).ToListAsync();

            ViewBag.TotalGames = myGames.Count;
            ViewBag.TotalTime = myGames.Sum(ug => ug.PlayTimeHours);
            ViewBag.HighestRating = userReviews.Any() ? userReviews.Max(r => r.Rating) : 0;
            ViewBag.LowestRating = userReviews.Any() ? userReviews.Min(r => r.Rating) : 0;

            return View(myGames);
        }

        // ZMIANA: Obsługa fizycznego pliku awatara
        [HttpPost]
        public async Task<IActionResult> UpdateProfilePicture(IFormFile avatarFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null && avatarFile != null && avatarFile.Length > 0)
            {
                // 1. Ustal ścieżkę do folderu zapisu w wwwroot/uploads/avatars
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "avatars");

                // 2. Jeśli folder nie istnieje, stwórz go
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                // 3. Stwórz unikalną nazwę pliku, żeby użytkownicy nie nadpisywali sobie zdjęć nawzajem
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(avatarFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // 4. Skopiuj plik na serwer
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(fileStream);
                }

                // 5. Zapisz ścieżkę w profilu użytkownika
                user.ProfilePictureUrl = "/uploads/avatars/" + uniqueFileName;
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction("Index");
        }

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
                Rating = null,
                PlayTimeHours = playTimeHours ?? 0
            };

            _context.UserGames.Add(userGame);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

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

        public async Task<IActionResult> GenerateReport()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId); // Pobieramy nazwę gracza
            var games = await _context.UserGames.Include(ug => ug.Game).Where(ug => ug.UserId == userId).ToListAsync();
            var reviews = await _context.Reviews.Include(r => r.Game).Where(r => r.UserId == userId).ToListAsync();

            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(0); // Zerujemy główny margines, aby nagłówek był na całą szerokość
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                    // ================= HEADER (Nagłówek) =================
                    page.Header()
                        .Background(Colors.Blue.Darken3)
                        .PaddingVertical(20)
                        .PaddingHorizontal(30)
                        .Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Kolekcja Gier").FontSize(26).FontColor(Colors.White).SemiBold();
                                col.Item().Text($"Użytkownik: {user?.UserName ?? "Gracz"}").FontSize(14).FontColor(Colors.Blue.Lighten4);
                            });

                            row.AutoItem().AlignRight().Column(col =>
                            {
                                col.Item().Text($"Wygenerowano:").FontSize(10).FontColor(Colors.White).AlignRight();
                                col.Item().Text($"{DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(12).FontColor(Colors.White).SemiBold().AlignRight();
                            });
                        });

                    // ================= ZAWARTOŚĆ =================
                    page.Content().Padding(30).Column(x =>
                    {
                        x.Spacing(20); // Odstępy między głównymi sekcjami

                        // 1. Podsumowanie statystyk w szarej ramce
                        x.Item().Background(Colors.Grey.Lighten4).BorderLeft(5).BorderColor(Colors.Blue.Darken2).Padding(15).Column(c =>
                        {
                            c.Spacing(5);
                            c.Item().Text("Szybkie statystyki").FontSize(16).SemiBold().FontColor(Colors.Blue.Darken2);
                            c.Item().Text($"Całkowita liczba gier w bibliotece: {games.Count}").FontSize(12);
                            c.Item().Text($"Łączny czas spędzony w grach: {games.Sum(g => g.PlayTimeHours)} godzin").FontSize(12);
                        });

                        // 2. Tabela Gier
                        x.Item().Text("Szczegóły Biblioteki").FontSize(18).SemiBold().FontColor(Colors.Black);

                        if (games.Any())
                        {
                            x.Item().Table(table =>
                            {
                                // Definicja szerokości kolumn
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(30);  // Lp.
                                    columns.RelativeColumn(3);   // Tytuł
                                    columns.RelativeColumn(2);   // Status
                                    columns.ConstantColumn(80);  // Czas (h)
                                });

                                // Nagłówek Tabeli
                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("#").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Tytuł Gry").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Obecny Status").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text("Czas (h)").SemiBold();
                                });

                                // Wypełnianie tabeli grami
                                int lp = 1;
                                foreach (var g in games)
                                {
                                    // Obramowanie pod każdym wierszem
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(lp.ToString());
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(g.Game.Title).SemiBold();
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(g.Status);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignCenter().Text(g.PlayTimeHours.ToString());
                                    lp++;
                                }
                            });
                        }
                        else
                        {
                            x.Item().Text("Brak gier w bibliotece.").Italic().FontColor(Colors.Grey.Medium);
                        }

                        // 3. Elegancka sekcja napisanych recenzji
                        x.Item().PaddingTop(15).Text("Napisane Recenzje").FontSize(18).SemiBold().FontColor(Colors.Black);

                        if (reviews.Any())
                        {
                            foreach (var r in reviews)
                            {
                                // Recenzje jako stylizowane karty
                                x.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(rc =>
                                {
                                    rc.Item().Row(r_row => {
                                        r_row.RelativeItem().Text(r.Game.Title).FontSize(13).SemiBold();
                                        r_row.AutoItem().Text($"Ocena: {r.Rating}/10").SemiBold().FontColor(Colors.Orange.Darken2);
                                    });
                                    rc.Item().PaddingTop(5).Text($"\"{r.Content}\"").Italic().FontColor(Colors.Grey.Darken3);
                                    rc.Item().PaddingTop(5).Text($"Napisano: {r.CreatedAt:dd.MM.yyyy}").FontSize(9).FontColor(Colors.Grey.Medium);
                                });
                            }
                        }
                        else
                        {
                            x.Item().Text("Brak napisanych recenzji.").Italic().FontColor(Colors.Grey.Medium);
                        }
                    });

                    // ================= FOOTER (Stopka z numerami stron) =================
                    page.Footer()
                        .PaddingVertical(10)
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Strona ").FontColor(Colors.Grey.Medium).FontSize(10);
                            x.CurrentPageNumber().FontColor(Colors.Grey.Medium).FontSize(10);
                            x.Span(" z ").FontColor(Colors.Grey.Medium).FontSize(10);
                            x.TotalPages().FontColor(Colors.Grey.Medium).FontSize(10);
                        });
                });
            });

            byte[] pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"Raport_{user?.UserName ?? "Gier"}.pdf");
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

        // ZMIANA: Obsługa fizycznego zrzutu ekranu z dysku
        [HttpPost]
        public async Task<IActionResult> AddScreenshot(IFormFile screenshotFile, int gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId != null && screenshotFile != null && screenshotFile.Length > 0)
            {
                // Zapisujemy screeny w wwwroot/uploads/screenshots
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "screenshots");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(screenshotFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await screenshotFile.CopyToAsync(fileStream);
                }

                var screenshot = new Screenshot
                {
                    UserId = userId,
                    GameId = gameId,
                    ImagePath = "/uploads/screenshots/" + uniqueFileName, // Zapis ścieżki do bazy
                    CreatedAt = DateTime.Now
                };

                _context.Screenshots.Add(screenshot);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Screenshots");
        }
    }
}