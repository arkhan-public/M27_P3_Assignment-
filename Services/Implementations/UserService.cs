using Microsoft.EntityFrameworkCore;
using QAWebApp.Data;
using QAWebApp.DTOs;
using QAWebApp.Models;
using QAWebApp.Services.Interfaces;
using BCrypt.Net;

namespace QAWebApp.Services.Implementations;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(ApplicationDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, ApplicationUser? User)> RegisterAsync(RegisterDto dto)
    {
        try
        {
            // Check if username already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == dto.Username || u.Email == dto.Email);

            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed: Username or email already exists");
                return (false, "Username or email already exists", null);
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new ApplicationUser
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {Username} registered successfully", user.Username);
            return (true, "Registration successful", user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            return (false, "An error occurred during registration", null);
        }
    }

    public async Task<(bool Success, string Message, ApplicationUser? User)> LoginAsync(LoginDto dto)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == dto.UsernameOrEmail || u.Email == dto.UsernameOrEmail);

            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found");
                return (false, "Invalid credentials", null);
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: Invalid password for user {Username}", user.Username);
                return (false, "Invalid credentials", null);
            }

            _logger.LogInformation("User {Username} logged in successfully", user.Username);
            return (true, "Login successful", user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user login");
            return (false, "An error occurred during login", null);
        }
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(int userId)
    {
        try
        {
            return await _context.Users.FindAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by ID {UserId}", userId);
            return null;
        }
    }

    public async Task<ApplicationUser?> GetUserByUsernameAsync(string username)
    {
        try
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by username {Username}", username);
            return null;
        }
    }
}
