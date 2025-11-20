using System.ComponentModel;

namespace InvestCaixa.Domain.Enums;

public enum PerfilInvestidor
{
    [Description("Conservador")]
    Conservador = 1,
    [Description("Moderado")]
    Moderado = 2,
    [Description("Agressivo")]
    Agressivo = 3
}
