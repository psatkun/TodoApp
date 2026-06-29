using System.ComponentModel.DataAnnotations;

namespace TodoApp.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public List<TodoItem> TodoItems { get; set; } = new();
    }
}
