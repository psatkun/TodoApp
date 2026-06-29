namespace TodoApp.Models.DTOs
{
    public class UpdateTodoDto : CreateTodoDto
    {
        public bool IsCompleted { get; set; }
    }
}
