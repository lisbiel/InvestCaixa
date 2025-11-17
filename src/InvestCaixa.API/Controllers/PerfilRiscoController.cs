namespace InvestCaixa.API.Controllers;

using InvestCaixa.Application.DTOs.Response;
using InvestCaixa.Application.Handlers.Queries;
using InvestCaixa.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/perfil-risco")]
[Authorize]
public class PerfilRiscoController : ControllerBase
{
    private readonly IMediator _mediator;

    public PerfilRiscoController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Obtém o perfil de risco do cliente
    /// </summary>
    [HttpGet("{clienteId}")]
    [ProducesResponseType(typeof(PerfilRiscoResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterPerfilRisco(int clienteId, CancellationToken cancellationToken)
    {
        var query = new ObterPerfilRiscoQuery(clienteId);
        var resultado = await _mediator.Send(query, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// Obtém produtos recomendados para um perfil específico
    /// 1 = Conservador, 2 = Moderado, 3 = Agressivo
    /// </summary>
    [HttpGet("produtos-recomendados/{perfil}")]
    [ProducesResponseType(typeof(IEnumerable<ProdutoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterProdutosRecomendados(
        PerfilInvestidor perfil,
        CancellationToken cancellationToken)
    {
        var query = new ObterProdutosRecomendadosQuery((int)perfil);
        var resultado = await _mediator.Send(query, cancellationToken);
        return Ok(resultado);
    }
}
