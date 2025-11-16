namespace InvestCaixa.Application.DTOs.Response;

public record SimulacaoHistoricoResponse
{
    public Guid Id { get; init; }
    public int ClienteId { get; init; }
    public string Produto { get; init; } = string.Empty;
    public decimal ValorInvestido { get; init; }
    public decimal ValorFinal { get; init; }
    public int PrazoMeses { get; init; }
    public DateTime DataSimulacao { get; init; }
    public decimal Rentabilidade { get; init; }
}
