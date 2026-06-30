using System.ComponentModel.DataAnnotations;

namespace TodoApp.Models
{
    /// <summary>
    /// Core database entity schema for a user account stored in the Users table.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Primary key auto-incremented integer identifier for the user
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Unique identity handle chosen by the user during registration
        /// </summary>
        [Required]
        public string Username { get; set; } = string.Empty;
        /// <summary>
        /// Cryptographically salted BCrypt hash string generated from the user's password
        /// </summary>
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        /// <summary>
        /// Collection tracknig all task records linked to the user
        /// </summary>
        public List<TodoItem> TodoItems { get; set; } = new();
    }
}
