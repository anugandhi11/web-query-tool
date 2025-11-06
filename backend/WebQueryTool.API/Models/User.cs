using System.ComponentModel.DataAnnotations;

namespace WebQueryTool.API.Models;

/// <summary>
/// Application user
/// </summary>
public class User
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [MaxLength(200)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// BCrypt hashed password
    /// </summary>
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; } = UserRole.User;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// User's database connections
    /// </summary>
    public virtual ICollection<DatabaseConnection> Connections { get; set; } = new List<DatabaseConnection>();

    /// <summary>
    /// User's query history
    /// </summary>
    public virtual ICollection<QueryHistory> QueryHistory { get; set; } = new List<QueryHistory>();
}

public enum UserRole
{
    Admin,
    User,
    ReadOnly
}
