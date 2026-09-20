using FeedbackPlatform.Application.Common;
using FeedbackPlatform.Application.Common.Exceptions;
using FeedbackPlatform.Application.Common.Interfaces;
using FeedbackPlatform.Domain.Entities;
using MediatR;

namespace FeedbackPlatform.Application.Features.CustomFields.Commands;

public sealed record DeleteFeedbackFieldDefinitionCommand(Guid Id) : IRequest;

public sealed class DeleteFeedbackFieldDefinitionCommandHandler(
    IFeedbackFieldDefinitionRepository fieldDefinitionRepository,
    IFeedbackAppRepository feedbackAppRepository,
    ICurrentUserService currentUser)
    : IRequestHandler<DeleteFeedbackFieldDefinitionCommand>
{
    public async Task Handle(DeleteFeedbackFieldDefinitionCommand request, CancellationToken cancellationToken)
    {
        var definition = await fieldDefinitionRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.FeedbackFieldDefinition), request.Id);

        var app = await feedbackAppRepository.GetByIdAsync(definition.FeedbackAppId, cancellationToken)
            ?? throw new NotFoundException(nameof(FeedbackApp), definition.FeedbackAppId);

        AuthorizationGuard.EnsureCanAccessApp(app, currentUser.UserId, currentUser.IsAdmin);

        await fieldDefinitionRepository.DeleteAsync(definition.Id, cancellationToken);
    }
}
