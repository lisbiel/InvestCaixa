using MediatR;

namespace InvestCaixa.Application.Handlers.Commands;

public class FinalizarInvestimentoCommand : IRequest<Guid>
{
    public int ClienteId { get; set; }
    public Guid ProdutoId { get; set; }
    public decimal ValorAplicado { get; set; }
    public int PrazoMeses { get; set; }
    public FinalizarInvestimentoCommand(int clienteId, Guid produtoId, decimal valor, int prazoMeses)
    {
        ClienteId = clienteId;
        ProdutoId = produtoId;
        ValorAplicado = valor;
        PrazoMeses = prazoMeses;
    }
}
