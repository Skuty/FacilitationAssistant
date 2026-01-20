namespace FacilitationAssistant.Core.Interfaces;

/// <summary>
/// Provides current date and time. Abstraction for testability.
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
