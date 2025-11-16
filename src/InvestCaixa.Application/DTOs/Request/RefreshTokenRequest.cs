namespace InvestCaixa.Application.DTOs.Request;

public record RefreshTokenRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}
