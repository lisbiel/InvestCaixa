// PerfilFinanceiro.cs
namespace InvestCaixa.Domain.Entities;


public class PerfilFinanceiro : BaseEntity
{
    public int ClienteId { get; private set; }
    public decimal RendaMensal { get; private set; }
    public decimal PatrimonioTotal { get; private set; }
    public decimal DividasAtivas { get; private set; }
    public int DependentesFinanceiros { get; private set; }
    public HorizonteInvestimento Horizonte { get; private set; }
    public ObjetivoInvestimento Objetivo { get; private set; }
    public int ToleranciaPerda { get; private set; }
    public bool ExperienciaInvestimentos { get; private set; }
    public virtual Cliente Cliente { get; private set; } = null!;

    private PerfilFinanceiro() { }

    public PerfilFinanceiro(
        int clienteId,
        decimal rendaMensal,
        decimal patrimonioTotal,
        decimal dividasAtivas,
        int dependentes,
        HorizonteInvestimento horizonte,
        ObjetivoInvestimento objetivo,
        int toleranciaPerda,
        bool experiencia)
    {
        if (rendaMensal <= 0) throw new DomainException("Renda deve ser positiva");
        if (patrimonioTotal < 0) throw new DomainException("Patrimônio não pode ser negativo");
        if (toleranciaPerda < 0 || toleranciaPerda > 10) throw new DomainException("Tolerância deve estar entre 0 e 10");

        ClienteId = clienteId;
        RendaMensal = rendaMensal;
        PatrimonioTotal = patrimonioTotal;
        DividasAtivas = dividasAtivas;
        DependentesFinanceiros = dependentes;
        Horizonte = horizonte;
        Objetivo = objetivo;
        ToleranciaPerda = toleranciaPerda;
        ExperienciaInvestimentos = experiencia;
    }
}