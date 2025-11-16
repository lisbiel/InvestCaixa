namespace InvestCaixa.Infrastructure.Repositories;

using InvestCaixa.Domain.Entities;
using InvestCaixa.Domain.Interfaces;
using InvestCaixa.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class ClienteRepository : RepositoryInt<Cliente>, IClienteRepository
{
    public ClienteRepository(InvestimentoDbContext context) : base(context)
    {
    }

    public async Task<Cliente?> ObterPorCPFAsync(
        string cpf,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.CPF == cpf)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Cliente?> ObterPorEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.Email == email)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PerfilRisco?> ObterPerfilRiscoAsync(
        int clienteId,
        CancellationToken cancellationToken = default)
    {
        return await _context.PerfisRisco
            .Where(p => p.ClienteId == clienteId)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AdicionarPerfilRiscoAsync(
        PerfilRisco perfilRisco,
        CancellationToken cancellationToken = default)
    {
        await _context.PerfisRisco.AddAsync(perfilRisco, cancellationToken);
    }
}
