namespace InvestCaixa.Application.Handlers.Queries;

using InvestCaixa.Application.DTOs.Response;
using MediatR;

public class ObterPerfilRiscoQuery : IRequest<PerfilRiscoResponse>
{
    public int ClienteId { get; set; }

    public ObterPerfilRiscoQuery(int clienteId) => ClienteId = clienteId;
}
