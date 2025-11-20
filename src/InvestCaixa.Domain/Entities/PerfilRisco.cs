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
        pontos += VolumeInvestimentos switch
        {
            >= 1_000_000 => 30,
            >= 500_000 => 25,
            >= 100_000 => 20,
            >= 50_000 => 15,
            >= 10_000 => 10,
            _ => 5
        };

        // Frequência (0-20 pontos)
        pontos += FrequenciaMovimentacoes switch
        {
            >= 20 => 20,
            >= 10 => 15,
            >= 5 => 10,
            >= 1 => 5,
            _ => 0
        };

        // Liquidez (0-10 pontos)
        pontos += PrefereLiquidez ? 3 : 10;

        // ===== ANÁLISE AVANÇADA (PERFIL FINANCEIRO) =====
        if (dadosFinanceiros != null)
        {
            // % do patrimônio investido (0-15 pontos)
            var percentualInvestido = dadosFinanceiros.PatrimonioTotal > 0
                ? VolumeInvestimentos / dadosFinanceiros.PatrimonioTotal
                : 0;

            // Ajuste: reduzir o peso do percentual investido para evitar que clientes com
            // patrimônio declarado baixo sejam promovidos com facilidade a perfil mais alto.
            var percentualPoints = percentualInvestido switch
            {
                >= 0.5m => 10,
                >= 0.3m => 7,
                >= 0.25m => 6,
                >= 0.1m => 3,
                _ => 1
            };
            pontos += percentualPoints;

            // Tolerância a risco (0-20 pontos) - aumentado de 15
            pontos += (dadosFinanceiros.ToleranciaPerda * 20) / 10;

            // Horizonte de investimento (0-15 pontos) - aumentado de 10
            pontos += dadosFinanceiros.Horizonte switch
            {
                HorizonteInvestimento.CurtoPrazo => 5,
                HorizonteInvestimento.MedioPrazo => 10,
                HorizonteInvestimento.LongoPrazo => 15,
                _ => 5
            };

            pontos += dadosFinanceiros.ExperienciaInvestimentos ? 10 : 0;
        }

        Pontuacao = pontos;

        if (pontos <= 38)
        {
            Perfil = PerfilInvestidor.Conservador;
            Descricao = "Perfil conservador: prioriza segurança e liquidez, baixa tolerância a risco";
        }
        else if (pontos <= 47)
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

    public void AtualizarPerfil(PerfilFinanceiro? dadosFinanceiros)
    {
        CalcularPerfil(dadosFinanceiros);
    }


}
