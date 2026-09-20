using FeedbackPlatform.Application.Features.FeedbackApps.Queries;
using FeedbackPlatform.Application.Features.Settings.Commands;
using FeedbackPlatform.Application.Features.Settings.Queries;
using FeedbackPlatform.Web.Models.Admin;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackPlatform.Web.Controllers;

[Authorize(Roles = "Admin")]
public sealed class AdminController(IMediator mediator) : Controller
{
    public async Task<IActionResult> Applications(CancellationToken cancellationToken)
    {
        var apps = await mediator.Send(new ListAllFeedbackAppsQuery(), cancellationToken);
        return View(apps);
    }

    [HttpGet]
    public async Task<IActionResult> Settings(CancellationToken cancellationToken)
    {
        var globalLimit = await mediator.Send(new GetGlobalRateLimitQuery(), cancellationToken);
        return View(new AdminSettingsViewModel { GlobalRateLimitPerMinute = globalLimit });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(AdminSettingsViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await mediator.Send(new SetGlobalRateLimitCommand(model.GlobalRateLimitPerMinute), cancellationToken);
        TempData["StatusMessage"] = "Global rate limit updated.";
        return RedirectToAction(nameof(Settings));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetAppRateLimitOverride(Guid id, int? requestsPerMinute, CancellationToken cancellationToken)
    {
        await mediator.Send(new SetAppRateLimitOverrideCommand(id, requestsPerMinute), cancellationToken);
        TempData["StatusMessage"] = "Application rate limit updated.";
        return RedirectToAction(nameof(Applications));
    }
}
