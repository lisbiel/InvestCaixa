using InvestCaixa.Application.DTOs.Response;
using MediatR;

namespace InvestCaixa.Application.Handlers.Queries;

public class ObterHistoricoCompletoQuery : IRequest<HistoricoCompletoResponse>
{
    public int ClienteId { get; set; }
    public ObterHistoricoCompletoQuery(int clienteId) => ClienteId = clienteId;
}