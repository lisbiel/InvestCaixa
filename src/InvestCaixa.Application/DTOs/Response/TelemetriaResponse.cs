namespace InvestCaixa.Application.DTOs.Response;

public record TelemetriaResponse
{
    public PeriodoTelemetriaDto Periodo { get; init; } = null!;
    public List<ServicoTelemetriaDto> Servicos { get; init; } = new();
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
}
