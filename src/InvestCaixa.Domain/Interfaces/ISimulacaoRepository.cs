namespace InvestCaixa.Domain.Interfaces;

public interface ISimulacaoRepository : IRepository<Simulacao>
{
    Task<IEnumerable<Simulacao>> ObterPorClienteAsync(int clienteId, CancellationToken cancellationToken = default);
    Task<IEnumerable<SimulacaoPorProdutoDia>> ObterSimulacoesPorProdutoDiaAsync(DateTime? dataInicio = null, DateTime? dataFim = null, CancellationToken cancellationToken = default);
}

public record SimulacaoPorProdutoDia
{
    public string Produto { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public int QuantidadeSimulacoes { get; set; }
    public decimal MediaValorFinal { get; set; }
}
