namespace TodoApp.Models
{
    public class TodoItem
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Foreign key --> user
        /// </summary>
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
