using FeedbackPlatform.Application.Common;
using FeedbackPlatform.Application.Common.Exceptions;
using FeedbackPlatform.Application.Common.Interfaces;
using FeedbackPlatform.Domain.Entities;
using FeedbackPlatform.Domain.Enums;
using FluentValidation;
using MediatR;

namespace FeedbackPlatform.Application.Features.CustomFields.Commands;

public sealed record CreateFeedbackFieldDefinitionCommand(
    Guid FeedbackAppId,
    string FieldKey,
    string Label,
    FeedbackFieldType FieldType,
    bool IsRequired,
    int DisplayOrder,
    string? OptionsCsv) : IRequest<Guid>;

public sealed class CreateFeedbackFieldDefinitionCommandValidator : AbstractValidator<CreateFeedbackFieldDefinitionCommand>
{
    public CreateFeedbackFieldDefinitionCommandValidator()
    {
        RuleFor(x => x.FieldKey)
            .NotEmpty()
            .MaximumLength(100)
            .Matches("^[a-z][a-z0-9_]*$")
            .WithMessage("Field key must be lowercase letters, numbers, and underscores, starting with a letter.");
        RuleFor(x => x.Label).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FieldType).IsInEnum();
        RuleFor(x => x.OptionsCsv)
            .NotEmpty()
            .When(x => x.FieldType == FeedbackFieldType.Select)
            .WithMessage("Select fields require at least one option.");
    }
}

public sealed class CreateFeedbackFieldDefinitionCommandHandler(
    IFeedbackAppRepository feedbackAppRepository,
    IFeedbackFieldDefinitionRepository fieldDefinitionRepository,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateFeedbackFieldDefinitionCommand, Guid>
{
    public async Task<Guid> Handle(CreateFeedbackFieldDefinitionCommand request, CancellationToken cancellationToken)
    {
        var app = await feedbackAppRepository.GetByIdAsync(request.FeedbackAppId, cancellationToken)
            ?? throw new NotFoundException(nameof(FeedbackApp), request.FeedbackAppId);

        AuthorizationGuard.EnsureCanAccessApp(app, currentUser.UserId, currentUser.IsAdmin);

        var definition = new FeedbackFieldDefinition
        {
            Id = Guid.NewGuid(),
            FeedbackAppId = app.Id,
            FieldKey = request.FieldKey.Trim().ToLowerInvariant(),
            Label = request.Label.Trim(),
            FieldType = request.FieldType,
            IsRequired = request.IsRequired,
            DisplayOrder = request.DisplayOrder,
            OptionsCsv = string.IsNullOrWhiteSpace(request.OptionsCsv) ? null : request.OptionsCsv.Trim()
        };

        await fieldDefinitionRepository.CreateAsync(definition, cancellationToken);
        return definition.Id;
    }
}
