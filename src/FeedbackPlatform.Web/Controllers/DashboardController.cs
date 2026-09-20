using FeedbackPlatform.Application.Features.FeedbackApps.Queries;
using FeedbackPlatform.Application.Features.Feedbacks.Queries;
using FeedbackPlatform.Web.Models.Dashboard;
using FeedbackPlatform.Web.Models.Feedback;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackPlatform.Web.Controllers;

[Authorize]
public sealed class DashboardController(IMediator mediator) : Controller
{
    private const int DaysInRange = 30;

    public async Task<IActionResult> Index(Guid? appId, CancellationToken cancellationToken)
    {
        var toDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var fromDate = toDate.AddDays(-(DaysInRange - 1));

        var stats = await mediator.Send(new GetFeedbackStatsQuery(appId, fromDate, toDate), cancellationToken);

        var appOptions = User.IsInRole("Admin")
            ? (await mediator.Send(new ListAllFeedbackAppsQuery(), cancellationToken))
                .Select(a => new FeedbackAppOption(a.Id, $"{a.Name} ({a.OwnerEmail})"))
                .ToList()
            : (await mediator.Send(new ListMyFeedbackAppsQuery(), cancellationToken))
                .Select(a => new FeedbackAppOption(a.Id, a.Name))
                .ToList();

        var viewModel = new DashboardViewModel
        {
            Stats = stats,
            AppOptions = appOptions,
            AppIdFilter = appId,
            FromDate = fromDate,
            ToDate = toDate
        };

        return View(viewModel);
    }
}
