using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

var app = builder.Build();

app.MapGet("/", () => "Task Manager API - Test Project");

// POST /tasks: creates a new task
app.MapPost("/tasks", async (HttpContext context, CreateTaskRequest taskRequest) =>
{
    var tenantId = context.Request.Headers["X-Tenant-ID"].ToString();

    if (string.IsNullOrWhiteSpace(tenantId))
        return Results.BadRequest("Missing X-Tenant-ID header");

    if (string.IsNullOrWhiteSpace(taskRequest.Description))
        return Results.BadRequest("Description is required");

    var newTask = new TaskItem(
        Guid.NewGuid().ToString(),
        taskRequest.Description,
        DateTime.UtcNow,
        tenantId
    );

    var filePath = "tasks.json";
    List<TaskItem> tasks = new();

    if (File.Exists(filePath))
    {
        var json = await File.ReadAllTextAsync(filePath);
        if (!string.IsNullOrWhiteSpace(json))
        {
            tasks = System.Text.Json.JsonSerializer.Deserialize<List<TaskItem>>(json) ?? new();
        }
    }

    tasks.Add(newTask);

    var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
    var updatedJson = System.Text.Json.JsonSerializer.Serialize(tasks, options);


    await File.WriteAllTextAsync(filePath, updatedJson);

    return Results.Created($"/tasks/{newTask.Id}", newTask);
});

app.MapGet("/tasks", async (HttpContext context) =>
{
    var tenantId = context.Request.Headers["X-Tenant-ID"].ToString();

    if (string.IsNullOrWhiteSpace(tenantId))
        return Results.BadRequest("Missing X-Tenant-ID header");

    var filePath = "tasks.json";
    if (!File.Exists(filePath))
        return Results.Ok(new List<TaskItem>());

    var json = await File.ReadAllTextAsync(filePath);
    var allTasks = System.Text.Json.JsonSerializer.Deserialize<List<TaskItem>>(json) ?? new();

    var tenantTasks = allTasks.Where(t => t.TenantId == tenantId).ToList();

    return Results.Ok(tenantTasks);
});

app.MapPut("/tasks/{id}", async (HttpContext context, string id, UpdateTaskRequest update) =>
{
    var tenantId = context.Request.Headers["X-Tenant-ID"].ToString();

    if (string.IsNullOrWhiteSpace(tenantId))
        return Results.BadRequest("Missing X-Tenant-ID header");

    if (string.IsNullOrWhiteSpace(update.Description))
        return Results.BadRequest("Description is required");

    var filePath = "tasks.json";
    if (!File.Exists(filePath))
        return Results.NotFound("Task storage not found");

    var json = await File.ReadAllTextAsync(filePath);
    var tasks = System.Text.Json.JsonSerializer.Deserialize<List<TaskItem>>(json) ?? new();

    var task = tasks.FirstOrDefault(t => t.Id == id && t.TenantId == tenantId);
    if (task == null)
        return Results.NotFound("Task not found for this tenant");

    var updatedTask = task with { Description = update.Description };

    var updatedList = tasks.Select(t => t.Id == id && t.TenantId == tenantId ? updatedTask : t).ToList();

    var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
    var updatedJson = System.Text.Json.JsonSerializer.Serialize(updatedList, options);
    await File.WriteAllTextAsync(filePath, updatedJson);

    return Results.Ok(updatedTask);
});



app.Run();

// Define the data models
record TaskItem(string Id, string Description, DateTime CreationDate, string TenantId);
record CreateTaskRequest(string Description);
record UpdateTaskRequest(string Description);

