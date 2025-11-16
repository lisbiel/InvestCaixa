namespace InvestCaixa.Application.Services;

using System.Collections.Concurrent;
using InvestCaixa.Application.DTOs.Response;
using InvestCaixa.Application.Interfaces;
using Microsoft.Extensions.Logging;

public class TelemetriaService : ITelemetriaService
{
    private readonly ILogger<TelemetriaService> _logger;
    private static readonly ConcurrentDictionary<string, TelemetriaEndpoint> _telemetria = new();

    public TelemetriaService(ILogger<TelemetriaService> logger)
    {
        _logger = logger;
    }

    public void RegistrarChamada(string nomeServico, long tempoRespostaMs)
    {
        _telemetria.AddOrUpdate(
            nomeServico,
            _ => new TelemetriaEndpoint
            {
                Nome = nomeServico,
                QuantidadeChamadas = 1,
                TempoTotalMs = tempoRespostaMs
            },
            (_, telemetria) =>
            {
                telemetria.QuantidadeChamadas++;
                telemetria.TempoTotalMs += tempoRespostaMs;
                return telemetria;
            });
    }

    public Task<TelemetriaResponse> ObterTelemetriaAsync(
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obtendo dados de telemetria");

        var inicio = dataInicio ?? DateTime.UtcNow.AddDays(-30);
        var fim = dataFim ?? DateTime.UtcNow;

        var servicos = _telemetria.Values.Select(t => new ServicoTelemetriaDto
        {
            Nome = t.Nome,
            QuantidadeChamadas = t.QuantidadeChamadas,
            MediaTempoRespostaMs = t.QuantidadeChamadas > 0 
                ? t.TempoTotalMs / t.QuantidadeChamadas 
                : 0
        }).ToList();

        var response = new TelemetriaResponse
        {
            Servicos = servicos,
            Periodo = new PeriodoTelemetriaDto
            {
                Inicio = inicio,
                Fim = fim
            }
        };

        return Task.FromResult(response);
    }

    private class TelemetriaEndpoint
    {
        public string Nome { get; set; } = string.Empty;
        public long QuantidadeChamadas { get; set; }
        public long TempoTotalMs { get; set; }
    }
}
