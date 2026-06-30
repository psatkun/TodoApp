namespace TodoApp.Models.DTOs
{
    /// <summary>
    /// Data Transfer Object sent back to the client upon a successful authentication.
    /// Wraps the generated token signature string.
    /// </summary>
    public record AuthResponseDto
    {
        /// <summary>
        /// Cryptographically signed JSON Web Token (JWT) authorizing subsequent client requests.
        /// </summary>
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
