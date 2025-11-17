using InvestCaixa.Application.DTOs.Response;
using InvestCaixa.Application.Handlers.Queries;
using InvestCaixa.Domain.Interfaces;
using MediatR;

namespace InvestCaixa.Application.Handlers;
public class ObterHistoricoCompletoQueryHandler : IRequestHandler<ObterHistoricoCompletoQuery, HistoricoCompletoResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public ObterHistoricoCompletoQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<HistoricoCompletoResponse> Handle(ObterHistoricoCompletoQuery request, CancellationToken cancellationToken)
    {
        var simulacoes = await _unitOfWork.SimulacaoRepository.ObterPorClienteAsync(request.ClienteId, cancellationToken);
        var investimentos = await _unitOfWork.InvestimentoFinalizadoRepository.ObterPorClienteAsync(request.ClienteId, cancellationToken);

        var simResponses = simulacoes.Select(s => new SimulacaoHistoricoResponse
        {
            Id = s.Id,
            ClienteId = s.ClienteId,
            ValorInvestido = s.ValorInvestido,
            ValorFinal = s.ValorFinal,
            Rentabilidade = s.RentabilidadeCalculada,
            DataSimulacao = s.DataSimulacao
        });

        var invResponses = investimentos.Select(i => new InvestimentoFinalizadoResponse
        {
            Id = i.Id,
            ClienteId = i.ClienteId,
            ProdutoId = i.ProdutoId,
            ProdutoNome = i.Produto.Nome,
            ValorAplicado = i.ValorAplicado,
            ValorResgatado = i.ValorResgatado,
            Status = (int)i.Status,
            DataAplicacao = i.DataAplicacao,
            DataResgate = i.DataResgate,
            RentabilidadeReal = i.ValorAplicado > 0 ? (i.ValorResgatado - i.ValorAplicado) / i.ValorAplicado : 0
        });

        var totalInvestido = investimentos.Sum(i => i.ValorAplicado);
        var totalResgatado = investimentos.Sum(i => i.ValorResgatado);

        return new HistoricoCompletoResponse
        {
            Simulacoes = simResponses,
            Investimentos = invResponses,
            TotalInvestido = totalInvestido,
            TotalResgatado = totalResgatado,
            RentabilidadeReal = totalInvestido > 0 ? (totalResgatado - totalInvestido) / totalInvestido : 0,
            TotalOperacoes = simResponses.Count() + invResponses.Count()
        };
    }
}
