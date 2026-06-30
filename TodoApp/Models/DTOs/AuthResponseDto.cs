namespace TodoApp.Models.DTOs
{
    public record AuthResponseDto
    {
        public string Token { get; init; } = string.Empty;

        public AuthResponseDto(string token)
        {
            Token = token;
        }

        public AuthResponseDto() 
        { 
        }
    }
}
