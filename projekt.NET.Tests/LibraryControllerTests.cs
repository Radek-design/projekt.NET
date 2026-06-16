using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using projekt.NET.Controllers;
using projekt.NET.Data;
using projekt.NET.Models;
using projekt.NET.Models.Entities;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace projekt.NET.Tests.Controllers
{
    public class LibraryControllerTests
    {
        private DbContextOptions<AppDbContext> GetInMemoryDbOptions()
        {
            return new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unikalna nazwa dla każdego testu
                .Options;
        }

        private Mock<UserManager<ApplicationUser>> MockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task GenerateReport_ReturnsPdfFileResult_WithCorrectFileName()
        {
            // Arrange
            var dbOptions = GetInMemoryDbOptions();
            var mockUserManager = MockUserManager();
            var mockEnv = new Mock<IWebHostEnvironment>();

            var testUserId = "user-123";
            var testUserName = "TestowyGracz";

            // Symulujemy użytkownika zalogowanego w systemie (Claims)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, testUserId)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // Mockujemy odpowiedź UserManager.FindByIdAsync
            var applicationUser = new ApplicationUser { Id = testUserId, UserName = testUserName };
            mockUserManager.Setup(um => um.FindByIdAsync(testUserId))
                           .ReturnsAsync(applicationUser);

            using (var context = new AppDbContext(dbOptions))
            {
                // Dodajemy testowe dane do bazy in-memory, aby PDF miał co wygenerować
                var game = new Game { Id = 1, Title = "Gra Testowa do PDF" };
                context.Games.Add(game);

                context.UserGames.Add(new UserGame { UserId = testUserId, GameId = 1, Status = "Ukończono", PlayTimeHours = 50 });
                context.Reviews.Add(new Review { UserId = testUserId, GameId = 1, Content = "Świetna gra", Rating = 10 });
                await context.SaveChangesAsync();

                var controller = new LibraryController(context, mockUserManager.Object, mockEnv.Object)
                {
                    ControllerContext = new ControllerContext
                    {
                        HttpContext = new DefaultHttpContext { User = claimsPrincipal }
                    }
                };

                // Act
                var result = await controller.GenerateReport();

                // Assert
                var fileResult = Assert.IsType<FileContentResult>(result);

                // Sprawdzamy czy to na pewno PDF
                Assert.Equal("application/pdf", fileResult.ContentType);

                // Sprawdzamy czy nazwa pliku zawiera nazwę użytkownika, zgodnie z logiką: $"Raport_{user?.UserName ?? "Gier"}.pdf"
                Assert.Equal($"Raport_{testUserName}.pdf", fileResult.FileDownloadName);

                // Sprawdzamy czy wygenerowano jakieś bajty (plik nie jest pusty)
                Assert.NotEmpty(fileResult.FileContents);
            }
        }
    }
}