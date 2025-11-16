namespace InvestCaixa.Application.DTOs.Response;

public record ProdutoResponse
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Tipo { get; init; } = string.Empty;
    public decimal Rentabilidade { get; init; }
    public string Risco { get; init; } = string.Empty;
    public int PrazoMinimoDias { get; init; }
    public decimal ValorMinimoAplicacao { get; init; }
    public bool PermiteLiquidez { get; init; }
    public string PerfilRecomendado { get; init; } = string.Empty;
}
