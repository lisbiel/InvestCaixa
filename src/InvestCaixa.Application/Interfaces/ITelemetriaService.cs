using InvestCaixa.Application.DTOs.Response;

namespace InvestCaixa.Application.Interfaces;

public interface ITelemetriaService
{
    void RegistrarChamada(string nomeServico, long tempoRespostaMs);
    void RegistrarChamadaAvancada(string nomeServico, long tempoRespostaMs, bool sucesso = true, string? tipoErro = null, Dictionary<string, object>? contexto = null);
    Task<TelemetriaResponse> ObterTelemetriaAsync(DateTime? dataInicio = null, DateTime? dataFim = null, CancellationToken cancellationToken = default);
}
