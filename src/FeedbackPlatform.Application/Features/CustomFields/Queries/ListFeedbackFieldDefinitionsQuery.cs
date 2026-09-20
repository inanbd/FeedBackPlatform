using FeedbackPlatform.Application.Common;
using FeedbackPlatform.Application.Common.Exceptions;
using FeedbackPlatform.Application.Common.Interfaces;
using FeedbackPlatform.Domain.Entities;
using FeedbackPlatform.Domain.Enums;
using MediatR;

namespace FeedbackPlatform.Application.Features.CustomFields.Queries;

public sealed record FeedbackFieldDefinitionDto(
    Guid Id, string FieldKey, string Label, FeedbackFieldType FieldType, bool IsRequired, int DisplayOrder, string? OptionsCsv);

public sealed record ListFeedbackFieldDefinitionsQuery(Guid FeedbackAppId) : IRequest<IReadOnlyList<FeedbackFieldDefinitionDto>>;

public sealed class ListFeedbackFieldDefinitionsQueryHandler(
    IFeedbackFieldDefinitionRepository fieldDefinitionRepository,
    IFeedbackAppRepository feedbackAppRepository,
    ICurrentUserService currentUser)
    : IRequestHandler<ListFeedbackFieldDefinitionsQuery, IReadOnlyList<FeedbackFieldDefinitionDto>>
{
    public async Task<IReadOnlyList<FeedbackFieldDefinitionDto>> Handle(
        ListFeedbackFieldDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var app = await feedbackAppRepository.GetByIdAsync(request.FeedbackAppId, cancellationToken)
            ?? throw new NotFoundException(nameof(FeedbackApp), request.FeedbackAppId);

        AuthorizationGuard.EnsureCanAccessApp(app, currentUser.UserId, currentUser.IsAdmin);

        var definitions = await fieldDefinitionRepository.ListByAppAsync(app.Id, cancellationToken);
        return definitions
            .Select(d => new FeedbackFieldDefinitionDto(d.Id, d.FieldKey, d.Label, d.FieldType, d.IsRequired, d.DisplayOrder, d.OptionsCsv))
            .ToList();
    }
}
