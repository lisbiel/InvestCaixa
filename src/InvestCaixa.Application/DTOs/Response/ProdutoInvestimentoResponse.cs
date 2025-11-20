namespace InvestCaixa.Application.DTOs.Response;

public class ProdutoInvestimentoResponse    
{
    /// <summary>
    /// Id único do produto
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Nome do produto
    /// </summary>
    /// <example>Fundo de Ações ABC</example>
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de Produto
    /// </summary>
    /// <example>CDB</example>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// Rentabilidade anual esperada em percentual
    /// </summary>
    /// <example>0.12</example>
    public decimal RentabilidadeAnual { get; set; }

    /// <summary>
    /// Valor mínimo de aplicação em Reais
    /// </summary>
    /// <example>1000.00</example>
    public decimal? ValorMinimo { get; set; }

    /// <summary>
    /// Perfil de investidor recomendado
    /// </summary>
    /// <example>Conservador</example>
    public string PerfilRecomendado { get; set; } = string.Empty;

    /// <summary>
    /// Nível de risco do produto
    /// </summary>
    /// <example>Baixo</example>
    public string NivelRisco { get; set; } = string.Empty;

    /// <summary>
    /// Indica se permite liquidez diaria
    /// </summary>
    /// <example>true</example>
    public bool PermiteLiquidez { get; set; }
}
