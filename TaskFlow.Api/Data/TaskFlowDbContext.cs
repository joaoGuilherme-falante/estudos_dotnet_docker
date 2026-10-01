using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Data;

public sealed class TaskFlowDbContext(DbContextOptions<TaskFlowDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<WorkItem> Tasks => Set<WorkItem>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(user => user.Email)
            .IsUnique();
        modelBuilder.Entity<User>()
            .Property(user => user.Email)
            .HasMaxLength(320);
        modelBuilder.Entity<User>()
            .Property(user => user.Name)
            .HasMaxLength(120);
        modelBuilder.Entity<Project>()
            .Property(project => project.Name)
            .HasMaxLength(160);
        modelBuilder.Entity<WorkItem>()
            .Property(task => task.Title)
            .HasMaxLength(200);
        modelBuilder.Entity<Comment>()
            .Property(comment => comment.Content)
            .HasMaxLength(4000);

        modelBuilder.Entity<WorkItem>()
            .HasOne(task => task.Project)
            .WithMany(project => project.Tasks)
            .HasForeignKey(task => task.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<WorkItem>()
            .HasOne(task => task.AssignedUser)
            .WithMany(user => user.AssignedTasks)
            .HasForeignKey(task => task.AssignedUserId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Comment>()
            .HasOne(comment => comment.Task)
            .WithMany(task => task.Comments)
            .HasForeignKey(comment => comment.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Comment>()
            .HasOne(comment => comment.User)
            .WithMany(user => user.Comments)
            .HasForeignKey(comment => comment.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
