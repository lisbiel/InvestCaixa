namespace InvestCaixa.Application.Services;

using System.Collections.Concurrent;
using System.Collections.Generic;
using InvestCaixa.Application.DTOs.Response;
using InvestCaixa.Application.Interfaces;
using Microsoft.Extensions.Logging;

public class TelemetriaService : ITelemetriaService
{
    private readonly ILogger<TelemetriaService> _logger;
    private static readonly ConcurrentDictionary<string, TelemetriaEndpoint> _telemetria = new();
    private static readonly ConcurrentQueue<RequestMetrics> _recentRequests = new();
    private static readonly ConcurrentDictionary<string, ErrorMetrics> _errorMetrics = new();

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

        var servicos = _telemetria.Values
            .Where(t => t.UltimaAtualizacao >= inicio && t.UltimaAtualizacao <= fim)
            .Select(t => new ServicoTelemetriaDto
            {
                Nome = t.Nome,
                QuantidadeChamadas = t.QuantidadeChamadas,
                MediaTempoRespostaMs = t.QuantidadeChamadas > 0 ? t.TempoTotalMs / t.QuantidadeChamadas : 0,
                MaxTempoRespostaMs = t.TempoMaximoMs,
                MinTempoRespostaMs = t.TempoMinimoMs == long.MaxValue ? 0 : t.TempoMinimoMs,
                TaxaSucesso = t.QuantidadeChamadas > 0 ? (decimal)(t.ChamadasSucesso / t.QuantidadeChamadas) * 100 : 0,
            }).ToList();

        var health = CalcularHealthMetrics(servicos);

        var topErros = _errorMetrics.Values
            .Where(e => e.UltimaOcorrencia >= inicio && e.UltimaOcorrencia <= fim)
            .OrderByDescending(e => e.Contagem)
            .Take(5)
            .Select(e => new ErroTelemetriaDto
            {
                Servico = e.Servico,
                TipoErro = e.TipoErro,
                Contagem = e.Contagem,
                PrimeiraOcorrencia = e.PrimeiraOcorrencia,
                UltimaOcorrencia = e.UltimaOcorrencia
            }).ToList();

        var response = new TelemetriaResponse
            {
                Servicos = servicos,
                HealthMetrics = health,
                Periodo = new PeriodoTelemetriaDto
                {
                    Inicio = inicio,
                    Fim = fim
                },
                TopErros = topErros
        };
        return Task.FromResult(response);
    }

    private HealthMetrics CalcularHealthMetrics(List<ServicoTelemetriaDto> servicos)
    {
        if(!servicos.Any())
        {
            return new HealthMetrics
            {
                StatusGeral = "Sem dados",
                TaxaSucessoGeral = 0,
                TempoRespostaMedioMs = 0
            };
        }

        return new HealthMetrics
        {
            TaxaSucessoGeral = servicos.Average(s => s.TaxaSucesso),
            TempoRespostaMedioMs = servicos.Average(s => s.MediaTempoRespostaMs),
            StatusGeral = servicos.Any(s => s.TaxaSucesso < 80) ? "Degradado" : "Saudável"
        };
    }

    public void RegistrarChamadaAvancada(string nomeServico, long tempoRespostaMs, bool sucesso = true, string? tipoErro = null, Dictionary<string, object>? contexto = null)
    {
        _telemetria.AddOrUpdate
            (
            nomeServico,
            _ => new TelemetriaEndpoint
            {
                Nome = nomeServico,
                QuantidadeChamadas = 1,
                TempoTotalMs = tempoRespostaMs,
                ChamadasSucesso = sucesso ? 1 : 0,
                ChamadasErro = sucesso ? 0 : 1,
                TempoMinimoMs = tempoRespostaMs,
                TempoMaximoMs = tempoRespostaMs,
                UltimaAtualizacao = DateTime.UtcNow
            },
            (_, telemetria) =>
            {
                telemetria.QuantidadeChamadas++;
                telemetria.TempoTotalMs += tempoRespostaMs;

                if (sucesso)
                {
                    telemetria.ChamadasSucesso++;
                }
                else
                {
                    telemetria.ChamadasErro++;
                }

                if (tempoRespostaMs < telemetria.TempoMinimoMs)
                {
                    telemetria.TempoMinimoMs = tempoRespostaMs;
                }

                if (tempoRespostaMs > telemetria.TempoMaximoMs)
                {
                    telemetria.TempoMaximoMs = tempoRespostaMs;
                }

                telemetria.UltimaAtualizacao = DateTime.UtcNow;
                return telemetria;
            });

        if(!sucesso && tipoErro is not null)
        {
            _logger.LogWarning("Erro registrado no serviço {Servico}: {TipoErro}", nomeServico, tipoErro);
            var errorKey = $"{nomeServico}:{tipoErro}";

            _errorMetrics.AddOrUpdate
            (
                errorKey,
                _ => new ErrorMetrics
                {
                    Servico = nomeServico,
                    TipoErro = tipoErro,
                    Contagem = 1,
                    PrimeiraOcorrencia = DateTime.UtcNow,
                    UltimaOcorrencia = DateTime.UtcNow
                },
                (_, errorMetrics) =>
                {
                    errorMetrics.Contagem++;
                    errorMetrics.UltimaOcorrencia = DateTime.UtcNow;
                    return errorMetrics;
                });
        }

        _recentRequests.Enqueue(new RequestMetrics
        {
            Servico = nomeServico,
            TempoRespostaMs = tempoRespostaMs,
            Sucesso = sucesso,
            Timestamp = DateTime.UtcNow,
            Contexto = contexto
        });


        while (_recentRequests.Count >= 1000)
        {
            _recentRequests.TryDequeue(out _);
        }

        if (sucesso)
            _logger.LogDebug("Chamada registrada no serviço {Servico} com tempo de resposta {TempoResposta} ms", nomeServico, tempoRespostaMs);
        else
            _logger.LogDebug("Chamada registrada no serviço {Servico} com tempo de resposta {TempoResposta} ms e erro do tipo {TipoErro}", nomeServico, tempoRespostaMs, tipoErro);
    }

    private class TelemetriaEndpoint
    {
        public string Nome { get; set; } = string.Empty;
        public long QuantidadeChamadas { get; set; }
        public long TempoTotalMs { get; set; }
        public long ChamadasSucesso { get; set; }
        public long ChamadasErro { get; set; }
        public long TempoMinimoMs { get; set; } = long.MaxValue;
        public long TempoMaximoMs { get; set; }
        public DateTime UltimaAtualizacao { get; set; } = DateTime.UtcNow;
    }

    private class ErrorMetrics
    {
        public string Servico { get; set; } = string.Empty;
        public string TipoErro { get; set; } = string.Empty;
        public long Contagem { get; set; }
        public DateTime PrimeiraOcorrencia { get; set; }
        public DateTime UltimaOcorrencia { get; set; }
    }

    private class RequestMetrics
    {        
        public string Servico { get; set; } = string.Empty;
        public long TempoRespostaMs { get; set; }
        public bool Sucesso { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object>? Contexto { get; set; }

    }
}
