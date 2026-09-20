using FeedbackPlatform.Application.Features.CustomFields.Queries;
using FeedbackPlatform.Application.Features.Feedbacks.Commands;
using FeedbackPlatform.Web.Auth;
using FeedbackPlatform.Web.Models.Api;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackPlatform.Web.Controllers.Api;

/// <summary>
/// The public feedback ingestion API. Every application registered in the portal calls this
/// with the API key it generated for itself (see the "X-Api-Key" header).
/// </summary>
[ApiController]
[Route("api/v1/feedback")]
[Authorize(AuthenticationSchemes = ApiKeyAuthenticationHandler.SchemeName)]
public sealed class FeedbackApiController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] SubmitFeedbackRequest request, CancellationToken cancellationToken)
    {
        var feedbackAppId = GetFeedbackAppId();

        var id = await mediator.Send(new SubmitFeedbackCommand(
            feedbackAppId,
            request.AppVersion,
            request.Title,
            request.Comment,
            request.StarRating,
            request.ReporterName,
            request.ReporterContact,
            GetClientIpAddress(),
            request.CustomFields), cancellationToken);

        return CreatedAtAction(nameof(Submit), new { id });
    }

    /// <summary>Lets a client app discover the custom fields its owner configured, to render them dynamically.</summary>
    [HttpGet("fields")]
    public async Task<IActionResult> GetFields(CancellationToken cancellationToken)
    {
        var fields = await mediator.Send(new GetFeedbackFieldDefinitionsForApiQuery(GetFeedbackAppId()), cancellationToken);
        return Ok(fields);
    }

    private Guid GetFeedbackAppId() =>
        Guid.Parse(User.FindFirst(ApiKeyAuthenticationHandler.FeedbackAppIdClaimType)!.Value);

    /// <summary>
    /// Relies on the ASP.NET Core ForwardedHeaders middleware (configured in Program.cs) to
    /// rewrite RemoteIpAddress from X-Forwarded-For — but only for requests from proxies the
    /// deployment explicitly trusts. Reading that header directly here would let any caller
    /// spoof the logged IP.
    /// </summary>
    private string GetClientIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
