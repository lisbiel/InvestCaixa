namespace InvestCaixa.API.Controllers;

using InvestCaixa.Application.DTOs.Request;
using InvestCaixa.Application.DTOs.Response;
using InvestCaixa.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SimulacaoController : ControllerBase
{
    private readonly ISimulacaoService _simulacaoService;
    private readonly ILogger<SimulacaoController> _logger;

    public SimulacaoController(
        ISimulacaoService simulacaoService,
        ILogger<SimulacaoController> logger)
    {
        _simulacaoService = simulacaoService;
        _logger = logger;
    }

    /// <summary>
    /// Simula um investimento
    /// </summary>
    [HttpPost("simular-investimento")]
    [ProducesResponseType(typeof(SimulacaoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SimulacaoResponse>> SimularInvestimento(
        [FromBody] SimularInvestimentoRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recebida requisição de simulação para cliente {ClienteId}", 
            request.ClienteId);

        var resultado = await _simulacaoService.SimularInvestimentoAsync(
            request, 
            cancellationToken);

        return Ok(resultado);
    }

    /// <summary>
    /// Obtém todas as simulações realizadas
    /// </summary>
    [HttpGet("simulacoes")]
    [ProducesResponseType(typeof(IEnumerable<SimulacaoHistoricoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SimulacaoHistoricoResponse>>> ObterSimulacoes(
        CancellationToken cancellationToken)
    {
        var simulacoes = await _simulacaoService.ObterSimulacoesAsync(cancellationToken);
        return Ok(simulacoes);
    }

    /// <summary>
    /// Obtém simulações agrupadas por produto e dia
    /// </summary>
    [HttpGet("simulacoes/por-produto-dia")]
    [ProducesResponseType(typeof(IEnumerable<SimulacaoPorProdutoDiaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SimulacaoPorProdutoDiaResponse>>> 
        ObterSimulacoesPorProdutoDia(
            [FromQuery] DateTime? dataInicio,
            [FromQuery] DateTime? dataFim,
            CancellationToken cancellationToken)
    {
        var resultado = await _simulacaoService.ObterSimulacoesPorProdutoDiaAsync(
            dataInicio,
            dataFim,
            cancellationToken);

        return Ok(resultado);
    }

    /// <summary>
    /// Lista produtos de investimento disponíveis para simulação
    /// </summary>
    /// <remarks>
    /// Retorna os produtos cadastrados com seus IDs, nomes e tipos.
    /// </remarks>
    /// <response code="200">Lista de produtos disponíveis</response>
    [HttpGet("produtos-disponiveis")]
    [ProducesResponseType(typeof(IEnumerable<ProdutoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProdutoResponse>>> 
        ObterProdutosDisponiveis(CancellationToken cancellationToken)
    {
        var produtos = await _simulacaoService.ObterProdutosDisponiveisAsync(cancellationToken);
        return Ok(produtos);
    }
}
