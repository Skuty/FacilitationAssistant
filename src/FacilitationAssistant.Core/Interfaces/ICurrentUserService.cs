namespace FacilitationAssistant.Core.Interfaces;

/// <summary>
/// Provides information about the current user context.
/// </summary>
public interface ICurrentUserService
{
    string SessionId { get; }
    bool IsFacilitator { get; }
    Guid? FacilitatorKey { get; }
    string DisplayName { get; }
}
