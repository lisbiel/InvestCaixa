using InvestCaixa.Domain.Events;
using MediatR;
namespace InvestCaixa.Domain.Entities;

public class InvestimentoFinalizado : BaseEntity
{
    public int ClienteId { get; private set; }
    public Guid ProdutoId { get; private set; }
    public decimal ValorAplicado { get; private set; }
    public decimal ValorResgatado { get; private set; }
    public DateTime DataAplicacao { get; private set; }
    public DateTime? DataResgate { get; private set; }
    public StatusInvestimento Status { get; private set; }
    public int PrazoMeses { get; private set; }
    public virtual Cliente Cliente { get; private set; } = null!;
    public virtual ProdutoInvestimento Produto { get; private set; } = null!;

    private InvestimentoFinalizado() { }

    public InvestimentoFinalizado(int clienteId, Guid produtoId, decimal valorAplicado, DateTime dataAplicacao)
    {
        if (valorAplicado <= 0) throw new DomainException("Valor aplicado deve ser positivo");
        ClienteId = clienteId;
        ProdutoId = produtoId;
        ValorAplicado = valorAplicado;
        DataAplicacao = dataAplicacao;
        Status = StatusInvestimento.Ativo;
        ValorResgatado = 0;

        RaiseDomainEvent(new InvestimentoFInalizadoEvent(
            Id, ClienteId, ProdutoId, ValorAplicado, DataAplicacao));
    }

    public InvestimentoFinalizado(int clienteId, Guid produtoId, decimal valorAplicado, DateTime dataAplicacao, int prazoMeses)
    {
        if (valorAplicado <= 0) throw new DomainException("Valor aplicado deve ser positivo");
        ClienteId = clienteId;
        ProdutoId = produtoId;
        ValorAplicado = valorAplicado;
        DataAplicacao = dataAplicacao;
        Status = StatusInvestimento.Ativo;
        ValorResgatado = 0;
        PrazoMeses = prazoMeses;

        RaiseDomainEvent(new InvestimentoFInalizadoEvent(
            Id, ClienteId, ProdutoId, ValorAplicado, DataAplicacao));
    }

    public void Resgatar(decimal valorResgatado)
    {
        if (Status != StatusInvestimento.Ativo) throw new DomainException("Investimento não está ativo");
        if (valorResgatado < 0) throw new DomainException("Valor resgatado não pode ser negativo");
        Status = StatusInvestimento.Resgatado;
        ValorResgatado = valorResgatado;
        DataResgate = DateTime.UtcNow;
    }
}
