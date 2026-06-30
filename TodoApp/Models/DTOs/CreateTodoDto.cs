namespace TodoApp.Models.DTOs
{
    public record CreateTodoDto
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
