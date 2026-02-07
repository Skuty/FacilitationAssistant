using FacilitationAssistant.Core.Entities;
using MediatR;

namespace FacilitationAssistant.Core.Queries;

public record GetQuestionByIdQuery(Guid QuestionId) : IRequest<Question?>;
