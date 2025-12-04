using Microsoft.EntityFrameworkCore;
using StackOverGrave.Api.Models;

namespace StackOverGrave.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ConversionResult> ConversionResults => Set<ConversionResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<ConversionResult>()
            .HasKey(c => c.Id);

        modelBuilder.Entity<ConversionResult>()
            .HasOne(c => c.Project)
            .WithMany()
            .HasForeignKey(c => c.ProjectId);
    }
}
