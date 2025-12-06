using Microsoft.EntityFrameworkCore;
using StackOverGrave.Api.Models;

namespace StackOverGrave.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ConversionResult> ConversionResults => Set<ConversionResult>();
    public DbSet<RepositoryProject> RepositoryProjects => Set<RepositoryProject>();
    public DbSet<RepositoryFile> RepositoryFiles => Set<RepositoryFile>();

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

        modelBuilder.Entity<RepositoryProject>()
            .HasKey(rp => rp.Id);

        modelBuilder.Entity<RepositoryFile>()
            .HasKey(rf => rf.Id);

        modelBuilder.Entity<RepositoryFile>()
            .HasOne(rf => rf.RepositoryProject)
            .WithMany(rp => rp.Files)
            .HasForeignKey(rf => rf.RepositoryProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
