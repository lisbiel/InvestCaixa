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
        bool prefereLiquidez)
    {
        ClienteId = clienteId;
        VolumeInvestimentos = volumeInvestimentos;
        FrequenciaMovimentacoes = frequenciaMovimentacoes;
        PrefereLiquidez = prefereLiquidez;

        CalcularPerfil();
    }

    public void AtualizarDados(
        decimal volumeInvestimentos,
        int frequenciaMovimentacoes,
        bool prefereLiquidez)
    {
        VolumeInvestimentos = volumeInvestimentos;
        FrequenciaMovimentacoes = frequenciaMovimentacoes;
        PrefereLiquidez = prefereLiquidez;
        CalcularPerfil();
        UpdatedAt = DateTime.UtcNow;
    }

    private void CalcularPerfil()
    {
        int pontos = 0;

        // Pontuação por volume (0-40 pontos)
        if (VolumeInvestimentos < 10000) pontos += 10;
        else if (VolumeInvestimentos < 50000) pontos += 20;
        else if (VolumeInvestimentos < 100000) pontos += 30;
        else pontos += 40;

        // Pontuação por frequência (0-30 pontos)
        if (FrequenciaMovimentacoes < 3) pontos += 10;
        else if (FrequenciaMovimentacoes < 10) pontos += 20;
        else pontos += 30;

        // Pontuação por preferência (0-30 pontos)
        pontos += PrefereLiquidez ? 10 : 30;

        //Cliente novo, deve jogar sua pontuação mais baixa
        if (VolumeInvestimentos == 0 && FrequenciaMovimentacoes == 0)
            pontos -= 20;

        Pontuacao = pontos;

        // Definir perfil baseado na pontuação
        if (pontos <= 40)
        {
            Perfil = PerfilInvestidor.Conservador;
            Descricao = "Perfil conservador: baixa movimentação, foco em segurança e liquidez";
        }
        else if (pontos <= 70)
        {
            Perfil = PerfilInvestidor.Moderado;
            Descricao = "Perfil moderado: equilíbrio entre segurança e rentabilidade";
        }
        else
        {
            Perfil = PerfilInvestidor.Agressivo;
            Descricao = "Perfil agressivo: busca por alta rentabilidade, aceita maior risco";
        }
    }
}
