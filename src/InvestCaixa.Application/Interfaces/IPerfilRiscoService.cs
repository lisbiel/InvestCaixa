using InvestCaixa.Application.DTOs.Response;

namespace InvestCaixa.Application.Interfaces;

public interface IPerfilRiscoService
{
    Task<PerfilRiscoResponse> ObterPerfilRiscoAsync(
        int clienteId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ProdutoResponse>> ObterProdutosRecomendadosAsync(
        Domain.Enums.PerfilInvestidor perfil,
        CancellationToken cancellationToken = default);

    Task AtualizarPerfilAsync(
        int clienteId,
        CancellationToken cancellationToken = default);
}
