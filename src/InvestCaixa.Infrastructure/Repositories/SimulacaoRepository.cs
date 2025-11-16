namespace InvestCaixa.Infrastructure.Repositories;

using InvestCaixa.Domain.Entities;
using InvestCaixa.Domain.Interfaces;
using InvestCaixa.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class SimulacaoRepository : Repository<Simulacao>, ISimulacaoRepository
{
    public SimulacaoRepository(InvestimentoDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Simulacao>> ObterPorClienteAsync(
        int clienteId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Produto)
            .Include(s => s.Cliente)
            .Where(s => s.ClienteId == clienteId)
            .OrderByDescending(s => s.DataSimulacao)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SimulacaoPorProdutoDia>> ObterSimulacoesPorProdutoDiaAsync(
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        CancellationToken cancellationToken = default)
    {
        var inicio = dataInicio ?? DateTime.UtcNow.AddDays(-30);
        var fim = dataFim ?? DateTime.UtcNow;

        var grupos = await _dbSet
        .Include(s => s.Produto)
        .Where(s => s.DataSimulacao.Date >= inicio.Date && s.DataSimulacao.Date <= fim.Date)
        .AsNoTracking()
        .ToListAsync(cancellationToken);

        var resultado = grupos
            .GroupBy(s => new
            {
                ProdutoNome = s.Produto.Nome,
                Data = s.DataSimulacao.Date
            })
            .Select(g => new SimulacaoPorProdutoDia
            {
                Produto = g.Key.ProdutoNome,
                Data = g.Key.Data,
                QuantidadeSimulacoes = g.Count(),
                MediaValorFinal = g.Average(s => s.ValorFinal)
            })
            .ToList();


        return resultado;
    }
}
