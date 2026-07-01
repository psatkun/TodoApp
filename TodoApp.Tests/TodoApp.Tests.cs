using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using TodoApp.Controllers;
using TodoApp.Data;
using TodoApp.Models;
using TodoApp.Models.DTOs;
using Xunit;

namespace TodoApp.Tests
{
    public class ControllerUnitTests
    {
        // Helper method to generate a clean, isolated in-memory database instance for each test run
        private TodoDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<TodoDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new TodoDbContext(options);
        }

        // Helper method to mock out IConfiguration for JWT strings
        private IConfiguration MockConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string> {
                {"JwtSettings:SecretKey", "SuperSecretKeyThatIsAtLeast32BytesLong!"},
                {"JwtSettings:Issuer", "TodoApp"},
                {"JwtSettings:Audience", "TodoAppUsers"}
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();
        }

        // ==========================================
        // AUTH CONTROLLER TESTS
        // ==========================================

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenUsernameAlreadyExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
            var config = MockConfiguration();

            // Seed the test database with an existing user
            context.Users.Add(new User { Id = 1, Username = "existingUser", PasswordHash = "anyHash" });
            await context.SaveChangesAsync();

            var controller = new AuthController(context, config);
            var duplicateRequest = new UserDto { Username = "existingUser", Password = "NewPassword123!" };

            // Act
            var result = await controller.Register(duplicateRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Username already exists.", badRequestResult.Value);
        }

        // ==========================================
        // TODOS CONTROLLER TESTS (Model Validation Guards)
        // ==========================================

        [Fact]
        public async Task CreateTodo_ReturnsBadRequest_WhenTitleIsBlankOrWhitespace()
        {
            // Arrange
            using var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
            var controller = new TodosController(context);

            var invalidRequest = new CreateTodoDto
            {
                Title = "   ", // Pure whitespace
                Description = "Valid Description",
                DueDate = DateTime.UtcNow.AddDays(2)
            };

            // Act
            var result = await controller.CreateTodo(invalidRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Task title cannot be blank.", badRequestResult.Value);
        }

        [Fact]
        public async Task CreateTodo_SavesSuccessfully_WhenTitleIsValid()
        {
            // 1. Arrange
            using var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
            var controller = new TodosController(context);

            // 🌟 THE FIX: Fake the authenticated user identity (User ID: 111)
            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "111")
    }));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaims }
            };

            var validRequest = new CreateTodoDto
            {
                Title = "Finish Code Comments",
                Description = "Clean up code text",
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            // 2. Act
            var result = await controller.CreateTodo(validRequest);

            // 3. Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var responseDto = Assert.IsType<TodoResponseDto>(createdAtActionResult.Value);
            Assert.Equal("Finish Code Comments", responseDto.Title);
        }

        // Test the custom FutureDate attribute directly

        [Fact]
        public void FutureDateAttribute_ReturnsFailure_WhenDateIsInThePast()
        {
            // Arrange
            var attribute = new FutureDateAttribute();
            var pastDate = DateTime.UtcNow.AddDays(-5);

            // Act
            var result = attribute.GetValidationResult(pastDate, new ValidationContext(new object()));

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(ValidationResult.Success, result);
        }

        [Fact]
        public void FutureDateAttribute_ReturnsSuccess_WhenDateIsTodayOrInTheFuture()
        {
            // Arrange
            var attribute = new FutureDateAttribute();
            var futureDate = DateTime.UtcNow.AddDays(3);

            // Act
            var result = attribute.GetValidationResult(futureDate, new ValidationContext(new object()));

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        // ==========================================
        // SECURITY & OWNERSHIP ENFORCEMENT TESTS
        // ==========================================

        [Fact]
        public async Task GetTodoById_ReturnsNotFound_WhenUserAttemptsToAccessAnotherUsersTodo()
        {
            // Arrange
            using var context = GetInMemoryDbContext(Guid.NewGuid().ToString());

            var userATaskId = Guid.NewGuid();
            var userBTaskId = Guid.NewGuid();

            context.TodoItems.AddRange(
                new TodoItem { Id = userATaskId, Title = "User A Task", UserId = 111, DueDate = DateTime.UtcNow.AddDays(1) },
                new TodoItem { Id = userBTaskId, Title = "User B Secret Task", UserId = 222, DueDate = DateTime.UtcNow.AddDays(1) }
            );
            await context.SaveChangesAsync();

            var controller = new TodosController(context);

            // 🌟 THE FIX: Instead of modifying the private property, mock the HttpContext User identity!
            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "111") // Pretending to be User A
    }));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaims }
            };

            // Act
            var result = await controller.GetTodoById(userBTaskId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task UpdateTodo_ReturnsNotFound_WhenUserAttemptsToModifyAnotherUsersTodo()
        {
            // Arrange
            using var context = GetInMemoryDbContext(Guid.NewGuid().ToString());

            var userBTaskId = Guid.NewGuid();
            context.TodoItems.Add(
                new TodoItem { Id = userBTaskId, Title = "User B Original Task", UserId = 222, DueDate = DateTime.UtcNow.AddDays(1) }
            );
            await context.SaveChangesAsync();

            var controller = new TodosController(context);

            // 🌟 THE FIX: Fake User A trying to update User B's task
            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
        new Claim(ClaimTypes.NameIdentifier, "111")
    }));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaims }
            };

            var updatePayload = new UpdateTodoDto
            {
                Title = "Malicious Edit Attempt",
                IsCompleted = true,
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            // Act
            var result = await controller.UpdateTodo(userBTaskId, updatePayload);

            // Assert
            Assert.IsType<NotFoundResult>(result);

            // Double Check: Verify the database record was completely untouched
            var unmodifiedTask = await context.TodoItems.FindAsync(userBTaskId);
            Assert.NotNull(unmodifiedTask);
            Assert.Equal("User B Original Task", unmodifiedTask.Title);
            Assert.False(unmodifiedTask.IsCompleted);
        }
    }
}