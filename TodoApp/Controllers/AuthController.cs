using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TodoApp.Models.DTOs;
using BC = BCrypt.Net.BCrypt;
using TodoApp.Data;
using Microsoft.EntityFrameworkCore;
using TodoApp.Models;

namespace TodoApp.Controllers
{
    /// <summary>
    /// Handles user authentication, including registration, login validation, 
    /// and secure JWT generation.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly TodoDbContext _dbContext;
        private readonly IConfiguration _configuration;
        public AuthController(TodoDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        /// <summary>
        /// Registers a new user account after validating username uniqueness, 
        /// storing a cryptographically salted password hash.
        /// </summary>
        /// <param name="request">The data containing the desired username and plaintext password.</param>
        /// <returns>A 200 OK message on success, or a 400 Bad Request if the username is taken.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDto request)
        {
            if (await _dbContext.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest("Username already exists.");
            }

            string hashedPassword = BC.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                PasswordHash = hashedPassword
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return Ok("Registration successful!");
        }

        /// <summary>
        /// Authenticates user credentials and issues a signed JWT access token.
        /// </summary>
        /// <param name="request">The credentials payload to verify.</param>
        /// <returns>An authentication token payload on success, or 401 Unauthorized on credential mismatch.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserDto request)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            // 🔐 Verify the plaintext password against the stored database hash
            if (user == null || !BC.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid username or password.");
            }

            var token = GenerateJwtToken(user);
            return Ok(new AuthResponseDto(token));
        }

        /// <summary>
        /// Constructs and digitally signs a JWT containing security identifiers valid for 7 days.
        /// </summary>
        /// <param name="user">The verified database user entity targeting current context.</param>
        /// <returns>A fully packed and encoded JWT token string.</returns>
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) // Dynamic User ID claim
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = creds,
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    /// <summary>
    /// Data Transfer Object representing authentication payloads.
    /// </summary>
    public class UserDto
    {
        public required string Username { get; set; } = string.Empty;
        public required string Password { get; set; } = string.Empty;
    }
}
