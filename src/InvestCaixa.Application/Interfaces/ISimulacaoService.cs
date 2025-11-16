using InvestCaixa.Application.DTOs.Request;
using InvestCaixa.Application.DTOs.Response;

namespace InvestCaixa.Application.Interfaces;

public interface ISimulacaoService
{
    Task<SimulacaoResponse> SimularInvestimentoAsync(
        SimularInvestimentoRequest request, 
        CancellationToken cancellationToken = default);

    Task<IEnumerable<SimulacaoHistoricoResponse>> ObterSimulacoesAsync(
        CancellationToken cancellationToken = default);

    Task<IEnumerable<SimulacaoPorProdutoDiaResponse>> ObterSimulacoesPorProdutoDiaAsync(
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        CancellationToken cancellationToken = default);
}

public record SimulacaoPorProdutoDiaResponse
{
    public string Produto { get; init; } = string.Empty;
    public DateTime Data { get; init; }
    public int QuantidadeSimulacoes { get; init; }
    public decimal MediaValorFinal { get; init; }
}
