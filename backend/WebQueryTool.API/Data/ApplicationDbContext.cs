using Microsoft.EntityFrameworkCore;
using WebQueryTool.API.Models;

namespace WebQueryTool.API.Data;

/// <summary>
/// Entity Framework database context for Web Query Tool
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<DatabaseConnection> DatabaseConnections { get; set; }
    public DbSet<QueryHistory> QueryHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.FullName).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).HasConversion<string>();
        });

        // DatabaseConnection configuration
        modelBuilder.Entity<DatabaseConnection>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasConversion<string>();
            entity.HasIndex(e => new { e.UserId, e.Name });

            // Foreign key relationship
            entity.HasOne<User>()
                  .WithMany(u => u.Connections)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // QueryHistory configuration
        modelBuilder.Entity<QueryHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ConnectionId);
            entity.HasIndex(e => e.ExecutedAt);
            entity.Property(e => e.SubmissionType).HasConversion<string>();

            // Foreign key relationships
            entity.HasOne(e => e.User)
                  .WithMany(u => u.QueryHistory)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Connection)
                  .WithMany()
                  .HasForeignKey(e => e.ConnectionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed default admin user (for development only)
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Create default admin user
        // Password: Admin@123
        // IMPORTANT: Change this in production!
        var adminId = Guid.NewGuid().ToString();
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = adminId,
            Email = "admin@example.com",
            FullName = "System Administrator",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
    }
}
