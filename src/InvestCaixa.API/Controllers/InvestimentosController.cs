using InvestCaixa.Application.DTOs.Request;
using InvestCaixa.Application.DTOs.Response;
using InvestCaixa.Application.Handlers.Commands;
using InvestCaixa.Application.Handlers.Queries;
using InvestCaixa.Domain.Entities;
using InvestCaixa.Domain.Enums;
using InvestCaixa.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvestCaixa.API.Controllers;

// <summary>
// Controla finaliza um novo investimento e obtém investimentos ativos dos clientes
//</summary>
[ApiController]
[Route("api/investimentos")]
[Authorize]
public class InvestimentosController : ControllerBase
{
    private readonly IMediator _mediator;

    public InvestimentosController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Finaliza uma simulação como investimento real
    /// </summary>
    [HttpPost("finalizar")]
    [ProducesResponseType(typeof(InvestimentoFinalizadoResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Finalizar(
        [FromBody] FinalizarInvestimentoRequest request,
        CancellationToken cancellationToken)
    {
        var command = new FinalizarInvestimentoCommand(request.ClienteId, request.ProdutoId, request.ValorAplicado);
        var investimentoId = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Finalizar), new { id = investimentoId });
    }

    /// <summary>
    /// Obtém histórico completo: simulações + investimentos finalizados com rentabilidade real
    /// </summary>
    [HttpGet("historico/{clienteId}")]
    [ProducesResponseType(typeof(HistoricoCompletoResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterHistoricoCompleto(int clienteId, CancellationToken cancellationToken)
    {
        var query = new ObterHistoricoCompletoQuery(clienteId);
        var resultado = await _mediator.Send(query, cancellationToken);
        return Ok(resultado);
    }
}