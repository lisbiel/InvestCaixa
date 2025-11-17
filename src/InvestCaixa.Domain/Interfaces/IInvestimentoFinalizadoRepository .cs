namespace InvestCaixa.Domain.Interfaces;

public interface IInvestimentoFinalizadoRepository : IRepository<InvestimentoFinalizado>
{
    Task<IEnumerable<InvestimentoFinalizado>> ObterPorClienteAsync(int clienteId, CancellationToken cancellationToken);
}
