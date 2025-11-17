namespace InvestCaixa.Domain.Interfaces;

public interface IPerfilFinanceiroRepository : IRepository<PerfilFinanceiro>
{
    Task<PerfilFinanceiro?> ObterPorClienteAsync(int clienteId, CancellationToken cancellationToken);
}
