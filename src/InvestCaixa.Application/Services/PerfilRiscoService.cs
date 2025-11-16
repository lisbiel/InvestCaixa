namespace InvestCaixa.Application.Services;

using AutoMapper;
using InvestCaixa.Application.DTOs.Response;
using InvestCaixa.Application.Interfaces;
using InvestCaixa.Domain.Entities;
using InvestCaixa.Domain.Enums;
using InvestCaixa.Domain.Exceptions;
using InvestCaixa.Domain.Interfaces;
using Microsoft.Extensions.Logging;

public class PerfilRiscoService : IPerfilRiscoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PerfilRiscoService> _logger;

    public PerfilRiscoService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<PerfilRiscoService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PerfilRiscoResponse> ObterPerfilRiscoAsync(
        int clienteId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obtendo perfil de risco para cliente {ClienteId}", clienteId);

        var cliente = await _unitOfWork.ClienteRepository
            .GetByIdAsync(clienteId, cancellationToken);

        if (cliente == null)
            throw new NotFoundException($"Cliente {clienteId} não encontrado");

        var perfilRisco = await _unitOfWork.ClienteRepository
            .ObterPerfilRiscoAsync(clienteId, cancellationToken);

        if (perfilRisco == null)
        {
            perfilRisco = await CalcularPerfilInicialAsync(clienteId, cancellationToken);
        }

        return _mapper.Map<PerfilRiscoResponse>(perfilRisco);
    }

    public async Task<IEnumerable<ProdutoResponse>> ObterProdutosRecomendadosAsync(
        PerfilInvestidor perfil,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Buscando produtos recomendados para perfil {Perfil}", perfil);

        var produtos = await _unitOfWork.ProdutoRepository
            .ObterPorPerfilAsync(perfil, cancellationToken);

        return _mapper.Map<IEnumerable<ProdutoResponse>>(produtos);
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
            .Count(p => p.PermiteLiquidez) > (simulacoes.Count() / 2);

        var perfilRisco = new PerfilRisco(
            clienteId,
            volumeTotal,
            frequencia,
            prefereLiquidez);

        await _unitOfWork.ClienteRepository
            .AdicionarPerfilRiscoAsync(perfilRisco, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return perfilRisco;
    }
}
