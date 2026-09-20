using FeedbackPlatform.Application.Features.ApiKeys.Commands;
using FeedbackPlatform.Application.Features.ApiKeys.Queries;
using FeedbackPlatform.Application.Features.CustomFields.Commands;
using FeedbackPlatform.Application.Features.CustomFields.Queries;
using FeedbackPlatform.Application.Features.FeedbackApps.Commands;
using FeedbackPlatform.Application.Features.FeedbackApps.Queries;
using FeedbackPlatform.Web.Models.Applications;
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
        Guid id, CreateCustomFieldViewModel newCustomField, CancellationToken cancellationToken)
    {
        // The parameter name doubles as the model-binding prefix, and must match the "NewCustomField.*"
        // field names the Details view posts (asp-for="NewCustomField.FieldKey" etc.) or binding silently no-ops.
        if (!ModelState.IsValid)
        {
            var viewModel = await BuildDetailsViewModelAsync(id, newKey: null, cancellationToken);
            viewModel.NewCustomField = newCustomField;
            return View(nameof(Details), viewModel);
        }

        await mediator.Send(new CreateFeedbackFieldDefinitionCommand(
            id, newCustomField.FieldKey, newCustomField.Label, newCustomField.FieldType,
            newCustomField.IsRequired, newCustomField.DisplayOrder, newCustomField.OptionsCsv), cancellationToken);

        TempData["StatusMessage"] = "Custom field added.";
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

        string? curlSnippet = null;
        if (newKey is not null)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            curlSnippet =
                $"curl -X POST {baseUrl}/api/v1/feedback \\\n" +
                $"  -H \"X-Api-Key: {newKey.RawKey}\" \\\n" +
                "  -H \"Content-Type: application/json\" \\\n" +
                "  -d '{\"appVersion\":\"1.0.0\",\"title\":\"Great app!\",\"comment\":\"Loving it so far.\",\"starRating\":5}'";
        }

        return new ApplicationDetailsViewModel
        {
            App = app,
            ApiKeys = apiKeys,
            CustomFields = customFields,
            NewlyGeneratedKey = newKey,
            CurlSnippet = curlSnippet,
            CanManageRateLimit = User.IsInRole("Admin")
        };
    }
}
