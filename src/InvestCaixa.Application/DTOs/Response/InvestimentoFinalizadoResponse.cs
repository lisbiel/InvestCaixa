namespace InvestCaixa.Application.DTOs.Response;
public class InvestimentoFinalizadoResponse
{
    public Guid Id { get; set; }
    public int ClienteId { get; set; }
    public Guid ProdutoId { get; set; }
    public string ProdutoNome { get; set; } = string.Empty;
    public decimal ValorAplicado { get; set; }
    public decimal ValorResgatado { get; set; }
    public DateTime DataAplicacao { get; set; }
    public DateTime? DataResgate { get; set; }
    public int Status { get; set; }
    public decimal RentabilidadeReal { get; set; }
}