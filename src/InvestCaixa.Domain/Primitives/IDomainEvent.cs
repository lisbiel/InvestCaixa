using MediatR;
namespace InvestCaixa.Domain.Primitives;

//Preparando para event-driven em v2
public interface IDomainEvent : INotification
{
}
