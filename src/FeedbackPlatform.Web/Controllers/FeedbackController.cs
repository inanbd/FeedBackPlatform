using FeedbackPlatform.Application.Features.FeedbackApps.Queries;
using FeedbackPlatform.Application.Features.Feedbacks.Queries;
using FeedbackPlatform.Web.Models.Feedback;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackPlatform.Web.Controllers;

[Authorize]
public sealed class FeedbackController(IMediator mediator) : Controller
{
    private const int PageSize = 20;

    public async Task<IActionResult> Index(Guid? appId, int? minRating, int page = 1, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new ListFeedbackQuery(appId, minRating, page, PageSize), cancellationToken);

        var appOptions = User.IsInRole("Admin")
            ? (await mediator.Send(new ListAllFeedbackAppsQuery(), cancellationToken))
                .Select(a => new FeedbackAppOption(a.Id, $"{a.Name} ({a.OwnerEmail})"))
                .ToList()
            : (await mediator.Send(new ListMyFeedbackAppsQuery(), cancellationToken))
                .Select(a => new FeedbackAppOption(a.Id, a.Name))
                .ToList();

        var viewModel = new FeedbackIndexViewModel
        {
            Feedback = result,
            AppOptions = appOptions,
            AppIdFilter = appId,
            MinStarRating = minRating
        };

        return View(viewModel);
    }
}
