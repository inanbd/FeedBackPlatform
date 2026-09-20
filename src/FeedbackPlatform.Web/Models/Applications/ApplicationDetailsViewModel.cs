using FeedbackPlatform.Application.Features.ApiKeys.Commands;
using FeedbackPlatform.Application.Features.ApiKeys.Queries;
using FeedbackPlatform.Application.Features.CustomFields.Queries;
using FeedbackPlatform.Application.Features.FeedbackApps.Queries;

namespace FeedbackPlatform.Web.Models.Applications;

public sealed class ApplicationDetailsViewModel
{
    public required FeedbackAppDto App { get; init; }
    public required IReadOnlyList<ApiKeyDto> ApiKeys { get; init; }
    public required IReadOnlyList<FeedbackFieldDefinitionDto> CustomFields { get; init; }
    public GenerateApiKeyResult? NewlyGeneratedKey { get; init; }
    public string? CurlSnippet { get; init; }
    public CreateCustomFieldViewModel NewCustomField { get; set; } = new();
    public bool CanManageRateLimit { get; init; }
}
