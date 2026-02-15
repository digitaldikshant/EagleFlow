using EagleFlow.Models;
using Microsoft.EntityFrameworkCore;

namespace EagleFlow.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<PasswordResetOtp> PasswordResetOtps => Set<PasswordResetOtp>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>()
            .HasIndex(d => d.DocumentNumber)
            .IsUnique();

        modelBuilder.Entity<AdminUser>()
            .HasIndex(a => a.Email)
            .IsUnique();

        modelBuilder.Entity<PasswordResetOtp>()
            .HasIndex(o => new { o.Email, o.OtpCode, o.IsUsed });

        base.OnModelCreating(modelBuilder);
    }
}
