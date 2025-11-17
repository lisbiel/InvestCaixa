namespace InvestCaixa.Domain.Entities;

public class PerfilRisco : BaseEntity
{
    public int ClienteId { get; private set; }
    public PerfilInvestidor Perfil { get; private set; }
    public int Pontuacao { get; private set; }
    public string Descricao { get; private set; }
    public decimal VolumeInvestimentos { get; private set; }
    public int FrequenciaMovimentacoes { get; private set; }
    public bool PrefereLiquidez { get; private set; }

    // Navigation property
    public virtual Cliente Cliente { get; private set; } = null!;

    private PerfilRisco() 
    {
        Descricao = string.Empty;
    }

    public PerfilRisco(
        int clienteId,
        decimal volumeInvestimentos,
        int frequenciaMovimentacoes,
        bool prefereLiquidez,
        PerfilFinanceiro? dadosFinanceiros = null)
    {
        ClienteId = clienteId;
        VolumeInvestimentos = volumeInvestimentos;
        FrequenciaMovimentacoes = frequenciaMovimentacoes;
        PrefereLiquidez = prefereLiquidez;

        CalcularPerfil(dadosFinanceiros);
    }

    public void AtualizarDados(
        decimal volumeInvestimentos,
        int frequenciaMovimentacoes,
        bool prefereLiquidez,
        PerfilFinanceiro? perfilFinanceiro = null)
    {
        VolumeInvestimentos = volumeInvestimentos;
        FrequenciaMovimentacoes = frequenciaMovimentacoes;
        PrefereLiquidez = prefereLiquidez;
        AtualizarPerfil(perfilFinanceiro);
        UpdatedAt = DateTime.UtcNow;
    }

    private void CalcularPerfil(PerfilFinanceiro? dadosFinanceiros)
    {
        int pontos = 0;

        // ===== ANÁLISE BÁSICA (HISTÓRICO) =====

        // Volume investido (0-30 pontos)
        if (VolumeInvestimentos < 10_000) pontos += 10;
        else if (VolumeInvestimentos < 50_000) pontos += 20;
        else if (VolumeInvestimentos < 100_000) pontos += 25;
        else pontos += 30;

        // Frequência (0-20 pontos)
        if (FrequenciaMovimentacoes < 3) pontos += 5;
        else if (FrequenciaMovimentacoes < 10) pontos += 12;
        else pontos += 20;

        // Liquidez (0-10 pontos)
        pontos += PrefereLiquidez ? 5 : 10;

        // ===== ANÁLISE AVANÇADA (PERFIL FINANCEIRO) =====
        if (dadosFinanceiros != null)
        {
            // % do patrimônio investido (0-15 pontos)
            var percentualInvestido = dadosFinanceiros.PatrimonioTotal > 0
                ? VolumeInvestimentos / dadosFinanceiros.PatrimonioTotal
                : 0;

            if (percentualInvestido < 0.1m) pontos += 5;
            else if (percentualInvestido < 0.3m) pontos += 10;
            else pontos += 15;

            // Tolerância a risco (0-15 pontos)
            pontos += dadosFinanceiros.ToleranciaPerda switch
            {
                <= 3 => 5,
                <= 6 => 10,
                _ => 15
            };

            // Horizonte de investimento (0-10 pontos)
            pontos += dadosFinanceiros.Horizonte switch
            {
                HorizonteInvestimento.CurtoPrazo => 3,
                HorizonteInvestimento.MedioPrazo => 7,
                HorizonteInvestimento.LongoPrazo => 10,
                _ => 5
            };
        }

        Pontuacao = pontos;

        // ===== CLASSIFICAÇÃO (0-100) =====
        if (pontos <= 35)
        {
            Perfil = PerfilInvestidor.Conservador;
            Descricao = "Perfil conservador: prioriza segurança e liquidez, baixa tolerância a risco";
        }
        else if (pontos <= 65)
        {
            Perfil = PerfilInvestidor.Moderado;
            Descricao = "Perfil moderado: busca equilíbrio entre segurança e rentabilidade";
        }
        else
        {
            Perfil = PerfilInvestidor.Agressivo;
            Descricao = "Perfil agressivo: foco em rentabilidade, aceita volatilidade e risco elevado";
        }
    }

    // Para recalcular quando houver atualização de dados
    public void AtualizarPerfil(PerfilFinanceiro? dadosFinanceiros)
    {
        CalcularPerfil(dadosFinanceiros);
    }
}
