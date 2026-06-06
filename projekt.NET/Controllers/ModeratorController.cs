using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using projekt.NET.Models.DTOs;
using projekt.NET.Models.Entities;
using projekt.NET.Repositories.Interface;
using projekt.NET.Services;


namespace projekt.NET.Controllers
{
    // [Authorize(Roles = "Moderator")] - tylko moderator ma dostęp do tych akcji
    [Authorize(Roles = "Moderator")]
    public class ModeratorController : Controller
    {
        private readonly IGameRepository _gameRepo;
        private readonly IProducerRepository _producerRepo;
        private readonly IPlatformRepository _platformRepo;
        private readonly IGenreRepository _genreRepo;
        private readonly IWebHostEnvironment _env;

        public ModeratorController(
            IGameRepository gameRepo,
            IProducerRepository producerRepo,
            IPlatformRepository platformRepo,
            IGenreRepository genreRepo,
            IWebHostEnvironment env)
        {
            _gameRepo = gameRepo;
            _producerRepo = producerRepo;
            _platformRepo = platformRepo;
            _genreRepo = genreRepo;
            _env = env;
        }

        // GRY

        // GET: /Moderator/Games - lista wszystkich gier
        public async Task<IActionResult> Games()
        {
            var games = await _gameRepo.GetAllGAsync();
            return View(games);
        }

        // GET: /Moderator/CreateGame - formularz dodawania gry
        public async Task<IActionResult> CreateGame()
        {
            await FillViewBags();
            return View(new CreateEditGameDto());
        }

        // POST: /Moderator/CreateGame - zapisuje nową grę do bazy
        [HttpPost]
        public async Task<IActionResult> CreateGame(CreateEditGameDto dto)
        {
            if (!ModelState.IsValid)
            {
                await FillViewBags();
                return View(dto);
            }

            var game = new Game
            {
                Title = dto.Title,
                Description = dto.Description,
                ReleaseDate = dto.ReleaseDate,
                ProducerId = dto.ProducerId,
                CoverImagePath = await SaveFileAsync(dto.CoverImage)
            };

            // Przypisujemy wybrane platformy i gatunki
            var platforms = await _platformRepo.GetAllAsync();
            var genres = await _genreRepo.GetAllAsync();
            game.Platforms = platforms.Where(p => dto.SelectedPlatformIds.Contains(p.Id)).ToList();
            game.Genres = genres.Where(g => dto.SelectedGenreIds.Contains(g.Id)).ToList();

            await _gameRepo.AddAsync(game);
            return RedirectToAction(nameof(Games));
        }

        // GET: /Moderator/EditGame/5 - formularz edycji gry
        public async Task<IActionResult> EditGame(int id)
        {
            var game = await _gameRepo.GetByIdAsync(id);
            if (game == null) return NotFound();

            await FillViewBags();
            var dto = new CreateEditGameDto
            {
                Id = game.Id,
                Title = game.Title,
                Description = game.Description,
                ReleaseDate = game.ReleaseDate,
                ProducerId = game.ProducerId,
                CoverImagePath = game.CoverImagePath,
                SelectedPlatformIds = game.Platforms.Select(p => p.Id).ToList(),
                SelectedGenreIds = game.Genres.Select(g => g.Id).ToList()
            };
            return View(dto);
        }

        // POST: /Moderator/EditGame - zapisuje zmiany w grze
        [HttpPost]
        public async Task<IActionResult> EditGame(CreateEditGameDto dto)
        {
            if (!ModelState.IsValid)
            {
                await FillViewBags();
                return View(dto);
            }

            var game = await _gameRepo.GetByIdAsync(dto.Id);
            if (game == null) return NotFound();

            game.Title = dto.Title;
            game.Description = dto.Description;
            game.ReleaseDate = dto.ReleaseDate;
            game.ProducerId = dto.ProducerId;

            // Jeśli przesłano nową okładkę, zapisujemy ją
            if (dto.CoverImage != null)
                game.CoverImagePath = await SaveFileAsync(dto.CoverImage);

            var platforms = await _platformRepo.GetAllAsync();
            var genres = await _genreRepo.GetAllAsync();
            game.Platforms = platforms.Where(p => dto.SelectedPlatformIds.Contains(p.Id)).ToList();
            game.Genres = genres.Where(g => dto.SelectedGenreIds.Contains(g.Id)).ToList();

            await _gameRepo.UpdateAsync(game);
            return RedirectToAction(nameof(Games));
        }

        // POST: /Moderator/DeleteGame/5 - usuwa grę z bazy
        [HttpPost]
        public async Task<IActionResult> DeleteGame(int id)
        {
            await _gameRepo.DeleteAsync(id);
            return RedirectToAction(nameof(Games));
        }

        //  PRODUCENCI 

        public async Task<IActionResult> Producers()
        {
            var producers = await _producerRepo.GetAllAsync();
            return View(producers);
        }

        public IActionResult CreateProducer() => View(new ProducerDto());

        [HttpPost]
        public async Task<IActionResult> CreateProducer(ProducerDto dto)
        {
            var producer = new Producer
            {
                Name = dto.Name,
                Country = dto.Country,
                LogoPath = await SaveFileAsync(dto.LogoFile)
            };
            await _producerRepo.AddAsync(producer);
            return RedirectToAction(nameof(Producers));
        }

        public async Task<IActionResult> EditProducer(int id)
        {
            var producer = await _producerRepo.GetByIdAsync(id);
            if (producer == null) return NotFound();
            return View(new ProducerDto { Id = producer.Id, Name = producer.Name, Country = producer.Country, LogoPath = producer.LogoPath });
        }

        [HttpPost]
        public async Task<IActionResult> EditProducer(ProducerDto dto)
        {
            var producer = await _producerRepo.GetByIdAsync(dto.Id);
            if (producer == null) return NotFound();
            producer.Name = dto.Name;
            producer.Country = dto.Country;
            if (dto.LogoFile != null)
                producer.LogoPath = await SaveFileAsync(dto.LogoFile);
            await _producerRepo.UpdateAsync(producer);
            return RedirectToAction(nameof(Producers));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProducer(int id)
        {
            await _producerRepo.DeleteAsync(id);
            return RedirectToAction(nameof(Producers));
        }

        // PLATFORMY 

        public async Task<IActionResult> Platforms()
        {
            var platforms = await _platformRepo.GetAllAsync();
            return View(platforms);
        }

        public IActionResult CreatePlatform() => View(new PlatformGenreDto());

        [HttpPost]
        public async Task<IActionResult> CreatePlatform(PlatformGenreDto dto)
        {
            await _platformRepo.AddAsync(new Platform { Name = dto.Name });
            return RedirectToAction(nameof(Platforms));
        }

        public async Task<IActionResult> EditPlatform(int id)
        {
            var platform = await _platformRepo.GetByIdAsync(id);
            if (platform == null) return NotFound();
            return View(new PlatformGenreDto { Id = platform.Id, Name = platform.Name });
        }

        [HttpPost]
        public async Task<IActionResult> EditPlatform(PlatformGenreDto dto)
        {
            var platform = await _platformRepo.GetByIdAsync(dto.Id);
            if (platform == null) return NotFound();
            platform.Name = dto.Name;
            await _platformRepo.UpdateAsync(platform);
            return RedirectToAction(nameof(Platforms));
        }

        [HttpPost]
        public async Task<IActionResult> DeletePlatform(int id)
        {
            await _platformRepo.DeleteAsync(id);
            return RedirectToAction(nameof(Platforms));
        }

        // GATUNKI

        public async Task<IActionResult> Genres()
        {
            var genres = await _genreRepo.GetAllAsync();
            return View(genres);
        }

        public IActionResult CreateGenre() => View(new PlatformGenreDto());

        [HttpPost]
        public async Task<IActionResult> CreateGenre(PlatformGenreDto dto)
        {
            await _genreRepo.AddAsync(new Genre { Name = dto.Name });
            return RedirectToAction(nameof(Genres));
        }

        public async Task<IActionResult> EditGenre(int id)
        {
            var genre = await _genreRepo.GetByIdAsync(id);
            if (genre == null) return NotFound();
            return View(new PlatformGenreDto { Id = genre.Id, Name = genre.Name });
        }

        [HttpPost]
        public async Task<IActionResult> EditGenre(PlatformGenreDto dto)
        {
            var genre = await _genreRepo.GetByIdAsync(dto.Id);
            if (genre == null) return NotFound();
            genre.Name = dto.Name;
            await _genreRepo.UpdateAsync(genre);
            return RedirectToAction(nameof(Genres));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteGenre(int id)
        {
            await _genreRepo.DeleteAsync(id);
            return RedirectToAction(nameof(Genres));
        }

        // Wypełnia listy rozwijane w formularzu gry
        private async Task FillViewBags()
        {
            ViewBag.Producers = new SelectList(await _producerRepo.GetAllAsync(), "Id", "Name");
            ViewBag.Platforms = await _platformRepo.GetAllAsync();
            ViewBag.Genres = await _genreRepo.GetAllAsync();
        }

        // Zapisuje przesłany plik na serwer i zwraca ścieżkę
        private async Task<string?> SaveFileAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            // Zapisujemy pliki w folderze wwwroot/uploads
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            // Unikalna nazwa pliku żeby uniknąć konfliktów
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return "/uploads/" + fileName;
        }
    
    // GET: /Moderator/ImportGame?name=Cyberpunk - pobiera dane gry z RAWG
[HttpGet]
        public async Task<IActionResult> ImportGame(string name, [FromServices] RawgService rawgService)
        {
            if (string.IsNullOrEmpty(name)) return BadRequest();

            var result = await rawgService.SearchGameAsync(name);
            if (result == null) return NotFound();

            // Zwracamy dane jako JSON - formularz w przeglądarce je wczyta
            return Json(new
            {
                title = result.Name,
                releaseDate = result.Released,
                coverUrl = result.Background_Image,
                rating = result.Rating,
                platforms = result.Platforms?.Select(p => p.Platform?.Name),
                genres = result.Genres?.Select(g => g.Name)
            });
        }
    }
}