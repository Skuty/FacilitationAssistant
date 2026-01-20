namespace FacilitationAssistant.Core.Domain.Common;

/// <summary>
/// Base class for aggregate roots in the domain model.
/// </summary>
public abstract class AggregateRoot
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;

    protected void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
