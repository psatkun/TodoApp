namespace TodoApp.Models.DTOs
{
    /// <summary>
    /// Data Transfer Objet representing a sanitized, read-only view of a todo task
    /// returned to the client.
    /// </summary>
    public record TodoResponseDto
    {
        /// <summary>
        /// Unique primary key ID
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Main title headline of the task
        /// </summary>
        public required string Title { get; set; }
        /// <summary>
        /// Detailed description or instructions associated with the task
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// Boolean indicating whether task has been marked as finished
        /// </summary>
        public bool IsCompleted { get; set; }
        /// <summary>
        /// Deadline date assigned to the task
        /// </summary>
        public DateTime? DueDate { get; set; }
    }
}
