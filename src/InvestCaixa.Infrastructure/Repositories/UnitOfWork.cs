using InvestCaixa.Domain.Interfaces;
using InvestCaixa.Infrastructure.Data;

namespace InvestCaixa.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly InvestimentoDbContext _context;
    public ISimulacaoRepository? SimulacaoRepository { get; }
    public IProdutoRepository? ProdutoRepository { get; }
    public IClienteRepository? ClienteRepository { get; }
    public IInvestimentoFinalizadoRepository? InvestimentoFinalizadoRepository { get; }
    public IPerfilFinanceiroRepository? PerfilFinanceiroRepository { get; }

    public UnitOfWork(InvestimentoDbContext context)
    {
        _context = context;
    }

    public UnitOfWork(
        InvestimentoDbContext context,
        ISimulacaoRepository simulacaoRepository,
        IProdutoRepository produtoRepository,
        IClienteRepository clienteRepository,
        IInvestimentoFinalizadoRepository investimentoFinalizadoRepository,
        IPerfilFinanceiroRepository perfilFinanceiroRepository)
    {
        _context = context;
        SimulacaoRepository = simulacaoRepository;
        ProdutoRepository = produtoRepository;                         // <- aqui vem o CachingProdutoRepository
        ClienteRepository = clienteRepository;
        InvestimentoFinalizadoRepository = investimentoFinalizadoRepository;
        PerfilFinanceiroRepository = perfilFinanceiroRepository;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
