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
    public class GamesControllerTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var db = new AppDbContext(options);
            db.Database.EnsureCreated();
            return db;
        }

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

            return new GamesController(dbContext)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext },
                TempData = tempData
            };
        }

        [Fact]
        public void Index_ReturnsAllGames_WithCalculatedAverageRating()
        {
            var db = GetInMemoryDbContext();

            // NAPRAWA: Usunięto Description
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

            var result = controller.Index(null) as ViewResult;
            var model = result?.Model as IEnumerable<Game>;

            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Single(model);
            Assert.Equal(9.0, model.First().AverageRating);
        }

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

            var result = controller.Create("cyberpunk 2077", DateTime.Now, "img", "desc", 1, null, null) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.True(controller.TempData.ContainsKey("ErrorMsg"));
        }

        [Fact]
        public void Create_AddsNewGame_WhenDataIsValid()
        {
            var db = GetInMemoryDbContext();
            var controller = GetControllerWithContext(db, role: "Moderator");

            var result = controller.Create("Wiedźmin 3", DateTime.Now, "img.jpg", "Opis", 1, null, null) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal(1, db.Games.Count());
            Assert.True(controller.TempData.ContainsKey("SuccessMsg"));
        }

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

            var result = controller.AddReview(1, 8, "Świetna gra!") as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Details", result.ActionName);
            Assert.Equal(1, db.Reviews.Count());
            Assert.Equal(8, db.Games.First(g => g.Id == 1).AverageRating);
        }

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

            var result = controller.DeleteGame(1) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Empty(db.Games);
            Assert.True(controller.TempData.ContainsKey("SuccessMsg"));
        }
    }
}