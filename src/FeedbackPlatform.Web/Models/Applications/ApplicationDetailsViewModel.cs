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

    /// <summary>Shown once, right after generating a key: a curl example using the real, one-time secret.</summary>
    public string? CurlSnippet { get; init; }

    /// <summary>
    /// Always available (API Key Example modal): a curl example reflecting the app's current custom
    /// fields, using the real key if one was just generated this request, otherwise a placeholder.
    /// </summary>
    public required string GeneralCurlSnippet { get; init; }

    public List<CreateCustomFieldViewModel> NewCustomFields { get; set; } = [new()];
    public bool CanManageRateLimit { get; init; }
}
