using InvestCaixa.Domain.Enums;
using InvestCaixa.Domain.ValueObjects;

namespace InvestCaixa.Application.DTOs.Response;

public record SimulacaoResponse
{
    public Guid Id { get; init; }
    public ProdutoValidadoDto ProdutoValidado { get; init; } = null!;
    public ResultadoSimulacaoDto ResultadoSimulacao { get; init; } = null!;
    public DateTime DataSimulacao { get; init; }
    public DisclaimerRegulatorio Disclaimer { get; init; } = null!;
    public string AdequacaoPerfil { get; init; } = null!;
}

public record ProdutoValidadoDto
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Tipo { get; init; } = string.Empty;
    public decimal Rentabilidade { get; init; }
    public string Risco { get; init; } = string.Empty;
}

public record ResultadoSimulacaoDto
{
    public decimal ValorFinal { get; init; }
    public decimal RentabilidadeEfetiva { get; init; }
    public int PrazoMeses { get; init; }
}
