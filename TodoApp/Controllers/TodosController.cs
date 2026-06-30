using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;
using TodoApp.Models.DTOs;
using System.Security.Claims;

namespace TodoApp.Controllers
{
    /// <summary>
    /// Handles secure CRUD operations for managing user-specific todo tasks.
    /// Requires JWT authentication across all endpoints.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TodosController : ControllerBase
    {
        private readonly TodoDbContext _dbContext;
        /// <summary>
        /// Extracts and parses the unique User ID from the authenticated JWT claims passport.
        /// Returns 0 if the identifier claim cannot be verified. 
        /// </summary>
        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        public TodosController(TodoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves all task items belonging exclusively to the currently logged-in user.
        /// </summary>
        /// <returns>A list of structured todo response data objects.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoResponseDto>>> GetAllTodos()
        {
            return await _dbContext.TodoItems
                            .Where(t => t.UserId == CurrentUserId)
                            .Select(t => new TodoResponseDto
                            {
                                Id = t.Id,
                                Title = t.Title,
                                Description = t.Description,
                                DueDate = t.DueDate,
                                IsCompleted = t.IsCompleted
                            })
                            .ToListAsync();
        }

        /// <summary>
        /// Creates a brand-new todo task for the authenticated user context.
        /// </summary>
        /// <param name="incomingDto">The data required to initialize the new task.</param>
        /// <returns>The newly generated task data along with a 201 Created redirect location header.</returns>
        [HttpPost]
        [HttpPost]
        public async Task<ActionResult<TodoResponseDto>> CreateTodo(CreateTodoDto incomingDto)
        {
            // ensure it isn't just whitespace that slipped past binding
            if (string.IsNullOrWhiteSpace(incomingDto.Title))
            {
                return BadRequest("Task title cannot be blank.");
            }

            var newTodoItem = new TodoItem
            {
                Id = Guid.NewGuid(),
                Title = incomingDto.Title.Trim(),
                Description = incomingDto.Description,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                DueDate = incomingDto.DueDate,
                UserId = CurrentUserId
            };

            _dbContext.TodoItems.Add(newTodoItem);
            await _dbContext.SaveChangesAsync();

            var responseData = new TodoResponseDto
            {
                Id = newTodoItem.Id,
                Title = newTodoItem.Title,
                Description = newTodoItem.Description,
                DueDate = newTodoItem.DueDate,
                IsCompleted = newTodoItem.IsCompleted
            };

            return CreatedAtAction(nameof(GetTodoById), new { id = responseData.Id }, responseData);
        }

        /// <summary>
        /// Fetches a specific todo task by its unique ID, validating ownership parameters.
        /// </summary>
        /// <param name="id">The Guid identifier of the target task.</param>
        /// <returns>The matching task data or a 404 Not Found if missing or unauthorized.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoResponseDto>> GetTodoById(Guid id)
        {
            var todoItem = await _dbContext.TodoItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

            if (todoItem == null)
            {
                return NotFound();
            }

            var responseData = new TodoResponseDto
            {
                Id = todoItem.Id,
                Title = todoItem.Title,
                Description = todoItem.Description,
                DueDate = todoItem.DueDate,
                IsCompleted = todoItem.IsCompleted
            };

            return Ok(responseData);
        }

        /// <summary>
        /// Permanently removes a task item from the database file.
        /// </summary>
        /// <param name="id">The Guid identifier of the task to be deleted.</param>
        /// <returns>A 204 No Content response upon successful deletion, or 404 if not found.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(Guid id)
        {
            var todoItem = await _dbContext.TodoItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

            if (todoItem == null)
            {
                return NotFound();
            }

            _dbContext.TodoItems.Remove(todoItem);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Updates the core property text, status, and due date elements of an existing task item.
        /// </summary>
        /// <param name="id">The Guid identifier of the target task being modified.</param>
        /// <param name="incomingDto">The complete payload containing the fresh state criteria.</param>
        /// <returns>A 204 No Content response on successful commit, or 404 if unmatched.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodo(Guid id, UpdateTodoDto incomingDto)
        {
            var todoItem = await _dbContext.TodoItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

            if (todoItem == null)
            {
                return NotFound();
            }

            // ensure it isn't just whitespace that slipped past binding
            if (string.IsNullOrWhiteSpace(incomingDto.Title))
            {
                return BadRequest("Task title cannot be blank.");
            }

            todoItem.Title = incomingDto.Title.Trim();
            todoItem.Description = incomingDto.Description;
            todoItem.DueDate = incomingDto.DueDate;
            todoItem.IsCompleted = incomingDto.IsCompleted;

            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
