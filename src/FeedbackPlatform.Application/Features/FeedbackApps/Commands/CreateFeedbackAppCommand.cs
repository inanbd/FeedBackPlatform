using FeedbackPlatform.Application.Common.Interfaces;
using FeedbackPlatform.Domain.Entities;
using FluentValidation;
using MediatR;

namespace FeedbackPlatform.Application.Features.FeedbackApps.Commands;

public sealed record CreateFeedbackAppCommand(string Name, string? Description) : IRequest<Guid>;

public sealed class CreateFeedbackAppCommandValidator : AbstractValidator<CreateFeedbackAppCommand>
{
    public CreateFeedbackAppCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public sealed class CreateFeedbackAppCommandHandler(
    IFeedbackAppRepository feedbackAppRepository, ICurrentUserService currentUser)
    : IRequestHandler<CreateFeedbackAppCommand, Guid>
{
    public async Task<Guid> Handle(CreateFeedbackAppCommand request, CancellationToken cancellationToken)
    {
        var app = new FeedbackApp
        {
            Id = Guid.NewGuid(),
            OwnerUserId = currentUser.UserId,
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        await feedbackAppRepository.CreateAsync(app, cancellationToken);
        return app.Id;
    }
}
