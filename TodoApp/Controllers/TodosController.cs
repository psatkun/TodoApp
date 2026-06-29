using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Contracts;
using TodoApp.Data;
using TodoApp.Models;
using TodoApp.Models.DTOs;

namespace TodoApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodosController : ControllerBase
    {
        private readonly TodoDbContext _dbContext;
        public TodosController(TodoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetAllTodos()
        {
            var rawTodos = await _dbContext.TodoItems.ToListAsync();

            var responseData = rawTodos.Select(todo => new TodoResponseDto
            {
                Id = todo.Id,
                Title = todo.Title,
                Description = todo.Description,
                IsCompleted = todo.IsCompleted
            });

            return Ok(rawTodos);
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
                Username = incomingDto.Username
            };

            _dbContext.TodoItems.Add(newTodoItem);

            await _dbContext.SaveChangesAsync();

            var responseData = new TodoResponseDto
            {
                Id = newTodoItem.Id,
                Title = newTodoItem.Title,
                Description = newTodoItem.Description,
                IsCompleted = false
            };

            return CreatedAtAction(nameof(GetTodoById), new { id = responseData.Id }, responseData);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TodoResponseDto>> GetTodoById(Guid id)
        {
            var todoItem = await _dbContext.TodoItems.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            var responseData = new TodoResponseDto
            {
                Id = todoItem.Id,
                Title = todoItem.Title,
                Description = todoItem.Description,
                IsCompleted = todoItem.IsCompleted
            };

            return Ok(responseData);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(Guid id)
        {
            var todoItem = await _dbContext.TodoItems.FindAsync(id);

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
            var todoItem = await _dbContext.TodoItems.FindAsync();

            if (todoItem == null)
            {
                return NotFound();
            }

            todoItem.Title = incomingDto.Title;
            todoItem.Description = incomingDto.Description;
            todoItem.IsCompleted = incomingDto.IsCompleted;

            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
