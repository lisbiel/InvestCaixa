namespace InvestCaixa.Application.Handlers;

using AutoMapper;
using InvestCaixa.Application.DTOs.Response;
using InvestCaixa.Application.Handlers.Queries;
using InvestCaixa.Domain.Entities;
using InvestCaixa.Domain.Enums;
using InvestCaixa.Domain.Exceptions;
using InvestCaixa.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

public class ObterPerfilRiscoQueryHandler : IRequestHandler<ObterPerfilRiscoQuery, PerfilRiscoResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ObterPerfilRiscoQueryHandler> _logger;

    public ObterPerfilRiscoQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ObterPerfilRiscoQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PerfilRiscoResponse> Handle(
        ObterPerfilRiscoQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Obtendo perfil de risco para cliente {ClienteId}", request.ClienteId);

        var cliente = await _unitOfWork.ClienteRepository
            .GetByIdAsync(request.ClienteId, cancellationToken);

        if (cliente == null)
            throw new NotFoundException($"Cliente {request.ClienteId} não encontrado");

        var perfilRisco = await _unitOfWork.ClienteRepository
            .ObterPerfilRiscoAsync(request.ClienteId, cancellationToken);

        if (perfilRisco == null)
        {
            perfilRisco = await CalcularPerfilInicialAsync(request.ClienteId, cancellationToken);
        } else 
        {
            var perfilFinanceiro = await _unitOfWork.PerfilFinanceiroRepository
                .ObterPorClienteAsync(request.ClienteId, cancellationToken);

            if (perfilFinanceiro != null &&
                perfilFinanceiro.UpdatedAt > perfilRisco.UpdatedAt)
            {
                var novoPerfilRisco = new PerfilRisco(
                    request.ClienteId,
                    perfilRisco.VolumeInvestimentos,
                    perfilRisco.FrequenciaMovimentacoes,
                    perfilRisco.PrefereLiquidez,
                    perfilFinanceiro);
                if(novoPerfilRisco != perfilRisco)
                {
                    perfilRisco.AtualizarPerfil(perfilFinanceiro);
                    await _unitOfWork.ClienteRepository.AtualizarPerfilRiscoAsync(perfilRisco, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Perfil de risco atualizado para cliente {ClienteId}", request.ClienteId);
                }
            }
        }

        return _mapper.Map<PerfilRiscoResponse>(perfilRisco);
    }

    private async Task<PerfilRisco> CalcularPerfilInicialAsync(
        int clienteId,
        CancellationToken cancellationToken)
    {
        var simulacoes = await _unitOfWork.SimulacaoRepository
            .ObterPorClienteAsync(clienteId, cancellationToken);

        var volumeTotal = simulacoes.Sum(s => s.ValorInvestido);
        var frequencia = simulacoes.Count();
        var prefereLiquidez = simulacoes
            .Select(s => s.Produto)
            .Count(p => p.PermiteLiquidez) > (simulacoes.Count() / 2.0);

        // **BUSCAR DADOS FINANCEIROS SE EXISTIREM**
        var dadosFinanceiros = await _unitOfWork.PerfilFinanceiroRepository
            .ObterPorClienteAsync(clienteId, cancellationToken);

        var perfilRisco = new PerfilRisco(
            clienteId,
            volumeTotal,
            frequencia,
            prefereLiquidez,
            dadosFinanceiros);

        await _unitOfWork.ClienteRepository
            .AdicionarPerfilRiscoAsync(perfilRisco, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return perfilRisco;
    }
}