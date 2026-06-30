namespace TodoApp.Models.DTOs
{
    /// <summary>
    /// Data Transfer Object representing payload sent by the client to modify
    /// an existing task item. Inherits base properties from <see cref="CreateTodoDto"/>
    /// </summary>
    public record UpdateTodoDto : CreateTodoDto
    {
        /// <summary>
        /// Boolean indicating whether the task has been marked as finished
        /// </summary>
        public bool IsCompleted { get; set; }
    }
}
