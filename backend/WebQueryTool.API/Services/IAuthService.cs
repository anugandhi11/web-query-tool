using WebQueryTool.API.Models;
using WebQueryTool.API.Models.DTOs;

namespace WebQueryTool.API.Services;

/// <summary>
/// Service for user authentication and authorization
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticate user and generate JWT token
    /// </summary>
    Task<LoginResponse> LoginAsync(LoginRequest request);

    /// <summary>
    /// Register a new user
    /// </summary>
    Task<User> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Generate JWT token for a user
    /// </summary>
    string GenerateJwtToken(User user);

    /// <summary>
    /// Validate JWT token and get user ID
    /// </summary>
    string? ValidateJwtToken(string token);

    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<User?> GetUserByIdAsync(string userId);
}
