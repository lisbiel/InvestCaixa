namespace InvestCaixa.Application.Services;

using InvestCaixa.Application.DTOs.Request;
using InvestCaixa.Application.DTOs.Response;
using InvestCaixa.Application.Interfaces;
using Microsoft.Extensions.Logging;

public class AuthService : IAuthService
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger)
    {
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processando login para usuário {Username}", request.Usuario);

        // TODO: Validar credenciais contra banco de dados
        // Este é um exemplo simplificado para demo
        if (request.Usuario != "admin" || request.Senha != "Admin@123")
        {
            _logger.LogWarning("Falha de autenticação para usuário {Username}", request.Usuario);
            return null;
        }

        var token = _jwtTokenService.GenerateToken(request.Usuario);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        return new LoginResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
    }

    public async Task<LoginResponse?> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processando refresh token");

        // TODO: Validar refresh token no banco de dados
        if (string.IsNullOrEmpty(request.RefreshToken))
            return null;

        var token = _jwtTokenService.GenerateToken("admin");
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        return new LoginResponse
        {
            Token = token,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
    }
}
