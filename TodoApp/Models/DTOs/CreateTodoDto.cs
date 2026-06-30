using System.ComponentModel.DataAnnotations;

namespace TodoApp.Models.DTOs
{
    /// <summary>
    /// Data Transfer Object carrying the required data payload from the client
    /// to initialize a new todo task item.
    /// </summary>
    public record CreateTodoDto
    {
        /// <summary>
        /// Main title headline of the task
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Task title cannot be blank.")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public required string Title { get; set; }
        /// <summary>
        /// Extended description containing supplementary details or instructions about the task (optional)
        /// </summary>
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }
        /// <summary>
        /// Deadline date by which the task must be completed
        /// </summary>
        [Required(ErrorMessage = "Please provide a valid due date.")]
        [FutureDate(ErrorMessage = "Due date cannot be in the past.")]
        public DateTime DueDate { get; set; }
    }

    /// <summary>
    /// Custom validation attribute to ensure a DateTime value is not set in the past.
    /// </summary>
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime dateTime)
            {
                // Clear out the time portion to compare pure calendar dates seamlessly
                if (dateTime.Date < DateTime.UtcNow.Date)
                {
                    return new ValidationResult(ErrorMessage ?? "The date cannot be in the past.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
