using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace TaskFlow.Tests;

public sealed class ApiAuthorizationTests(TaskFlowApiFactory factory)
    : IClassFixture<TaskFlowApiFactory>
{
    [Fact]
    public async Task AdminCanCreateUsersButDeveloperCannotCreateProjects()
    {
        using var client = factory.CreateClient();

        var anonymousResponse = await client.GetAsync("/api/projects");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);

        var adminToken = await LoginAsync(client, TaskFlowApiFactory.AdminEmail, TaskFlowApiFactory.AdminPassword);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var createUserResponse = await client.PostAsJsonAsync("/api/users", new
        {
            name = "Developer User",
            email = "developer@example.test",
            password = "Developer-Password-123!",
            role = "Developer"
        });
        Assert.Equal(HttpStatusCode.Created, createUserResponse.StatusCode);

        var developerToken = await LoginAsync(client, "developer@example.test", "Developer-Password-123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", developerToken);
        var forbiddenResponse = await client.PostAsJsonAsync("/api/projects", new
        {
            name = "Forbidden project",
            description = "A Developer must not create projects."
        });
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var createdProject = await client.PostAsJsonAsync("/api/projects", new
        {
            name = "Allowed project",
            description = "Created by an Admin."
        });
        Assert.Equal(HttpStatusCode.Created, createdProject.StatusCode);
    }

    private static async Task<string> LoginAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("accessToken").GetString()
            ?? throw new InvalidOperationException("Login response did not include an access token.");
    }
}
