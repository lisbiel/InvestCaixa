using InvestCaixa.Application.DTOs.Response;
using InvestCaixa.Application.Handlers.Queries;
using InvestCaixa.Domain.Enums;
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
            RentabilidadeReal = i.ValorAplicado > 0 ? CalcularRentabilidadeReal(i.ValorAplicado, i.ValorResgatado, i.DataAplicacao, i.Produto.Rentabilidade, i.Status) : 0,
        });

        var totalInvestido = invResponses.Sum(i => i.ValorAplicado);
        var totalResgatado = invResponses.Sum(i => i.ValorResgatado);
        var totalRentabilidade = invResponses.Sum(i => i.RentabilidadeReal);

        return new HistoricoCompletoResponse
        {
            Simulacoes = simResponses,
            Investimentos = invResponses,
            TotalInvestido = totalInvestido,
            TotalResgatado = totalResgatado,
            RentabilidadeReal = totalRentabilidade,
            TotalOperacoes = simResponses.Count() + invResponses.Count()
        };
    }

    private decimal CalcularRentabilidadeReal(decimal valorAplicado, decimal valorResgatado, DateTime dataAplicacao, decimal rentabilidade, StatusInvestimento status)
    {
        if (status == StatusInvestimento.Cancelado)
            return 0m;
        else if (status == StatusInvestimento.Resgatado)
            return valorAplicado > 0 ? (valorAplicado - valorResgatado) / valorAplicado : 0;
        else if (status == StatusInvestimento.Ativo)
        {

            // Calcular diferença de dias
            DateTime hoje = DateTime.Today;
            TimeSpan diferenca = hoje - dataAplicacao;
            int totalDias = (int)diferenca.TotalDays;

            // Número de períodos por ano (365 para diário)
            int periodosAno = 365;

            // Calcular anos
            double anos = totalDias / 365.0;

            // Fórmula: A = P * (1 + r/n)^(n*t)
            decimal montanteFinal = valorAplicado *
                (decimal)Math.Pow(
                    (double)(1 + rentabilidade / periodosAno),
                    periodosAno * anos);

            // Rendimento = Montante Final - Principal
            decimal rentabilidadeReal = ((montanteFinal - valorAplicado) / valorAplicado);

            return rentabilidadeReal;
        }
        return 0m;
    }
}
