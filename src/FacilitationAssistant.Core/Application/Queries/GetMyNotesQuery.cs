using FacilitationAssistant.Core.Application.DTOs;
using MediatR;

namespace FacilitationAssistant.Core.Application.Queries;

/// <summary>
/// Query to get private notes for a specific owner (facilitator or attendee).
/// </summary>
public record GetMyNotesQuery(Guid MeetingId, string OwnerId) : IRequest<List<NoteDto>>;
