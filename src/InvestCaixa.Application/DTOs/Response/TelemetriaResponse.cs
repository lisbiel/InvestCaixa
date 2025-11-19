using StackExchange.Redis;

namespace InvestCaixa.Application.DTOs.Response;

public record TelemetriaResponse
{
    public PeriodoTelemetriaDto Periodo { get; init; } = null!;
    public List<ServicoTelemetriaDto> Servicos { get; init; } = new();
    public HealthMetrics HealthMetrics { get; init; } = null!;
    public List<ErroTelemetriaDto> TopErros { get; init; } = new();
}

public record PeriodoTelemetriaDto
{
    public DateTime Inicio { get; init; }
    public DateTime Fim { get; init; }
}

public record ServicoTelemetriaDto
{
    public string Nome { get; init; } = string.Empty;
    public long QuantidadeChamadas { get; init; }
    public decimal MediaTempoRespostaMs { get; init; }
    public decimal MinTempoRespostaMs { get; init; }
    public decimal MaxTempoRespostaMs { get; init; }
    public decimal TaxaSucesso { get; init; }
    public DateTime UltimaAtualizacao { get; init; }
}

public record HealthMetrics
{
    public decimal TaxaSucessoGeral { get; init; }
    public decimal TempoRespostaMedioMs { get; init; }
    public string StatusGeral { get; init; } = "Desconhecido";
}

public record ErroTelemetriaDto
{
    public String Servico { get; init; } = string.Empty;
    public String TipoErro { get; init; } = string.Empty;
    public long Contagem { get; init; }
    public DateTime PrimeiraOcorrencia { get; init; }
    public DateTime UltimaOcorrencia { get; init; }
}