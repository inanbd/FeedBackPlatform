using FeedbackPlatform.Application.Common;
using FeedbackPlatform.Application.Common.Exceptions;
using FeedbackPlatform.Application.Common.Interfaces;
using FeedbackPlatform.Domain.Entities;
using FeedbackPlatform.Domain.Enums;
using FluentValidation;
using MediatR;

namespace FeedbackPlatform.Application.Features.CustomFields.Commands;

public sealed record FieldDefinitionInput(
    string FieldKey, string Label, FeedbackFieldType FieldType, bool IsRequired, int DisplayOrder, string? OptionsCsv);

/// <summary>Adds one or more custom field definitions to an application in a single request.</summary>
public sealed record CreateFeedbackFieldDefinitionsCommand(
    Guid FeedbackAppId, IReadOnlyList<FieldDefinitionInput> Fields) : IRequest<IReadOnlyList<Guid>>;

public sealed class FieldDefinitionInputValidator : AbstractValidator<FieldDefinitionInput>
{
    public FieldDefinitionInputValidator()
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

public sealed class CreateFeedbackFieldDefinitionsCommandValidator : AbstractValidator<CreateFeedbackFieldDefinitionsCommand>
{
    public CreateFeedbackFieldDefinitionsCommandValidator()
    {
        RuleFor(x => x.Fields).NotEmpty().WithMessage("Add at least one field.");
        RuleForEach(x => x.Fields).SetValidator(new FieldDefinitionInputValidator());
        RuleFor(x => x.Fields)
            .Must(fields => fields.Select(f => f.FieldKey.Trim().ToLowerInvariant()).Distinct().Count() == fields.Count)
            .WithMessage("Field keys must be unique.")
            .When(x => x.Fields.Count > 0);
    }
}

public sealed class CreateFeedbackFieldDefinitionsCommandHandler(
    IFeedbackAppRepository feedbackAppRepository,
    IFeedbackFieldDefinitionRepository fieldDefinitionRepository,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateFeedbackFieldDefinitionsCommand, IReadOnlyList<Guid>>
{
    public async Task<IReadOnlyList<Guid>> Handle(CreateFeedbackFieldDefinitionsCommand request, CancellationToken cancellationToken)
    {
        var app = await feedbackAppRepository.GetByIdAsync(request.FeedbackAppId, cancellationToken)
            ?? throw new NotFoundException(nameof(FeedbackApp), request.FeedbackAppId);

        AuthorizationGuard.EnsureCanAccessApp(app, currentUser.UserId, currentUser.IsAdmin);

        var ids = new List<Guid>(request.Fields.Count);
        foreach (var field in request.Fields)
        {
            var definition = new FeedbackFieldDefinition
            {
                Id = Guid.NewGuid(),
                FeedbackAppId = app.Id,
                FieldKey = field.FieldKey.Trim().ToLowerInvariant(),
                Label = field.Label.Trim(),
                FieldType = field.FieldType,
                IsRequired = field.IsRequired,
                DisplayOrder = field.DisplayOrder,
                OptionsCsv = string.IsNullOrWhiteSpace(field.OptionsCsv) ? null : field.OptionsCsv.Trim()
            };

            await fieldDefinitionRepository.CreateAsync(definition, cancellationToken);
            ids.Add(definition.Id);
        }

        return ids;
    }
}
