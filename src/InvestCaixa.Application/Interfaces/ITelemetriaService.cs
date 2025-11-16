using InvestCaixa.Application.DTOs.Response;

namespace InvestCaixa.Application.Interfaces;

public interface ITelemetriaService
{
    void RegistrarChamada(string nomeServico, long tempoRespostaMs);
    Task<TelemetriaResponse> ObterTelemetriaAsync(DateTime? dataInicio = null, DateTime? dataFim = null, CancellationToken cancellationToken = default);
}
