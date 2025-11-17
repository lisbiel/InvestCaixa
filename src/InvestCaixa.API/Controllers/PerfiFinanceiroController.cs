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

        var perfilExistente = await _unitOfWork.ClienteRepository.ObterPerfilRiscoAsync(clienteId, cancellationToken);

        var perfil = new PerfilFinanceiro(
            clienteId,
            request.RendaMensal,
            request.PatrimonioTotal,
            request.DividasAtivas,
            request.DependentesFinanceiros,
            (HorizonteInvestimento)request.Horizonte,
            (ObjetivoInvestimento)request.Objetivo,
            request.ToleranciaPerda,
            request.ExperienciaInvestimentos);

        perfilExistente.AtualizarPerfil(perfil);

        await _unitOfWork.PerfilFinanceiroRepository.AddAsync(perfil, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);



        return CreatedAtAction(nameof(CriarPerfilFinanceiro), new { id = perfil.Id });
    }
}
