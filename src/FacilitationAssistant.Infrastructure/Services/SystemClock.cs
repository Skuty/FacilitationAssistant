using FacilitationAssistant.Core.Interfaces;

namespace FacilitationAssistant.Infrastructure.Services;

/// <summary>
/// System clock implementation providing UTC time.
/// </summary>
public class SystemClock : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
