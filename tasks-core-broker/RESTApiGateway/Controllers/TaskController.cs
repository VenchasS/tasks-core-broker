using Microsoft.AspNetCore.Mvc;
using RESTApiGateway.DTO;
using Microsoft.AspNetCore.Authorization;
using Prometheus;

[Authorize]
[ApiController]
[Route("tasks")]
public class TasksController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private static readonly Histogram ResponseTimeHistogram = Metrics
    .CreateHistogram("response_time", "Histogram of response time of each method",
        new HistogramConfiguration
        {
            // We divide measurements in 10 buckets of $100 each, up to $1000.
            Buckets = Histogram.LinearBuckets(start: 0, width: 1, count: 4)
        });

    private static readonly Counter TasksRecievedRequestsCount = Metrics
        .CreateCounter("taskController_jobs_requests_total", "Number of recieved requests");

    public TasksController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTasks()
    {
        TasksRecievedRequestsCount.Inc();
        var client = _httpClientFactory.CreateClient("TaskQueueClient");
        var response = await client.GetAsync("/queue/tasks");
        var content = await response.Content.ReadAsStringAsync();
        return Content(content, "application/json");
    }

    [HttpPost]
    public async Task<IActionResult> PostTask([FromBody] TaskDTO task)
    {
        TasksRecievedRequestsCount.Inc();
        var client = _httpClientFactory.CreateClient("TaskQueueClient");
        var response = await client.PostAsJsonAsync("/queue", task);
        var content = await response.Content.ReadAsStringAsync();
        return Content(content, "application/json");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTaskById(string id)
    {
        TasksRecievedRequestsCount.Inc();
        var client = _httpClientFactory.CreateClient("TaskQueueClient");
        var response = await client.GetAsync($"/queue/tasks/{id}");
        var content = await response.Content.ReadAsStringAsync();
        return Content(content, "application/json");
    }

    [HttpPost("/restart/{id}")]
    public async Task<IActionResult> RestartTask(string id)
    {
        TasksRecievedRequestsCount.Inc();
        var client = _httpClientFactory.CreateClient("TaskQueueClient");
        var response = await client.PostAsync($"/queue/restart/{id}", null);
        var content = await response.Content.ReadAsStringAsync();
        return Content(content, "application/json");
    }
}