namespace InvestCaixa.Infrastructure.Repositories;

using InvestCaixa.Domain.Entities;
using InvestCaixa.Domain.Enums;
using InvestCaixa.Domain.Interfaces;
using InvestCaixa.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class ProdutoRepository : Repository<ProdutoInvestimento>, IProdutoRepository
{
    public ProdutoRepository(InvestimentoDbContext context) : base(context)
    {
    }

    public async Task<ProdutoInvestimento?> ObterPorTipoAsync(
        string tipo,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<TipoProduto>(tipo, true, out var tipoProduto))
            return null;

        return await _dbSet
            .Where(p => p.Tipo == tipoProduto)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProdutoInvestimento>> ObterPorPerfilAsync(
        PerfilInvestidor perfil,
        CancellationToken cancellationToken = default)
    {
        return (await _dbSet
            .Where(p => p.PerfilRecomendado == perfil)
            .OrderBy(p => p.Risco)
            .AsNoTracking()
            .ToListAsync(cancellationToken))
            .OrderByDescending(p => p.Rentabilidade);
    }

    public async Task<IEnumerable<ProdutoInvestimento>> ObterPorCriteriosAsync(
        decimal? valorMinimo = null,
        NivelRisco? nivelRisco = null,
        bool? permiteLiquidez = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (valorMinimo.HasValue)
            query = query.Where(p => p.ValorMinimoAplicacao <= valorMinimo.Value);

        if (nivelRisco.HasValue)
            query = query.Where(p => p.Risco == nivelRisco.Value);

        if (permiteLiquidez.HasValue)
            query = query.Where(p => p.PermiteLiquidez == permiteLiquidez.Value);

        return await query
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
