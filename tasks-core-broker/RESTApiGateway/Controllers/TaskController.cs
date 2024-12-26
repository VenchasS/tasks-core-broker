using Microsoft.AspNetCore.Mvc;
using RESTApiGateway.DTO;
using Microsoft.AspNetCore.Authorization;

[Authorize]
[ApiController]
[Route("tasks")]
public class TasksController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public TasksController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTasks()
    {
        var client = _httpClientFactory.CreateClient("TaskQueueClient");
        var response = await client.GetAsync("/queue/tasks");
        var content = await response.Content.ReadAsStringAsync();
        return Content(content, "application/json");
    }

    [HttpPost]
    public async Task<IActionResult> PostTask([FromBody] TaskDTO task)
    {
        var client = _httpClientFactory.CreateClient("TaskQueueClient");
        var response = await client.PostAsJsonAsync("/queue", task);
        var content = await response.Content.ReadAsStringAsync();
        return Content(content, "application/json");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTaskById(string id)
    {
        var client = _httpClientFactory.CreateClient("TaskQueueClient");
        var response = await client.GetAsync($"/queue/tasks/{id}");
        var content = await response.Content.ReadAsStringAsync();
        return Content(content, "application/json");
    }

    [HttpPost("/restart/{id}")]
    public async Task<IActionResult> RestartTask(string id)
    {
        var client = _httpClientFactory.CreateClient("TaskQueueClient");
        var response = await client.PostAsync($"/queue/restart/{id}", null);
        var content = await response.Content.ReadAsStringAsync();
        return Content(content, "application/json");
    }
}