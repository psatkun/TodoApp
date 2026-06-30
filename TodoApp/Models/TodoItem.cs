namespace TodoApp.Models
{
    /// <summary>
    /// Core database entity scheme for a task stored in the TodoItems table.
    /// </summary>
    public class TodoItem
    {
        /// <summary>
        /// Primary key unique identifier for the task
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Main title of the task
        /// </summary>
        public required string Title { get; set; }
        /// <summary>
        /// Detailed description or instructions for the task (optional)
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// Boolean value indicating whether the task has been marked as finished
        /// </summary>
        public bool IsCompleted { get; set; }
        /// <summary>
        /// Due date by which the task must be finished
        /// </summary>
        public DateTime DueDate { get; set; }
        /// <summary>
        /// System timestamp for when task was initialized
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// Timestamp logging when task completed (optional)
        /// </summary>
        public DateTime? CompletedAt { get; set; }
        /// <summary>
        /// Foreign key integer identifier referencing owner account
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// Gets or sets the virtual navigation properties linking this task to its parent <see cref="Models.User"/> database entity model.
        /// </summary>
        public User? User { get; set; }
    }
}
