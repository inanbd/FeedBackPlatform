using FeedbackPlatform.Application.Common.Interfaces;
using MediatR;

namespace FeedbackPlatform.Application.Features.CustomFields.Queries;

/// <summary>
/// Used by the feedback submission API to expose an application's custom field schema to
/// its own client apps, so they can render the extra fields. The caller is already
/// authorized by holding a valid API key for this exact FeedbackAppId (see ApiKeyAuthenticationHandler),
/// so no additional ownership check is needed here.
/// </summary>
public sealed record GetFeedbackFieldDefinitionsForApiQuery(Guid FeedbackAppId) : IRequest<IReadOnlyList<FeedbackFieldDefinitionDto>>;

public sealed class GetFeedbackFieldDefinitionsForApiQueryHandler(IFeedbackFieldDefinitionRepository fieldDefinitionRepository)
    : IRequestHandler<GetFeedbackFieldDefinitionsForApiQuery, IReadOnlyList<FeedbackFieldDefinitionDto>>
{
    public async Task<IReadOnlyList<FeedbackFieldDefinitionDto>> Handle(
        GetFeedbackFieldDefinitionsForApiQuery request, CancellationToken cancellationToken)
    {
        var definitions = await fieldDefinitionRepository.ListByAppAsync(request.FeedbackAppId, cancellationToken);
        return definitions
            .Select(d => new FeedbackFieldDefinitionDto(d.Id, d.FieldKey, d.Label, d.FieldType, d.IsRequired, d.DisplayOrder, d.OptionsCsv))
            .ToList();
    }
}
