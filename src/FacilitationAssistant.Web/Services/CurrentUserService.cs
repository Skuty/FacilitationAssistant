using FacilitationAssistant.Core.Interfaces;

namespace FacilitationAssistant.Web.Services;

/// <summary>
/// Provides current user context from HTTP context.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string SessionId => _httpContextAccessor.HttpContext?.Connection.Id ?? "unknown";

    public bool IsFacilitator =>
        _httpContextAccessor.HttpContext?.Request.Query.ContainsKey("facilitatorKey") ?? false;

    public Guid? FacilitatorKey
    {
        get
        {
            var keyString = _httpContextAccessor.HttpContext?.Request.Query["facilitatorKey"].FirstOrDefault();
            return Guid.TryParse(keyString, out var key) ? key : null;
        }
    }

    public string DisplayName =>
        _httpContextAccessor.HttpContext?.Request.Query["displayName"].FirstOrDefault() ?? "Anonymous";
}
