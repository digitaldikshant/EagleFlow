using EagleFlow.Models;
using Microsoft.EntityFrameworkCore;

namespace EagleFlow.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>()
            .HasIndex(d => d.DocumentNumber)
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}
