using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using projekt.NET.Controllers;
using projekt.NET.Data;
using projekt.NET.Models;
using projekt.NET.Models.Entities;
using Xunit;

namespace projekt.NET.Tests
{
    // Testy jednostkowe dla GamesController, sprawdzające logikę działania akcji bez konieczności uruchamiania całej aplikacji
    public class GamesControllerTests
    {
        // Metoda pomocnicza do tworzenia kontekstu bazy danych w pamięci, co pozwala na testowanie logiki bez wpływu na rzeczywistą bazę danych
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var db = new AppDbContext(options);
            db.Database.EnsureCreated();
            return db;
        }

        // Metoda pomocnicza do tworzenia instancji GamesController z odpowiednim kontekstem użytkownika, co pozwala na testowanie akcji wymagających uwierzytelnienia i autoryzacji
        private GamesController GetControllerWithContext(AppDbContext dbContext, string role = "User", string userId = "user1")
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, role)
            }, "mock"));

            var httpContext = new DefaultHttpContext { User = user };
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            // Tworzymy instancję GamesController z ustawionym kontekstem HTTP i TempData, co pozwala na testowanie akcji, które korzystają z tych elementów
            return new GamesController(dbContext)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext },
                TempData = tempData
            };
        }

        // Test sprawdzający, czy metoda Index zwraca wszystkie gry z poprawnie obliczonym średnim ratingiem na podstawie recenzji
        [Fact]
        public void Index_ReturnsAllGames_WithCalculatedAverageRating()
        {
            var db = GetInMemoryDbContext();

            // Dodajemy producenta i grę z recenzjami do bazy danych, aby przetestować, czy metoda Index poprawnie oblicza średni rating
            var producer = new Producer { Id = 1, Name = "Testowy Producent" };
            db.Producers.Add(producer);

            var game = new Game
            {
                Id = 1,
                Title = "Test Game",
                AverageRating = 0,
                Description = "Test",
                CoverImagePath = "Test",
                ProducerId = 1,
                Producer = producer,
                Reviews = new List<Review> { new Review { Rating = 10, Content = "A", UserId = "1" }, new Review { Rating = 8, Content = "B", UserId = "2" } },
                Platforms = new List<Platform>(),
                Genres = new List<Genre>()
            };
            db.Games.Add(game);
            db.SaveChanges();

            var controller = GetControllerWithContext(db);

            // Wywołujemy metodę Index i sprawdzamy, czy zwraca poprawne dane, w tym obliczony średni rating na podstawie recenzji
            var result = controller.Index(null) as ViewResult;
            var model = result?.Model as IEnumerable<Game>;

            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Single(model);
            Assert.Equal(9.0, model.First().AverageRating);
        }

        // Test sprawdzający, czy metoda Create zwraca błąd, gdy próbuje dodać grę o tytule, który już istnieje w bazie danych (niezależnie od wielkości liter)
        [Fact]
        public void Create_ReturnsError_WhenGameAlreadyExists()
        {
            var db = GetInMemoryDbContext();
            db.Games.Add(new Game
            {
                Id = 2,
                Title = "Cyberpunk 2077",
                Description = "Test",
                CoverImagePath = "Test",
                Platforms = new List<Platform>(),
                Genres = new List<Genre>(),
                Reviews = new List<Review>()
            });
            db.SaveChanges();

            var controller = GetControllerWithContext(db, role: "Moderator");
            
            // Próba dodania gry o tytule "cyberpunk 2077" (różnica w wielkości liter) powinna zwrócić błąd, ponieważ tytuł już istnieje w bazie danych
            var result = controller.Create("cyberpunk 2077", DateTime.Now, "img", "desc", 1, null, null, null, null, null) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.True(controller.TempData.ContainsKey("ErrorMsg"));
        }

        // Test sprawdzający, czy metoda Create poprawnie dodaje nową grę do bazy danych, gdy dane są poprawne, oraz czy ustawia odpowiednią wiadomość sukcesu w TempData
        [Fact]
        public void Create_AddsNewGame_WhenDataIsValid()
        {
            var db = GetInMemoryDbContext();
            var controller = GetControllerWithContext(db, role: "Moderator");

            // Wywołujemy metodę Create z poprawnymi danymi i sprawdzamy, czy gra została dodana do bazy danych oraz czy ustawiona została wiadomość sukcesu w TempData
            var result = controller.Create("Wiedźmin 3", DateTime.Now, "img.jpg", "Opis", 1, null, null, null, null, null) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal(1, db.Games.Count());
            Assert.True(controller.TempData.ContainsKey("SuccessMsg"));
        }

        // Test sprawdzający, czy metoda AddReview poprawnie dodaje recenzję do gry i aktualizuje średni rating gry na podstawie nowych recenzji
        [Fact]
        public void AddReview_UpdatesGameAverageRating()
        {
            var db = GetInMemoryDbContext();
            db.Games.Add(new Game
            {
                Id = 1,
                Title = "Game 1",
                Description = "Test",
                CoverImagePath = "Test",
                Platforms = new List<Platform>(),
                Genres = new List<Genre>(),
                Reviews = new List<Review>()
            });
            db.SaveChanges();

            var controller = GetControllerWithContext(db, userId: "user123");

            // Wywołujemy metodę AddReview, dodając recenzję z oceną 8, i sprawdzamy, czy gra została zaktualizowana z nową recenzją oraz czy średni rating gry został poprawnie obliczony na podstawie nowych recenzji
            var result = controller.AddReview(1, 8, "Świetna gra!") as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Details", result.ActionName);
            Assert.Equal(1, db.Reviews.Count());
            Assert.Equal(8, db.Games.First(g => g.Id == 1).AverageRating);
        }

        // Test sprawdzający, czy metoda DeleteGame poprawnie usuwa grę z bazy danych i ustawia odpowiednią wiadomość sukcesu w TempData
        [Fact]
        public void DeleteGame_RemovesGameFromDatabase()
        {
            var db = GetInMemoryDbContext();
            db.Games.Add(new Game
            {
                Id = 1,
                Title = "Gra do usunięcia",
                Description = "Test",
                CoverImagePath = "Test",
                Platforms = new List<Platform>(),
                Genres = new List<Genre>(),
                Reviews = new List<Review>()
            });
            db.SaveChanges();

            var controller = GetControllerWithContext(db, role: "Moderator");

            // Wywołujemy metodę DeleteGame, usuwając grę o Id 1, i sprawdzamy, czy gra została usunięta z bazy danych oraz czy ustawiona została wiadomość sukcesu w TempData
            var result = controller.DeleteGame(1) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Empty(db.Games);
            Assert.True(controller.TempData.ContainsKey("SuccessMsg"));
        }
    }
}