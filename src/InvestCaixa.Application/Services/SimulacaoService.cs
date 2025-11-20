namespace InvestCaixa.Application.Services;

using AutoMapper;
using InvestCaixa.Application.DTOs.Request;
using InvestCaixa.Application.DTOs.Response;
using InvestCaixa.Application.Interfaces;
using InvestCaixa.Domain.Entities;
using InvestCaixa.Domain.Enums;
using InvestCaixa.Domain.Exceptions;
using InvestCaixa.Domain.Interfaces;
using InvestCaixa.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using InvestCaixa.Application.Extensions;

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

        var adequacaoPerfil = await CalcularAdequacaoBase(produto.Risco, request.ClienteId);

        var disclaimer = GerarDisclaimer(produto.Tipo);

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
            DataSimulacao = DateTime.UtcNow,
            Disclaimer = disclaimer,
            AdequacaoPerfil = adequacaoPerfil.GetDescription()
        };
    }

    private static DisclaimerRegulatorio GerarDisclaimer(TipoProduto tipo)
    {
        return tipo switch
        {
            TipoProduto.LCI or TipoProduto.LCA or TipoProduto.TesouroDireto => 
                DisclaimerRegulatorio.ParaRendaFixa(),
            TipoProduto.CDB =>
                DisclaimerRegulatorio.ParaCDB(),
            TipoProduto.Fundo => 
                DisclaimerRegulatorio.ParaRendaVariavel(),
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

    private async Task<AdequacaoPerfil> CalcularAdequacaoBase(NivelRisco riscoProduto, int clienteId)
    {
        var perfil = await _unitOfWork.ClienteRepository.ObterPerfilRiscoAsync(clienteId);
        if (perfil == null)
            return AdequacaoPerfil.NaoAvaliado;

        return (riscoProduto, perfil.Perfil) switch
        {
            (NivelRisco.Baixo, PerfilInvestidor.Conservador) => AdequacaoPerfil.Adequado,
            (NivelRisco.Baixo, PerfilInvestidor.Moderado) => AdequacaoPerfil.InadequadoBaixoRisco,
            (NivelRisco.Baixo, PerfilInvestidor.Agressivo) => AdequacaoPerfil.InadequadoBaixoRisco,

            (NivelRisco.Medio, PerfilInvestidor.Conservador) => AdequacaoPerfil.InadequadoAltoRisco,
            (NivelRisco.Medio, PerfilInvestidor.Moderado) => AdequacaoPerfil.Adequado,
            (NivelRisco.Medio, PerfilInvestidor.Agressivo) => AdequacaoPerfil.Adequado,

            (NivelRisco.Alto, PerfilInvestidor.Conservador) => AdequacaoPerfil.InadequadoAltoRisco,
            (NivelRisco.Alto, PerfilInvestidor.Moderado) => AdequacaoPerfil.InadequadoAltoRisco,
            (NivelRisco.Alto, PerfilInvestidor.Agressivo) => AdequacaoPerfil.Adequado,
            _ => AdequacaoPerfil.NaoAvaliado
        };
    }

    public Task<IEnumerable<ProdutoResponse>> ObterProdutosDisponiveisAsync(CancellationToken cancellationToken = default)
    {
        var produtos = _unitOfWork.ProdutoRepository
            .GetAllAsync(cancellationToken);

        return produtos.ContinueWith(t => _mapper.Map<IEnumerable<ProdutoResponse>>(t.Result), cancellationToken);
    }
}
