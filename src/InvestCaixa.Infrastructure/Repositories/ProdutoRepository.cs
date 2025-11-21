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

        return _dbSet
            .Where(p => p.Tipo == tipoProduto)
            .AsNoTracking()
            .ToList()
            .OrderBy(p => p.Rentabilidade)
            .FirstOrDefault();
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

    public async Task<IEnumerable<ProdutoInvestimento>> ObterPorTipoEPerfilAsync(
        string tipo,
        PerfilInvestidor? perfil = null,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<TipoProduto>(tipo, true, out var tipoProduto))
            return Enumerable.Empty<ProdutoInvestimento>();

        var query = _dbSet.Where(p => p.Tipo == tipoProduto);

        // Se perfil foi especificado, priorizar produtos recomendados para esse perfil
        if (perfil.HasValue)
        {
            query = query.OrderBy(p => p.PerfilRecomendado == perfil.Value ? 0 : 1)
                         .ThenBy(p => Math.Abs((int)p.PerfilRecomendado - (int)perfil.Value))
                         .ThenByDescending(p => p.Rentabilidade);
        }
        else
        {
            // Sem perfil, ordenar apenas por rentabilidade
            query = query.OrderBy(p => p.Rentabilidade);
        }

        return await query.AsNoTracking().ToListAsync(cancellationToken);
    }
}
