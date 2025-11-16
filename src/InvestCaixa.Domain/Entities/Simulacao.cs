namespace InvestCaixa.Domain.Entities;

public class Simulacao : BaseEntity
{
    public int ClienteId { get; private set; }
    public string ProdutoSimulado { get; private set; }
    public Guid ProdutoId { get; private set; }
    public decimal ValorInvestido { get; private set; }
    public decimal ValorFinal { get; private set; }
    public int PrazoMeses { get; private set; }
    public DateTime DataSimulacao { get; private set; }
    public decimal RentabilidadeCalculada { get; private set; }

    // Navigation properties
    public virtual Cliente Cliente { get; private set; } = null!;
    public virtual ProdutoInvestimento Produto { get; private set; } = null!;

    private Simulacao() { }

    public Simulacao(
        int clienteId,
        string produto,
        Guid produtoId,
        decimal valorInvestido,
        decimal valorFinal,
        int prazoMeses,
        DateTime dataSimulacao)
    {
        if (valorInvestido <= 0)
            throw new DomainException("Valor investido deve ser maior que zero");

        if (valorFinal < valorInvestido)
            throw new DomainException("Valor final não pode ser menor que valor investido");

        if (prazoMeses <= 0)
            throw new DomainException("Prazo deve ser maior que zero");

        ClienteId = clienteId;
        ProdutoSimulado = produto;
        ProdutoId = produtoId;
        ValorInvestido = valorInvestido;
        ValorFinal = valorFinal;
        PrazoMeses = prazoMeses;
        DataSimulacao = dataSimulacao;
        RentabilidadeCalculada = CalcularRentabilidade();
    }

    private decimal CalcularRentabilidade()
    {
        return (ValorFinal - ValorInvestido) / ValorInvestido;
    }
}
