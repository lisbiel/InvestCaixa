using InvestCaixa.Application.DTOs.Request;
using InvestCaixa.Application.DTOs.Response;

namespace InvestCaixa.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse?> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
}
