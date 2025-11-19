using System.ComponentModel;

namespace InvestCaixa.Domain.Enums;

public enum AdequacaoPerfil
{
    [Description("Não Avaliado")]
    NaoAvaliado,
    [Description("Adequado")]
    Adequado,
    [Description("Inadequado - Baixo Risco")]
    InadequadoBaixoRisco,
    [Description("Inadequado - Alto Risco")]
    InadequadoAltoRisco
}
