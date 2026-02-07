using FacilitationAssistant.Core.Entities;
using MediatR;

namespace FacilitationAssistant.Core.Queries;

public record GetMeetingByTokenQuery(string Token, bool IsFacilitator) : IRequest<Meeting?>;
