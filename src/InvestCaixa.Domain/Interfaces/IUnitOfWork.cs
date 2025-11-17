namespace InvestCaixa.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ISimulacaoRepository SimulacaoRepository { get; }
    IProdutoRepository ProdutoRepository { get; }
    IClienteRepository ClienteRepository { get; }
    IInvestimentoFinalizadoRepository InvestimentoFinalizadoRepository { get; }
    IPerfilFinanceiroRepository PerfilFinanceiroRepository { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
