using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaskFlow.Api.Data;

namespace TaskFlow.Tests;

public sealed class TaskFlowApiFactory : WebApplicationFactory<Program>
{
    public const string AdminEmail = "admin@example.test";
    public const string AdminPassword = "Testing-Admin-Password-123!";
    private readonly Dictionary<string, string?> _originalEnvironment = new()
    {
        ["ConnectionStrings__TaskFlowDb"] = Environment.GetEnvironmentVariable("ConnectionStrings__TaskFlowDb"),
        ["Jwt__Key"] = Environment.GetEnvironmentVariable("Jwt__Key"),
        ["Jwt__Issuer"] = Environment.GetEnvironmentVariable("Jwt__Issuer"),
        ["Jwt__Audience"] = Environment.GetEnvironmentVariable("Jwt__Audience"),
        ["BootstrapAdmin__Name"] = Environment.GetEnvironmentVariable("BootstrapAdmin__Name"),
        ["BootstrapAdmin__Email"] = Environment.GetEnvironmentVariable("BootstrapAdmin__Email"),
        ["BootstrapAdmin__Password"] = Environment.GetEnvironmentVariable("BootstrapAdmin__Password")
    };

    public TaskFlowApiFactory()
    {
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__TaskFlowDb", "Host=localhost;Database=test;Username=test;Password=test");
        Environment.SetEnvironmentVariable("Jwt__Key", "test-secret-key-that-is-at-least-32-bytes-long");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "TaskFlow.Tests");
        Environment.SetEnvironmentVariable("Jwt__Audience", "TaskFlow.Tests");
        Environment.SetEnvironmentVariable("BootstrapAdmin__Name", "Test Administrator");
        Environment.SetEnvironmentVariable("BootstrapAdmin__Email", AdminEmail);
        Environment.SetEnvironmentVariable("BootstrapAdmin__Password", AdminPassword);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<TaskFlowDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<TaskFlowDbContext>>();
            services.AddDbContext<TaskFlowDbContext>(options =>
                options.UseInMemoryDatabase("taskflow-integration-tests"));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
        {
            return;
        }

        foreach (var (key, value) in _originalEnvironment)
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}
