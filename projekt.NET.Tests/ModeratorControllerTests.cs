using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using projekt.NET.Controllers;
using projekt.NET.Models.DTOs;
using projekt.NET.Models.Entities;
using projekt.NET.Repositories.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace projekt.NET.Tests.Controllers
{
    public class ModeratorControllerTests
    {
        private readonly Mock<IGameRepository> _mockGameRepo;
        private readonly Mock<IProducerRepository> _mockProducerRepo;
        private readonly Mock<IPlatformRepository> _mockPlatformRepo;
        private readonly Mock<IGenreRepository> _mockGenreRepo;
        private readonly Mock<IWebHostEnvironment> _mockEnv;
        private readonly ModeratorController _controller;

        public ModeratorControllerTests()
        {
            // 1. Inicjalizacja mocków dla wszystkich zależności
            _mockGameRepo = new Mock<IGameRepository>();
            _mockProducerRepo = new Mock<IProducerRepository>();
            _mockPlatformRepo = new Mock<IPlatformRepository>();
            _mockGenreRepo = new Mock<IGenreRepository>();
            _mockEnv = new Mock<IWebHostEnvironment>();

            // 2. Wstrzyknięcie mocków do kontrolera
            _controller = new ModeratorController(
                _mockGameRepo.Object,
                _mockProducerRepo.Object,
                _mockPlatformRepo.Object,
                _mockGenreRepo.Object,
                _mockEnv.Object
            );
        }

        [Fact]
        public async Task Games_ReturnsViewResult_WithListOfGames()
        {
            // Arrange
            var games = new List<Game> { new Game { Id = 1, Title = "Testowa Gra" } };
            _mockGameRepo.Setup(repo => repo.GetAllGAsync()).ReturnsAsync(games);

            // Act
            var result = await _controller.Games();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Game>>(viewResult.ViewData.Model);
            Assert.Single(model);
        }

        [Fact]
        public async Task CreateGame_Get_ReturnsViewWithFilledViewBags()
        {
            // Arrange - Przygotowanie danych do ViewBags
            _mockProducerRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Producer> { new Producer { Id = 1, Name = "Prod" } });
            _mockPlatformRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Platform>());
            _mockGenreRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Genre>());

            // Act
            var result = await _controller.CreateGame();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<CreateEditGameDto>(viewResult.Model);
            Assert.NotNull(viewResult.ViewData["Producers"]);
            Assert.NotNull(viewResult.ViewData["Platforms"]);
            Assert.NotNull(viewResult.ViewData["Genres"]);
        }

        [Fact]
        public async Task CreateGame_PostInvalidModelState_ReturnsViewWithDto()
        {
            // Arrange - Symulacja błędu walidacji
            _controller.ModelState.AddModelError("Title", "Tytuł jest wymagany");
            var dto = new CreateEditGameDto();

            // ViewBagi będą na nowo ładowane w przypadku błędu walidacji
            _mockProducerRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Producer>());
            _mockPlatformRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Platform>());
            _mockGenreRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Genre>());

            // Act
            var result = await _controller.CreateGame(dto);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(dto, viewResult.Model);
        }

        [Fact]
        public async Task CreateGame_PostValidModelState_AddsGameAndRedirects()
        {
            // Arrange - Prawidłowe DTO
            var dto = new CreateEditGameDto
            {
                Title = "Nowa Gra",
                SelectedPlatformIds = new List<int> { 1 },
                SelectedGenreIds = new List<int> { 2 }
            };

            // Zwracamy encje platform i gatunków by mogły być przypisane do nowej gry
            _mockPlatformRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Platform> { new Platform { Id = 1 } });
            _mockGenreRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Genre> { new Genre { Id = 2 } });

            // Act
            var result = await _controller.CreateGame(dto);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Games", redirectResult.ActionName);

            // Weryfikujemy czy funkcja AddAsync w repozytorium została wywołana dokładnie jeden raz
            _mockGameRepo.Verify(r => r.AddAsync(It.Is<Game>(g =>
                g.Title == "Nowa Gra" &&
                g.Platforms.Count == 1 &&
                g.Genres.Count == 1)), Times.Once);
        }

        [Fact]
        public async Task DeleteGame_Post_DeletesAndRedirects()
        {
            // Act
            var result = await _controller.DeleteGame(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Games", redirectResult.ActionName);
            _mockGameRepo.Verify(r => r.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task Producers_ReturnsViewResult_WithListOfProducers()
        {
            // Arrange
            var producers = new List<Producer> { new Producer { Id = 1, Name = "Test Producer" } };
            _mockProducerRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(producers);

            // Act
            var result = await _controller.Producers();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Producer>>(viewResult.ViewData.Model);
            Assert.Single(model);
        }

        [Fact]
        public async Task CreateProducer_PostValidDto_AddsProducerAndRedirects()
        {
            // Arrange
            var dto = new ProducerDto { Name = "Testowy Producent", Country = "Polska" };

            // Act
            var result = await _controller.CreateProducer(dto);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Producers", redirectResult.ActionName);
            _mockProducerRepo.Verify(r => r.AddAsync(It.Is<Producer>(p => p.Name == "Testowy Producent")), Times.Once);
        }
    }
}