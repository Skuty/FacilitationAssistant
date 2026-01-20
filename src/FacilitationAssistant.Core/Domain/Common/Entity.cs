namespace FacilitationAssistant.Core.Domain.Common;

/// <summary>
/// Base class for entities that are not aggregate roots.
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
}
