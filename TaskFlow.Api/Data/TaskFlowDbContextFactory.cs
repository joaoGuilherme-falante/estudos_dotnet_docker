using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TaskFlow.Api.Data;

public sealed class TaskFlowDbContextFactory : IDesignTimeDbContextFactory<TaskFlowDbContext>
{
    public TaskFlowDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__TaskFlowDb")
            ?? "Host=localhost;Database=taskflow;Username=taskflow;Password=design-time-only";
        var options = new DbContextOptionsBuilder<TaskFlowDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        return new TaskFlowDbContext(options);
    }
}
