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
            perfilRisco = await CalcularPerfilCompleto(clienteId, cancellationToken);
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

    /* Calculo inicial baseado apenas em simulações realizadas
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
    }*/

    public async Task AtualizarPerfilAsync(
        int clientId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var perfilAtualizado = await CalcularPerfilCompleto(clientId, cancellationToken);

            var perfilExistente = await _unitOfWork.ClienteRepository
                .ObterPerfilRiscoAsync(clientId, cancellationToken);

            if (perfilExistente != null)
            {
                perfilExistente.AtualizarDados(
                    perfilAtualizado.VolumeInvestimentos,
                    perfilAtualizado.FrequenciaMovimentacoes,
                    perfilAtualizado.PrefereLiquidez);

                _logger.LogInformation("Perfil atualizado para cliente {ClienteId} - Perfil: {PerfilAntigo} -> {PerfilNovo}, Pontuacao - {PontuacaoAntiga} -> {PontuacaoNova}"
                    , clientId,
                    perfilExistente.Perfil,
                    perfilAtualizado.Perfil,
                    perfilExistente.Pontuacao,
                    perfilAtualizado.Pontuacao);
            }
            else
            {
                await _unitOfWork.ClienteRepository
                    .AdicionarPerfilRiscoAsync(perfilAtualizado, cancellationToken);
                _logger.LogInformation("Perfil criado para cliente {ClienteId} - Perfil: {Perfil}, Pontuacao - {Pontuacao}"
                    , clientId,
                    perfilAtualizado.Perfil,
                    perfilAtualizado.Pontuacao);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar perfil de risco para cliente {ClienteId}", clientId);
            throw;
        }
    }

    private async Task<PerfilRisco> CalcularPerfilCompleto(
        int clienteId,
        CancellationToken cancellationToken)
    {
        var simulacoes = await _unitOfWork.SimulacaoRepository
            .ObterPorClienteAsync(clienteId, cancellationToken);

        var investimentosFinalizados = await _unitOfWork.InvestimentoFinalizadoRepository
            .ObterPorClienteAsync(clienteId, cancellationToken);

        var perfilFinanceiro = await _unitOfWork.PerfilFinanceiroRepository
            .ObterPorClienteAsync(clienteId, cancellationToken);

        var volumeSimulacao = simulacoes.Sum(s => s.ValorInvestido);
        var volumeInvestimentosFinalizados = investimentosFinalizados.Sum(i => i.ValorAplicado);
        var volumeTotal = volumeSimulacao + volumeInvestimentosFinalizados;

        //Último ano para pegar um panorama mais atual
        var dataCorte = DateTime.UtcNow.AddMonths(-12);
        var simulacaoRecente = simulacoes.Count(s => s.DataSimulacao >= dataCorte);
        var investimentoFinalizadoRecente = investimentosFinalizados.Count(i => i.DataAplicacao >= dataCorte);
        var frequenciaRecente = simulacaoRecente + (investimentoFinalizadoRecente * 2); //Vamos dar peso a quem investe mais do que a quem simula

        var prefereLiquidez = CalcularPreferenciaLiquidez(simulacoes, investimentosFinalizados);

        var perfilRisco = AnaliseRiscoCOmportamental(simulacoes, investimentosFinalizados);

        var perfil = new PerfilRisco(
            clienteId,
            volumeTotal,
            frequenciaRecente,
            prefereLiquidez,
            perfilFinanceiro);

        return perfil;
    }

    private object AnaliseRiscoCOmportamental(IEnumerable<Simulacao> simulacoes, IEnumerable<InvestimentoFinalizado> investimentosFinalizados)
    {
        var pontoRisco = 0;

        pontoRisco += simulacoes.Count(s => s.Produto.Risco == NivelRisco.Baixo) * 1;
        pontoRisco += simulacoes.Count(s => s.Produto.Risco == NivelRisco.Medio) * 2;
        pontoRisco += simulacoes.Count(s => s.Produto.Risco == NivelRisco.Alto) * 3;

        //Peso maior para investimentos reais sem ignorar as simulações
        pontoRisco += investimentosFinalizados.Count(i => i.Produto.Risco == NivelRisco.Baixo) * 2;
        pontoRisco += investimentosFinalizados.Count(i => i.Produto.Risco == NivelRisco.Medio) * 4;
        pontoRisco += investimentosFinalizados.Count(i => i.Produto.Risco == NivelRisco.Alto) * 6;
        
        var tipoSimulados = simulacoes
            .Select(s => s.Produto.Tipo)
            .Distinct()
            .Count();

        var tiposInvestidos = investimentosFinalizados
            .Select(i => i.Produto.Tipo)
            .Distinct()
            .Count();
        pontoRisco += (tipoSimulados + tiposInvestidos) * 2;

        var totalOperacoes = simulacoes.Count() + investimentosFinalizados.Count();
        if (totalOperacoes == 0) return PerfilInvestidor.Conservador;

        var mediaRisco = pontoRisco / totalOperacoes;

        return mediaRisco switch
        {
            <= 2 => PerfilInvestidor.Conservador,
            > 2 and <= 4 => PerfilInvestidor.Moderado,
            _ => PerfilInvestidor.Agressivo,
        };
    }

    private static bool CalcularPreferenciaLiquidez(IEnumerable<Simulacao> simulacoes, IEnumerable<InvestimentoFinalizado> investimentosFinalizados)
    {
        var totalOperacoes = simulacoes.Count() + investimentosFinalizados.Count();
        if (totalOperacoes == 0)
            return true;

        var simulacoesLiquidez = simulacoes
            .Select(s => s.Produto)
            .Count(p => p.PermiteLiquidez);

        var investimentosLiquidez = investimentosFinalizados
            .Select(i => i.Produto)
            .Count(p => p.PermiteLiquidez);

        var simulacoesCurtoPrazo = simulacoes
            .Count(s => s.PrazoMeses <= 12);

        var investimentosCurtoPrazo = investimentosFinalizados
            .Count(i => (i.DataResgate.HasValue && (i.DataResgate.Value - i.DataAplicacao).TotalDays <= 365)
            || i.PrazoMeses <= 12);

        //Mantendo multplicação por 1 para controle de pesos facilitado em v3
        var pontuacaoLiquidez = (simulacoesLiquidez * 1) + (investimentosLiquidez * 3) 
            + (simulacoesCurtoPrazo * 0.5) + (investimentosCurtoPrazo * 1);

        var pontuacaoMaxima = (simulacoes.Count() * 1.5) + (investimentosFinalizados.Count() * 3);

        return pontuacaoMaxima > 0 && (pontuacaoLiquidez / pontuacaoMaxima) >= 0.55;
    }
}
