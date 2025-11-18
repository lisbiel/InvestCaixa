using InvestCaixa.Application.DTOs.Request;
using InvestCaixa.Application.Interfaces;
using InvestCaixa.Domain.Entities;
using InvestCaixa.Domain.Enums;
using InvestCaixa.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvestCaixa.API.Controllers;

// <summary>
// Controla o perfil financeiro mais completo do cliente/ estilo suitability
// </summary>
[ApiController]
[Route("api/perfil-financeiro")]
[Authorize]
public class PerfilFinanceiroController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPerfilRiscoService _perfilRiscoService;

    public PerfilFinanceiroController(IUnitOfWork unitOfWork, IPerfilRiscoService perfilRiscoService)
    {
        _unitOfWork = unitOfWork;
        _perfilRiscoService = perfilRiscoService;
    }

    /// <summary>
    /// Cria ou atualiza perfil financeiro do cliente
    /// </summary>
    [HttpPost("{clienteId}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CriarPerfilFinanceiro(
        int clienteId,
        [FromBody] CriarPerfilFinanceiroRequest request,
        CancellationToken cancellationToken)
    {
        var cliente = await _unitOfWork.ClienteRepository.GetByIdAsync(clienteId, cancellationToken);
        if (cliente is null) return NotFound("Cliente não encontrado");

        var perfilFinanceiroExistente = await _unitOfWork.PerfilFinanceiroRepository.ObterPorClienteAsync(clienteId, cancellationToken);

        var perfilRiscoExistente = await _unitOfWork.ClienteRepository.ObterPerfilRiscoAsync(clienteId, cancellationToken);

        PerfilFinanceiro perfilFinanceiro;
        var isNovoPerfil = false;

        if (perfilFinanceiroExistente != null)
        {
            perfilFinanceiroExistente.AtualizarDados(
                request.RendaMensal,
                request.PatrimonioTotal,
                request.DividasAtivas,
                request.DependentesFinanceiros,
                (HorizonteInvestimento)request.Horizonte,
                (ObjetivoInvestimento)request.Objetivo,
                request.ToleranciaPerda,
                request.ExperienciaInvestimentos);

            perfilFinanceiro = perfilFinanceiroExistente;
            await _unitOfWork.PerfilFinanceiroRepository.UpdateAsync(perfilFinanceiro, cancellationToken);
        }
        else
        {
            perfilFinanceiro = new PerfilFinanceiro(
                clienteId,
                request.RendaMensal,
                request.PatrimonioTotal,
                request.DividasAtivas,
                request.DependentesFinanceiros,
                (HorizonteInvestimento)request.Horizonte,
                (ObjetivoInvestimento)request.Objetivo,
                request.ToleranciaPerda,
                request.ExperienciaInvestimentos);
            await _unitOfWork.PerfilFinanceiroRepository.AddAsync(perfilFinanceiro, cancellationToken);
            isNovoPerfil = true;

            await _unitOfWork.PerfilFinanceiroRepository.AddAsync(perfilFinanceiro, cancellationToken);
            isNovoPerfil = true;
        }

        if (perfilRiscoExistente != null)
        {
            perfilRiscoExistente.AtualizarPerfil(perfilFinanceiro);
            await _unitOfWork.ClienteRepository.AtualizarPerfilRiscoAsync(perfilRiscoExistente, cancellationToken);
        }
        else
        {
            var novoPerfilRisco = new PerfilRisco(
                clienteId,
                volumeInvestimentos: 0,
                frequenciaMovimentacoes: 0,
                prefereLiquidez: true);

            novoPerfilRisco.AtualizarPerfil(perfilFinanceiro);
            await _unitOfWork.ClienteRepository.AdicionarPerfilRiscoAsync(novoPerfilRisco, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return isNovoPerfil ? CreatedAtAction(nameof(CriarPerfilFinanceiro), new { id = perfilFinanceiro.Id }) : Ok(new {message = "Perfil financeiro e de risco atualizados com sucesso", perfilFinanceiro});
    }
}
