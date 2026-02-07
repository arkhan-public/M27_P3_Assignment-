using QAWebApp.DTOs;
using QAWebApp.Models;
using QAWebApp.Repositories.Interfaces;
using QAWebApp.Services.Interfaces;
using BCrypt.Net;

namespace QAWebApp.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, ApplicationUser? User)> RegisterAsync(RegisterDto dto)
    {
        try
        {
            // Check if username already exists
            var usernameExists = await _userRepository.UsernameExistsAsync(dto.Username);
            if (usernameExists)
            {
                _logger.LogWarning("Registration failed: Username already exists");
                return (false, "Username already exists", null);
            }

            // Check if email already exists
            var emailExists = await _userRepository.EmailExistsAsync(dto.Email);
            if (emailExists)
            {
                _logger.LogWarning("Registration failed: Email already exists");
                return (false, "Email already exists", null);
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

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

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
            // Try to find user by username first
            var user = await _userRepository.GetByUsernameAsync(dto.UsernameOrEmail);
            
            // If not found by username, try by email
            if (user == null)
            {
                user = await _userRepository.GetByEmailAsync(dto.UsernameOrEmail);
            }

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
            return await _userRepository.GetByIdAsync(userId);
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
            return await _userRepository.GetByUsernameAsync(username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by username {Username}", username);
            return null;
        }
    }
}
