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

    /// <summary>
    /// Obtém produtos recomendados por tipo considerando o perfil do cliente
    /// </summary>
    /// <param name="clienteId">ID do cliente</param>
    /// <param name="tipo">Tipo do produto (CDB, LCI, LCA, Fundo, etc.)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <remarks>
    /// Retorna produtos do tipo especificado ordenados por compatibilidade com o perfil de risco do cliente.
    /// Se o cliente não tiver perfil definido, retorna produtos ordenados por rentabilidade.
    /// </remarks>
    /// <response code="200">Lista de produtos recomendados para o cliente</response>
    /// <response code="404">Cliente não encontrado</response>
    [HttpGet("produtos-recomendados/{clienteId}/{tipo}")]
    [ProducesResponseType(typeof(IEnumerable<ProdutoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<ProdutoResponse>>> 
        ObterProdutosRecomendadosPorTipo(
            int clienteId, 
            string tipo,
            CancellationToken cancellationToken)
    {
        _logger.LogInformation("Obtendo produtos recomendados do tipo {Tipo} para cliente {ClienteId}", 
            tipo, clienteId);

        var produtos = await _simulacaoService.ObterProdutosRecomendadosPorTipoAsync(
            clienteId, tipo, cancellationToken);

        return Ok(produtos);
    }
}
