namespace InvestCaixa.API.Controllers;

using InvestCaixa.Application.DTOs.Response;
using InvestCaixa.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TelemetriaController : ControllerBase
{
    private readonly ITelemetriaService _telemetriaService;
    private readonly ILogger<TelemetriaController> _logger;

    public TelemetriaController(
        ITelemetriaService telemetriaService,
        ILogger<TelemetriaController> logger)
    {
        _telemetriaService = telemetriaService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém dados de telemetria da API
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(TelemetriaResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TelemetriaResponse>> ObterTelemetria(
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Obtendo dados de telemetria");

        var result = await _telemetriaService.ObterTelemetriaAsync(dataInicio, dataFim, cancellationToken);
        return Ok(result);
    }
}
