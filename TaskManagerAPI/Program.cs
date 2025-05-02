namespace TaskManagerAPI;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json.Serialization;

public class Program
{
    // This method is used to send the task to the external service (EXTRA)
    private static async Task addRecord(TaskItem task) {
        using var client = new HttpClient();
        client.BaseAddress = new Uri("https://services.paloalto.swiss:10443");
        var payload = new
        {
            userId = 23,
            passwordWS = "1234",
            cabinetId = "804dfcb0-cf00-49c7-bb23-ec68bc3a6097",
            indexFields = new[]
            {
                new { fieldName = "TASK_ID", fieldValue = task.Id },
                new { fieldName = "TASK_DESCRIPTION", fieldValue = task.Description },
                new { fieldName = "CREATION DATE", fieldValue = task.CreationDate.ToString("yyyy-MM-dd") }
            }
        };
        var response = await client.PostAsJsonAsync("/api2/Docuware/add-record", payload);
        response.EnsureSuccessStatusCode();
    }


    public static void Main(string[] args)
    {

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

            // Call the external service to add the record (EXTRA)
            await addRecord(newTask);

            return Results.Created($"/tasks/{newTask.Id}", newTask);
        });

        // GET /tasks: retrieves all tasks for the tenant
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

        // PUT /tasks/{id}: updates a task for the tenant
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
    }

    // Define the data models
    record TaskItem(string Id, string Description, DateTime CreationDate, string TenantId);
    record CreateTaskRequest(string Description);
    record UpdateTaskRequest(string Description);
}
