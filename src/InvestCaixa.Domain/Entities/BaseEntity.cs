using InvestCaixa.Domain.Primitives;
using MediatR;
using System.Collections.ObjectModel;

namespace InvestCaixa.Domain.Entities;

/// <summary>
/// Entidade base genérica que aceita qualquer tipo de chave (Guid, int, long, etc)
/// </summary>
public abstract class BaseEntity<TKey> where TKey : notnull
{
    public TKey Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public bool IsDeleted { get; protected set; } = false;

    // Coleção de eventos de domínio
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => new ReadOnlyCollection<IDomainEvent>(_domainEvents);

    protected BaseEntity() { }

    protected BaseEntity(TKey id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent is null) return;
        _domainEvents.Add(domainEvent);
    }

    protected void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent is null) return;
        _domainEvents.Remove(domainEvent);
    }

    protected void ClearDomainEvents() => _domainEvents.Clear();
}

/// <summary>
/// Entidade base padrão com Guid (para retrocompatibilidade)
/// </summary>
public abstract class BaseEntity : BaseEntity<Guid>
{
    protected BaseEntity() : base(Guid.NewGuid()) { }
}