using InvestCaixa.Domain.Interfaces;
using InvestCaixa.Infrastructure.Data;

namespace InvestCaixa.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly InvestimentoDbContext _context;
    private ISimulacaoRepository? _simulacaoRepository;
    private IProdutoRepository? _produtoRepository;
    private IClienteRepository? _clienteRepository;
    private IInvestimentoFinalizadoRepository? _investimentoFinalizadoRepository;
    private IPerfilFinanceiroRepository? _perfilFinanceiroRepository;

    public UnitOfWork(InvestimentoDbContext context)
    {
        _context = context;
    }

    public ISimulacaoRepository SimulacaoRepository => 
        _simulacaoRepository ??= new SimulacaoRepository(_context);

    public IProdutoRepository ProdutoRepository => 
        _produtoRepository ??= new ProdutoRepository(_context);

    public IClienteRepository ClienteRepository => 
        _clienteRepository ??= new ClienteRepository(_context);

    public IInvestimentoFinalizadoRepository InvestimentoFinalizadoRepository => 
        _investimentoFinalizadoRepository ??= new InvestimentoFinalizadoRepository(_context);
    public IPerfilFinanceiroRepository PerfilFinanceiroRepository => 
        _perfilFinanceiroRepository ??= new PerfilFinanceiroRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
