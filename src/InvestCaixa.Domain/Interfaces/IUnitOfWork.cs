namespace InvestCaixa.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ISimulacaoRepository SimulacaoRepository { get; }
    IProdutoRepository ProdutoRepository { get; }
    IClienteRepository ClienteRepository { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
