using System.Text;
using FeedbackPlatform.Application.Features.ApiKeys.Commands;
using FeedbackPlatform.Application.Features.ApiKeys.Queries;
using FeedbackPlatform.Application.Features.CustomFields.Commands;
using FeedbackPlatform.Application.Features.CustomFields.Queries;
using FeedbackPlatform.Application.Features.FeedbackApps.Commands;
using FeedbackPlatform.Application.Features.FeedbackApps.Queries;
using FeedbackPlatform.Domain.Enums;
using FeedbackPlatform.Web.Models.Applications;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackPlatform.Web.Controllers;

[Authorize]
public sealed class ApplicationsController(IMediator mediator) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var apps = await mediator.Send(new ListMyFeedbackAppsQuery(), cancellationToken);
        return View(apps);
    }

    [HttpGet]
    public IActionResult Create() => View(new CreateFeedbackAppViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateFeedbackAppViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var id = await mediator.Send(new CreateFeedbackAppCommand(model.Name, model.Description), cancellationToken);
        TempData["StatusMessage"] = "Application created. Generate an API key below to start collecting feedback.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var viewModel = await BuildDetailsViewModelAsync(id, newKey: null, cancellationToken);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateApiKey(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GenerateApiKeyCommand(id), cancellationToken);
        var viewModel = await BuildDetailsViewModelAsync(id, result, cancellationToken);
        return View(nameof(Details), viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokeApiKey(Guid id, Guid apiKeyId, CancellationToken cancellationToken)
    {
        await mediator.Send(new RevokeApiKeyCommand(apiKeyId), cancellationToken);
        TempData["StatusMessage"] = "API key revoked.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCustomField(
        Guid id, List<CreateCustomFieldViewModel> newCustomFields, CancellationToken cancellationToken)
    {
        // The parameter name doubles as the model-binding prefix, and must match the "NewCustomFields[i].*"
        // field names the Details view posts (name="NewCustomFields[0].FieldKey" etc.) or binding silently no-ops.
        // Rows left completely blank (an unused extra row) are dropped rather than treated as errors.
        var filledRows = newCustomFields
            .Where(f => !string.IsNullOrWhiteSpace(f.FieldKey) || !string.IsNullOrWhiteSpace(f.Label))
            .ToList();

        if (filledRows.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Add at least one field.");
        }

        if (!ModelState.IsValid || filledRows.Count == 0)
        {
            var viewModel = await BuildDetailsViewModelAsync(id, newKey: null, cancellationToken);
            viewModel.NewCustomFields = newCustomFields.Count > 0 ? newCustomFields : [new()];
            return View(nameof(Details), viewModel);
        }

        try
        {
            await mediator.Send(new CreateFeedbackFieldDefinitionsCommand(
                id,
                filledRows
                    .Select(f => new FieldDefinitionInput(
                        f.FieldKey, f.Label, f.FieldType, f.IsRequired, f.DisplayOrder, f.OptionsCsv))
                    .ToList()),
                cancellationToken);
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(string.Empty, error.ErrorMessage);
            }

            var viewModel = await BuildDetailsViewModelAsync(id, newKey: null, cancellationToken);
            viewModel.NewCustomFields = newCustomFields;
            return View(nameof(Details), viewModel);
        }

        TempData["StatusMessage"] = filledRows.Count == 1
            ? "Custom field added."
            : $"{filledRows.Count} custom fields added.";
        TempData["ShowCurlModal"] = true;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCustomField(Guid id, Guid fieldId, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteFeedbackFieldDefinitionCommand(fieldId), cancellationToken);
        TempData["StatusMessage"] = "Custom field removed.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<ApplicationDetailsViewModel> BuildDetailsViewModelAsync(
        Guid id, GenerateApiKeyResult? newKey, CancellationToken cancellationToken)
    {
        var app = await mediator.Send(new GetFeedbackAppByIdQuery(id), cancellationToken);
        var apiKeys = await mediator.Send(new ListApiKeysQuery(id), cancellationToken);
        var customFields = await mediator.Send(new ListFeedbackFieldDefinitionsQuery(id), cancellationToken);

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var apiKeyForExample = newKey?.RawKey ?? "YOUR_API_KEY";

        return new ApplicationDetailsViewModel
        {
            App = app,
            ApiKeys = apiKeys,
            CustomFields = customFields,
            NewlyGeneratedKey = newKey,
            CurlSnippet = newKey is not null ? BuildCurlSnippet(baseUrl, apiKeyForExample, customFields) : null,
            GeneralCurlSnippet = BuildCurlSnippet(baseUrl, apiKeyForExample, customFields),
            CanManageRateLimit = User.IsInRole("Admin")
        };
    }

    private static string BuildCurlSnippet(
        string baseUrl, string apiKey, IReadOnlyList<Application.Features.CustomFields.Queries.FeedbackFieldDefinitionDto> customFields)
    {
        var body = new Dictionary<string, object?>
        {
            ["appVersion"] = "1.0.0",
            ["title"] = "Great app!",
            ["comment"] = "Loving it so far.",
            ["starRating"] = 5
        };

        if (customFields.Count > 0)
        {
            body["customFields"] = customFields.ToDictionary(f => f.FieldKey, ExampleValue);
        }

        var json = System.Text.Json.JsonSerializer.Serialize(body);

        return
            $"curl -X POST {baseUrl}/api/v1/feedback \\\n" +
            $"  -H \"X-Api-Key: {apiKey}\" \\\n" +
            "  -H \"Content-Type: application/json\" \\\n" +
            $"  -d '{json}'";
    }

    private static string ExampleValue(Application.Features.CustomFields.Queries.FeedbackFieldDefinitionDto field) =>
        field.FieldType switch
        {
            FeedbackFieldType.Number => "0",
            FeedbackFieldType.Boolean => "true",
            FeedbackFieldType.Date => DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"),
            FeedbackFieldType.Select => field.OptionsCsv?.Split(',').FirstOrDefault()?.Trim() ?? "option",
            _ => "example value"
        };
}
