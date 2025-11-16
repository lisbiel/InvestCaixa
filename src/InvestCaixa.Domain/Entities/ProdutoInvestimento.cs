namespace InvestCaixa.Domain.Entities;

public class ProdutoInvestimento : BaseEntity
{
    public string Nome { get; private set; }
    public TipoProduto Tipo { get; private set; }
    public decimal Rentabilidade { get; private set; }
    public NivelRisco Risco { get; private set; }
    public int PrazoMinimoDias { get; private set; }
    public decimal ValorMinimoAplicacao { get; private set; }
    public bool PermiteLiquidez { get; private set; }
    public PerfilInvestidor PerfilRecomendado { get; private set; }

    private ProdutoInvestimento() { }

    public ProdutoInvestimento(
        string nome,
        TipoProduto tipo,
        decimal rentabilidade,
        NivelRisco risco,
        int prazoMinimoDias,
        decimal valorMinimoAplicacao,
        bool permiteLiquidez,
        PerfilInvestidor perfilRecomendado)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome do produto não pode estar vazio");

        if (rentabilidade < 0)
            throw new DomainException("Rentabilidade não pode ser negativa");

        if (valorMinimoAplicacao <= 0)
            throw new DomainException("Valor mínimo de aplicação deve ser positivo");

        Nome = nome;
        Tipo = tipo;
        Rentabilidade = rentabilidade;
        Risco = risco;
        PrazoMinimoDias = prazoMinimoDias;
        ValorMinimoAplicacao = valorMinimoAplicacao;
        PermiteLiquidez = permiteLiquidez;
        PerfilRecomendado = perfilRecomendado;
    }

    public void AtualizarRentabilidade(decimal novaRentabilidade)
    {
        if (novaRentabilidade < 0)
            throw new DomainException("Rentabilidade não pode ser negativa");

        Rentabilidade = novaRentabilidade;
        UpdatedAt = DateTime.UtcNow;
    }
}
