namespace TodoApp.Models.DTOs
{
    public record UpdateTodoDto : CreateTodoDto
    {
        public bool IsCompleted { get; set; }
    }
}
