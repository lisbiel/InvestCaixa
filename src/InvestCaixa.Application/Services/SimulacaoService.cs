namespace InvestCaixa.Application.Services;

using AutoMapper;
using InvestCaixa.Application.DTOs.Request;
using InvestCaixa.Application.DTOs.Response;
using InvestCaixa.Application.Interfaces;
using InvestCaixa.Domain.Entities;
using InvestCaixa.Domain.Exceptions;
using InvestCaixa.Domain.Interfaces;
using Microsoft.Extensions.Logging;

public class SimulacaoService : ISimulacaoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<SimulacaoService> _logger;

    public SimulacaoService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<SimulacaoService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SimulacaoResponse> SimularInvestimentoAsync(
        SimularInvestimentoRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando simulação para cliente {ClienteId} com valor {Valor}", 
            request.ClienteId, request.Valor);

        var produto = await _unitOfWork.ProdutoRepository
            .ObterPorTipoAsync(request.TipoProduto, cancellationToken);

        if (produto == null)
            throw new NotFoundException($"Produto do tipo {request.TipoProduto} não encontrado");

        if (request.Valor < produto.ValorMinimoAplicacao)
            throw new ValidationException(
                $"Valor mínimo para este produto é {produto.ValorMinimoAplicacao:C}");

        var meses = request.PrazoMeses;
        var taxaMensal = produto.Rentabilidade / 12;
        var valorFinal = request.Valor * (decimal)Math.Pow((double)(1 + taxaMensal), meses);
        var rentabilidadeEfetiva = (valorFinal - request.Valor) / request.Valor;

        var simulacao = new Simulacao(
            request.ClienteId,
            produto.Nome,
            produto.Id,
            request.Valor,
            valorFinal,
            meses,
            DateTime.UtcNow);

        await _unitOfWork.SimulacaoRepository.AddAsync(simulacao, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Simulação concluída. Valor inicial: {ValorInicial}, Valor final: {ValorFinal}",
            request.Valor, valorFinal);

        return new SimulacaoResponse
        {
            Id = simulacao.Id,
            ProdutoValidado = _mapper.Map<ProdutoValidadoDto>(produto),
            ResultadoSimulacao = new ResultadoSimulacaoDto
            {
                ValorFinal = valorFinal,
                RentabilidadeEfetiva = rentabilidadeEfetiva,
                PrazoMeses = meses
            },
            DataSimulacao = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<SimulacaoHistoricoResponse>> ObterSimulacoesAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obtendo histórico de todas as simulações");

        var simulacoes = await _unitOfWork.SimulacaoRepository
            .GetAllAsync(cancellationToken);

        return _mapper.Map<IEnumerable<SimulacaoHistoricoResponse>>(simulacoes);
    }

    public async Task<IEnumerable<SimulacaoPorProdutoDiaResponse>> ObterSimulacoesPorProdutoDiaAsync(
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obtendo simulações por produto e dia. Período: {DataInicio} a {DataFim}",
            dataInicio ?? DateTime.UtcNow.AddDays(-30), dataFim ?? DateTime.UtcNow);

        var resultado = await _unitOfWork.SimulacaoRepository
            .ObterSimulacoesPorProdutoDiaAsync(dataInicio, dataFim, cancellationToken);

        return resultado.Select(r => new SimulacaoPorProdutoDiaResponse
        {
            Produto = r.Produto,
            Data = r.Data,
            QuantidadeSimulacoes = r.QuantidadeSimulacoes,
            MediaValorFinal = r.MediaValorFinal
        });
    }
}
