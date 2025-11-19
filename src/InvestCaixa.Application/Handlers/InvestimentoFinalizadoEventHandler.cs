using InvestCaixa.Application.Interfaces;
using InvestCaixa.Domain.Events;
using InvestCaixa.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InvestCaixa.Application.Handlers;

public class InvestimentoFinalizadoEventHandler : INotificationHandler<InvestimentoFInalizadoEvent>
{
    private readonly IPerfilRiscoService _perfilRiscoService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InvestimentoFinalizadoEventHandler> _logger;

    public InvestimentoFinalizadoEventHandler(
        IPerfilRiscoService perfilRiscoService,
        IUnitOfWork unitOfWork,
        ILogger<InvestimentoFinalizadoEventHandler> logger)
    {
        _perfilRiscoService = perfilRiscoService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(InvestimentoFInalizadoEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Atualizando perfil de risco para o cliente {ClienteId} após investimento finalizado em {DataHora}"
            , notification.ClienteId, notification.DataInvestimento.ToString("dd/MM/yyyy HH:mm"));
        
        try
        {
            var perfilRisco = await _unitOfWork.ClienteRepository
                .ObterPerfilRiscoAsync(notification.ClienteId, cancellationToken);
            if (perfilRisco != null)
            {
                await _perfilRiscoService.AtualizarPerfilAsync(notification.ClienteId, cancellationToken);
                _logger.LogInformation("Perfil de risco atualizado com sucesso para o cliente {ClienteId}", notification.ClienteId);
            }
            else
            {
                // Caso o perfil de risco não exista, apenas o obtém para garantir que está criado
                _ = await _perfilRiscoService.ObterPerfilRiscoAsync(notification.ClienteId, cancellationToken);
                _logger.LogInformation("Perfil de risco criado para o cliente {ClienteId}", notification.ClienteId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar/criar perfil de risco para o cliente {ClienteId}", notification.ClienteId);
            //Sem propagação -- Aguardando DLQ para tratar erros pontuais
        }
    }
}
