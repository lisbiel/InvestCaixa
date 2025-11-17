using InvestCaixa.Domain.Entities;
using InvestCaixa.Domain.Interfaces;
using InvestCaixa.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InvestCaixa.Infrastructure.Repositories;

public class PerfilFinanceiroRepository : Repository<PerfilFinanceiro>, IPerfilFinanceiroRepository
{
    public PerfilFinanceiroRepository(InvestimentoDbContext dbContext) : base(dbContext) { }

    public async Task<PerfilFinanceiro?> ObterPorClienteAsync(int clienteId, CancellationToken cancellationToken)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.ClienteId == clienteId && !p.IsDeleted, cancellationToken);
    }
}