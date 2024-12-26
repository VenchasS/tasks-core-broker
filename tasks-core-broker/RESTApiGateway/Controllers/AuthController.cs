using Microsoft.AspNetCore.Mvc;
using RESTApiGateway.Services;
using RESTApiGateway.DTO;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] AuthDTO request)
    {
        // Здесь можно реализовать логику проверки имени пользователя и пароля
        if (request.Username == "test" && request.Password == "password") // Простая проверка
        {
            var token = _authService.GenerateJwtToken(request.Username);
            return Ok(new { token });
        }

        return Unauthorized();
    }
}