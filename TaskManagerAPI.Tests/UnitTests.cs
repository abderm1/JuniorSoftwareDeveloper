namespace TaskManagerAPI.Tests;

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Text.Json;

using Xunit;

public class UnitTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public UnitTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PostTask_ReturnsCreatedResponse()
    {
        // Arrange
        File.WriteAllText("tasks.json", ""); // Reset json file
        var client = _factory.CreateClient();
        var newTask = new { Description = "Test task" };
        client.DefaultRequestHeaders.Add("X-Tenant-ID", "company-a");

        // Act
        var response = await client.PostAsJsonAsync("/tasks", newTask);

        // Assert
        response.EnsureSuccessStatusCode();
        var task = await response.Content.ReadAsStringAsync();

        Assert.Contains("Test task", task);
    }

    [Fact]
    public async Task GetTask_ReturnsOkResponse_WithTasks()
    {
        // Arrange
        File.WriteAllText("tasks.json", ""); // Reset json file
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-ID", "company-a");
        var newTask = new { Description = "Test 2 task" };

        // Create a new task to ensure there are tasks to retrieve
        var response = await client.PostAsJsonAsync("/tasks", newTask);
        response.EnsureSuccessStatusCode();
        var task = await response.Content.ReadAsStringAsync();
        Assert.Contains("Test 2 task", task);


        // Act
        var getResponse = await client.GetAsync("/tasks");

        // Assert
        getResponse.EnsureSuccessStatusCode();
        var tasks = await response.Content.ReadAsStringAsync();
        Assert.Contains("Test 2 task", task);
    }

    [Fact]
    public async Task PutTask_ReturnsOkResponse_AndUpdatesTask()
    {
        // Arrange
        File.WriteAllText("tasks.json", ""); // Reset json file
        string? _taskId;
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-ID", "company-a");

        var newTask = new { Description = "Test 3 task" };
        // Create a new task to update later
        var response = await client.PostAsJsonAsync("/tasks", newTask);

        // Assert
        response.EnsureSuccessStatusCode();
        var task = await response.Content.ReadAsStringAsync();

        Assert.Contains("Test 3 task", task);
        var taskJson = JsonDocument.Parse(task).RootElement;
        _taskId = taskJson.GetProperty("id").GetString(); // Extract the taskId for the PUT request

        // Prepare the updated task data
        var updatedTask = new { Description = "Updated test task" };

        // Act - PUT the task using the taskId from the first test
        var putResponse = await client.PutAsJsonAsync($"/tasks/{_taskId}", updatedTask);

        // Assert
        putResponse.EnsureSuccessStatusCode(); 
        var updatedTaskResponse = await putResponse.Content.ReadAsStringAsync();
        Assert.Contains("Updated test task", updatedTaskResponse);  // Check if the task description was updated
        Assert.Contains(_taskId, updatedTaskResponse); // Check if the taskId is still the same
    }
}
