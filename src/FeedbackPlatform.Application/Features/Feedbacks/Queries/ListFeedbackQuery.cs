using FeedbackPlatform.Application.Common;
using FeedbackPlatform.Application.Common.Interfaces;
using FeedbackPlatform.Application.Common.Models;
using MediatR;

namespace FeedbackPlatform.Application.Features.Feedbacks.Queries;

public sealed record ListFeedbackQuery(Guid? AppIdFilter, int? MinStarRating, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<FeedbackDto>>;

public sealed class ListFeedbackQueryHandler(
    IFeedbackRepository feedbackRepository, FeedbackScopeResolver scopeResolver, ICurrentUserService currentUser)
    : IRequestHandler<ListFeedbackQuery, PagedResult<FeedbackDto>>
{
    public async Task<PagedResult<FeedbackDto>> Handle(ListFeedbackQuery request, CancellationToken cancellationToken)
    {
        var scope = await scopeResolver.ResolveAsync(
            request.AppIdFilter, currentUser.UserId, currentUser.IsAdmin, cancellationToken);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 200);

        var (items, totalCount) = await feedbackRepository.SearchAsync(
            new FeedbackSearchQuery(scope, request.MinStarRating, page, pageSize), cancellationToken);

        var dtos = items
            .Select(f => new FeedbackDto(
                f.Id, f.FeedbackAppId, f.AppVersion, f.Title, f.Comment, f.StarRating,
                f.ReporterName, f.ReporterContact, f.IpAddress, f.CustomFieldsJson, f.CreatedAtUtc))
            .ToList();

        return new PagedResult<FeedbackDto>(dtos, totalCount, page, pageSize);
    }
}
