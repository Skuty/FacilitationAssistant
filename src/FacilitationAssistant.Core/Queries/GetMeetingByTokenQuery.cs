using FacilitationAssistant.Core.Entities;
using Mediator;

namespace FacilitationAssistant.Core.Queries;

public record GetMeetingByTokenQuery(string Token, bool IsFacilitator) : IRequest<Meeting?>;
