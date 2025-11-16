namespace InvestCaixa.API.Controllers;

using InvestCaixa.Application.DTOs.Response;
using InvestCaixa.Application.Interfaces;
using InvestCaixa.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/perfil-risco")]
[Authorize]
public class PerfilRiscoController : ControllerBase
{
    private readonly IPerfilRiscoService _perfilRiscoService;
    private readonly ILogger<PerfilRiscoController> _logger;

    public PerfilRiscoController(
        IPerfilRiscoService perfilRiscoService,
        ILogger<PerfilRiscoController> logger)
    {
        _perfilRiscoService = perfilRiscoService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém o perfil de risco de um cliente
    /// </summary>
    [HttpGet("{clienteId}")]
    [ProducesResponseType(typeof(PerfilRiscoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PerfilRiscoResponse>> ObterPerfilRisco(
        int clienteId,
        CancellationToken cancellationToken)
    {
        var perfil = await _perfilRiscoService.ObterPerfilRiscoAsync(clienteId, cancellationToken);
        return Ok(perfil);
    }

    /// <summary>
    /// Obtém produtos recomendados para um perfil específico.
    /// 1 = Conservador, 2 = Moderado, 3 = Agressivo.
    /// </summary>
    [HttpGet("produtos-recomendados/{perfil}")]
    [ProducesResponseType(typeof(IEnumerable<ProdutoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProdutoResponse>>> ObterProdutosRecomendados(
        PerfilInvestidor perfil,
        CancellationToken cancellationToken)
    {
        var produtos = await _perfilRiscoService
            .ObterProdutosRecomendadosAsync(perfil, cancellationToken);

        return Ok(produtos);
    }
}
