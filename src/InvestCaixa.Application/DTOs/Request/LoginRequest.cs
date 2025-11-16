using System.ComponentModel.DataAnnotations;

namespace InvestCaixa.Application.DTOs.Request;

public record LoginRequest
{
    [Required]
    public string Usuario { get; init; } = string.Empty;
    [Required]
    public string Senha { get; init; } = string.Empty;
}
