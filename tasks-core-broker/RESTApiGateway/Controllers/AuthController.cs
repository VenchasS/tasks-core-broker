using Microsoft.AspNetCore.Mvc;
using RESTApiGateway.Services;
using RESTApiGateway.DTO;
using Prometheus;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private static readonly Counter TasksRecievedRequestsCount = Metrics
        .CreateCounter("taskController_jobs_requests_total", "Number of recieved requests");

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] AuthDTO request)
    {
        TasksRecievedRequestsCount.Inc();
        // Здесь можно реализовать логику проверки имени пользователя и пароля
        if (request.Username == "test" && request.Password == "password") // Простая проверка
        {
            var token = _authService.GenerateJwtToken(request.Username);
            return Ok(new { token });
        }

        return Unauthorized();
    }
}