namespace InvestCaixa.Application.DTOs.Response;

public record PerfilRiscoResponse
{
    public Guid Id { get; init; }
    public int ClienteId { get; init; }
    public string Perfil { get; init; } = string.Empty;
    public int Pontuacao { get; init; }
    public string Descricao { get; init; } = string.Empty;
}
