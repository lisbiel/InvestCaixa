namespace InvestCaixa.Domain.ValueObjects;

public record DisclaimerRegulatorio
{
    public string Texto { get; init; }
    public TipoDisclaimer Tipo { get; init; }
    public bool Obrigatorio { get; init; }

    public DisclaimerRegulatorio(string texto, TipoDisclaimer tipo, bool obrigatorio)
    {
        Texto = texto;
        Tipo = tipo;
        Obrigatorio = obrigatorio;
    }


    /*
     * public enum TipoProduto
    {
        CDB = 1,
        LCI = 2,
        LCA = 3,
        TesouroDireto = 4,
        Fundo = 5
    }*/
    public static DisclaimerRegulatorio ParaCDB() => new DisclaimerRegulatorio(
        texto: "Para produtos com garantia do FGC, o valor garantido é limitado a R$ 250.000,00 por CPF e por instituição financeira",
        tipo: TipoDisclaimer.FGC,
        obrigatorio: true
    );

    public static DisclaimerRegulatorio ParaRendaFixa() => new DisclaimerRegulatorio(
        texto: "Investimentos em renda fixa estão sujeitos a riscos de mercado, incluindo variações nas taxas de juros e inflação.",
        tipo: TipoDisclaimer.BaixoRisco,
        obrigatorio: true
    );

    public static DisclaimerRegulatorio ParaRendaVariavel() => new DisclaimerRegulatorio(
        texto: "Lucro passado não é garantia de lucro futuro. Investimento em fundos podem apresentar variãção de rentabilidade e risco de perda de capital.",
        tipo: TipoDisclaimer.RiscoVariabilidade,
        obrigatorio: true
    );
}
