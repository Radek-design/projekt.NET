using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using projekt.NET.Controllers;
using projekt.NET.Data;
using projekt.NET.Models;
using projekt.NET.Models.Entities;
using Xunit;

namespace projekt.NET.Tests
{
    public class ForumControllerTests
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

        private Mock<UserManager<ApplicationUser>> GetMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var options = new Mock<IOptions<IdentityOptions>>();
            var hasher = new Mock<IPasswordHasher<ApplicationUser>>();
            var userValidators = new List<IUserValidator<ApplicationUser>>();
            var passValidators = new List<IPasswordValidator<ApplicationUser>>();
            var normalizer = new Mock<ILookupNormalizer>();
            var errors = new Mock<IdentityErrorDescriber>();
            var services = new Mock<IServiceProvider>();
            var logger = new Mock<ILogger<UserManager<ApplicationUser>>>();

            return new Mock<UserManager<ApplicationUser>>(
                store.Object, options.Object, hasher.Object, userValidators, passValidators, normalizer.Object, errors.Object, services.Object, logger.Object);
        }

        private ForumController GetController(AppDbContext dbContext, Mock<UserManager<ApplicationUser>> userManagerMock, string role = "User", string userId = "user1")
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            }, "mock"));

            var httpContext = new DefaultHttpContext { User = user };
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            return new ForumController(dbContext, userManagerMock.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext },
                TempData = tempData
            };
        }

        [Fact]
        public async Task Index_ReturnsOnlyApprovedOrUserOwnedPosts()
        {
            var db = GetInMemoryDbContext();

            // NAPRAWA: Tworzymy fikcyjnych użytkowników, aby EF Core przepuścił posty przez INNER JOIN
            var user1 = new ApplicationUser { Id = "otherUser", UserName = "Inny User" };
            var user2 = new ApplicationUser { Id = "myUser", UserName = "Ja" };
            db.Users.AddRange(user1, user2);

            // Dodajemy posty przypisane do tych użytkowników
            db.ForumPosts.AddRange(
                new ForumPost { Id = 1, Title = "Zatw", Content = "C", Topic = "T", UserId = "otherUser", User = user1, IsApproved = true },
                new ForumPost { Id = 2, Title = "Cudzy", Content = "C", Topic = "T", UserId = "otherUser", User = user1, IsApproved = false },
                new ForumPost { Id = 3, Title = "Mój", Content = "C", Topic = "T", UserId = "myUser", User = user2, IsApproved = false }
            );
            await db.SaveChangesAsync();

            var userManagerMock = GetMockUserManager();
            userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("myUser");

            var controller = GetController(db, userManagerMock, role: "User", userId: "myUser");

            var result = await controller.Index(null) as ViewResult;
            var model = result?.Model as IEnumerable<ForumPost>;

            Assert.NotNull(model);
            Assert.Equal(2, model.Count()); // Teraz model zwróci równe 2 wpisy!
            Assert.DoesNotContain(model, p => p.Title == "Cudzy");
        }

        [Fact]
        public async Task CreatePost_AddsUnapprovedPostToDatabase()
        {
            var db = GetInMemoryDbContext();
            var userManagerMock = GetMockUserManager();
            userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                           .ReturnsAsync(new ApplicationUser { Id = "user1", UserName = "TestUser" });

            var controller = GetController(db, userManagerMock);

            var result = await controller.CreatePost("Testowy tytuł", "Treść posta", "Gry") as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal(1, db.ForumPosts.Count());
            Assert.False(db.ForumPosts.First().IsApproved);
        }

        [Fact]
        public async Task ApprovePost_AsModerator_SetsIsApprovedToTrue()
        {
            var db = GetInMemoryDbContext();
            var post = new ForumPost { Id = 1, Title = "Do akceptacji", Content = "C", Topic = "T", UserId = "u", IsApproved = false };
            db.ForumPosts.Add(post);
            await db.SaveChangesAsync(); // Błąd nr 89 wyeliminowany

            var userManagerMock = GetMockUserManager();
            var controller = GetController(db, userManagerMock, role: "Moderator");

            var result = await controller.ApprovePost(1) as RedirectToActionResult;

            Assert.NotNull(result);
            var approvedPost = await db.ForumPosts.FindAsync(1);
            Assert.True(approvedPost.IsApproved);
        }

        [Fact]
        public async Task AddComment_SavesCommentToDatabase()
        {
            var db = GetInMemoryDbContext();
            db.ForumPosts.Add(new ForumPost { Id = 1, Title = "Test Post", Content = "C", Topic = "T", UserId = "u", IsApproved = true, Comments = new List<ForumComment>() });
            await db.SaveChangesAsync();

            var userManagerMock = GetMockUserManager();
            userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                           .ReturnsAsync(new ApplicationUser { Id = "user1" });

            var controller = GetController(db, userManagerMock);

            var result = await controller.AddComment(1, "Świetny wpis!") as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Details", result.ActionName);
            Assert.Equal(1, db.ForumComments.Count());
        }
    }
}