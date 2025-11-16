namespace InvestCaixa.Domain.Interfaces;

public interface IClienteRepository : IRepositoryInt<Cliente>
{
    Task<Cliente?> ObterPorCPFAsync(string cpf, CancellationToken cancellationToken = default);
    Task<Cliente?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<PerfilRisco?> ObterPerfilRiscoAsync(int clienteId, CancellationToken cancellationToken = default);
    Task AdicionarPerfilRiscoAsync(PerfilRisco perfilRisco, CancellationToken cancellationToken = default);
}
