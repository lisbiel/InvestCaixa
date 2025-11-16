namespace InvestCaixa.API.Controllers;

using InvestCaixa.Application.DTOs.Request;
using InvestCaixa.Application.DTOs.Response;
using InvestCaixa.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Realiza login e retorna token JWT
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Tentativa de login para usuário {Username}", request.Usuario);

        var response = await _authService.LoginAsync(request, cancellationToken);

        if (response == null)
        {
            _logger.LogWarning("Falha no login para usuário {Username}", request.Usuario);
            return Unauthorized(new ProblemDetails
            {
                Title = "Falha na autenticação",
                Detail = "Credenciais inválidas",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        _logger.LogInformation("Login bem-sucedido para usuário {Username}", request.Usuario);
        return Ok(response);
    }

    /// <summary>
    /// Renova o token JWT usando refresh token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.RefreshTokenAsync(request, cancellationToken);

        if (response == null)
            return Unauthorized();

        return Ok(response);
    }
}
