using FacilitationAssistant.Core.Entities;
using Mediator;

namespace FacilitationAssistant.Core.Queries;

public record GetQuestionByIdQuery(Guid QuestionId) : IRequest<Question?>;
