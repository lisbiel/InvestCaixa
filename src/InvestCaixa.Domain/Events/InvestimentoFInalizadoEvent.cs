using InvestCaixa.Domain.Primitives;
using System.Diagnostics.CodeAnalysis;

namespace InvestCaixa.Domain.Events;

[ExcludeFromCodeCoverage] //Apenas event, sem lógica de negócio
public sealed record InvestimentoFInalizadoEvent(
    Guid InvestimentoId,
    int ClienteId,
    Guid ProdutoId,
    decimal valorAplicado,
    DateTime DataInvestimento) : IDomainEvent;
