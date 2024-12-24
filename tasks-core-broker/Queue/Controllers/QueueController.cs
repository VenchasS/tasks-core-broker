
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TaskQueue.Dto;
using TaskQueue.Services;

namespace TaskQueue.Controllers
{
    [ApiController]
    [Route("/queue")]
    public class QueueController : ControllerBase
    {
        private readonly QueueService _taskQueueService;
        private readonly int _defaultTtl;

        public QueueController(QueueService taskQueueService, IConfiguration configuration)
        {
            _taskQueueService = taskQueueService;
            _defaultTtl = configuration.GetValue<int>("TaskSettings:DefaultTTL");
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Retrieve welcome message", Description = "Provides a basic welcome message from the TaskQueue API.")]
        public IActionResult GetWelcomeMessage() => Ok("Welcome to TaskQueue API!");

        [HttpPost]
        [SwaggerOperation(Summary = "Add task to queue", Description = "Validates the input and adds a new task to the database and message broker.")]
        public async Task<IActionResult> AddTask([FromBody] TaskDto task)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            task.Ttl = task.Ttl <= 0 ? _defaultTtl : task.Ttl;

            try
            {
                var taskId = await _taskQueueService.AddTask(task);
                return Ok(taskId);
            }
            catch (Exception e)
            {
                LogException(e);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("restart/{id:int}")]
        [SwaggerOperation(Summary = "Restart task", Description = "Restarts a specified task by its ID.")]
        public async Task<IActionResult> RestartTask(int id)
        {
            try
            {
                await _taskQueueService.RestartTask(id);
                return Ok($"Task {id} has been restarted.");
            }
            catch (Exception e)
            {
                LogException(e);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("result")]
        [SwaggerOperation(Summary = "Get task result", Description = "Fetches the result of a specified task by its ID.")]
        public async Task<IActionResult> GetTaskResult([FromBody] int id)
        {
            try
            {
                var result = await _taskQueueService.GetTaskResultById(id);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Task {id} not found.");
            }
            catch (Exception e)
            {
                LogException(e);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("tasks")]
        [SwaggerOperation(Summary = "List all tasks", Description = "Retrieves a list of all tasks within the system.")]
        public async Task<IActionResult> GetAllTasks()
        {
            try
            {
                var tasks = await _taskQueueService.GetAllTasks();
                return Ok(tasks);
            }
            catch (Exception e)
            {
                LogException(e);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private void LogException(Exception e)
        {
            Console.WriteLine(e);
        }
    }
}
