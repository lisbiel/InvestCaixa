namespace InvestCaixa.Application.DTOs.Request;

public record SimularInvestimentoRequest
{
    public int ClienteId { get; init; }
    public decimal Valor { get; init; }
    public int PrazoMeses { get; init; }
    public string TipoProduto { get; init; } = string.Empty;
}
