using InvestCaixa.Domain.Entities;
using InvestCaixa.Domain.Interfaces;
using InvestCaixa.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InvestCaixa.Infrastructure.Repositories;

public class InvestimentoFinalizadoRepository : Repository<InvestimentoFinalizado>, IInvestimentoFinalizadoRepository
{
    public InvestimentoFinalizadoRepository(InvestimentoDbContext dbContext) : base(dbContext) { }

    public async Task<IEnumerable<InvestimentoFinalizado>> ObterPorClienteAsync(int clienteId, CancellationToken cancellationToken)
    {
        return await _dbSet
            .Include(i => i.Produto)
            .Where(i => i.ClienteId == clienteId && !i.IsDeleted)
            .OrderByDescending(i => i.DataAplicacao)
            .ToListAsync(cancellationToken);
    }
}