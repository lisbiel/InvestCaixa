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
}

/// <summary>
/// Entidade base padrão com Guid (para retrocompatibilidade)
/// </summary>
public abstract class BaseEntity : BaseEntity<Guid>
{
    protected BaseEntity() : base(Guid.NewGuid()) { }
}