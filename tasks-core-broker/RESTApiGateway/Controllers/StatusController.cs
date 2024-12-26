using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RESTApiGateway.Controllers
{
    [Authorize]
    [ApiController]
    [Route("status")]
    public class StatusController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public StatusController (IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStatusAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("TaskQueueClient");
            var response = await client.GetAsync("/queue/status");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
    }
}
