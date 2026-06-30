using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;
using TodoApp.Models.DTOs;
using System.Security.Claims;

namespace TodoApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TodosController : ControllerBase
    {
        private readonly TodoDbContext _dbContext;
        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        public TodosController(TodoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

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

        [HttpPost]
        public async Task<ActionResult<TodoResponseDto>> CreateTodo(CreateTodoDto incomingDto)
        {
            var newTodoItem = new TodoItem
            {
                Id = Guid.NewGuid(),
                Title = incomingDto.Title,
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodo(Guid id, UpdateTodoDto incomingDto)
        {
            var todoItem = await _dbContext.TodoItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

            if (todoItem == null)
            {
                return NotFound();
            }

            todoItem.Title = incomingDto.Title;
            todoItem.Description = incomingDto.Description;
            todoItem.DueDate = incomingDto.DueDate;
            todoItem.IsCompleted = incomingDto.IsCompleted;

            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
