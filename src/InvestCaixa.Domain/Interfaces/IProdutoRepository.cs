namespace InvestCaixa.Domain.Interfaces;

public interface IProdutoRepository : IRepository<ProdutoInvestimento>
{
    Task<ProdutoInvestimento?> ObterPorTipoAsync(string tipo, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProdutoInvestimento>> ObterPorPerfilAsync(PerfilInvestidor perfil, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProdutoInvestimento>> ObterPorCriteriosAsync(decimal? valorMinimo = null, NivelRisco? nivelRisco = null, bool? permiteLiquidez = null, CancellationToken cancellationToken = default);
}
