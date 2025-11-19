using InvestCaixa.Domain.Primitives;

namespace InvestCaixa.Domain.Events;

public sealed record InvestimentoFInalizadoEvent(
    Guid InvestimentoId,
    int ClienteId,
    Guid ProdutoId,
    decimal valorAplicado,
    DateTime DataInvestimento) : IDomainEvent;
