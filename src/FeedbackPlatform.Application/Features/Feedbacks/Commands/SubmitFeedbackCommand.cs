using System.Text.Json;
using FeedbackPlatform.Application.Common.Exceptions;
using FeedbackPlatform.Application.Common.Interfaces;
using FeedbackPlatform.Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace FeedbackPlatform.Application.Features.Feedbacks.Commands;

public sealed record SubmitFeedbackCommand(
    Guid FeedbackAppId,
    string AppVersion,
    string Title,
    string Comment,
    int StarRating,
    string? ReporterName,
    string? ReporterContact,
    string IpAddress,
    IReadOnlyDictionary<string, string>? CustomFields) : IRequest<Guid>;

public sealed class SubmitFeedbackCommandValidator : AbstractValidator<SubmitFeedbackCommand>
{
    public SubmitFeedbackCommandValidator()
    {
        RuleFor(x => x.AppVersion).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Comment).NotEmpty();
        RuleFor(x => x.StarRating).InclusiveBetween(1, 5);
        RuleFor(x => x.ReporterName).MaximumLength(200);
        RuleFor(x => x.ReporterContact).MaximumLength(200);
        RuleFor(x => x.IpAddress).NotEmpty();
    }
}

public sealed class SubmitFeedbackCommandHandler(
    IFeedbackRepository feedbackRepository,
    IFeedbackFieldDefinitionRepository fieldDefinitionRepository,
    IFeedbackAppRepository feedbackAppRepository,
    IUserRepository userRepository,
    IEmailNotificationQueue emailQueue)
    : IRequestHandler<SubmitFeedbackCommand, Guid>
{
    public async Task<Guid> Handle(SubmitFeedbackCommand request, CancellationToken cancellationToken)
    {
        var app = await feedbackAppRepository.GetByIdAsync(request.FeedbackAppId, cancellationToken)
            ?? throw new NotFoundException(nameof(FeedbackApp), request.FeedbackAppId);

        var definitions = await fieldDefinitionRepository.ListByAppAsync(app.Id, cancellationToken);
        var customFieldsJson = definitions.Count > 0
            ? ValidateAndSerializeCustomFields(definitions, request.CustomFields)
            : null;

        var feedback = new Feedback
        {
            Id = Guid.NewGuid(),
            FeedbackAppId = app.Id,
            AppVersion = request.AppVersion.Trim(),
            Title = request.Title.Trim(),
            Comment = request.Comment.Trim(),
            StarRating = request.StarRating,
            ReporterName = string.IsNullOrWhiteSpace(request.ReporterName) ? null : request.ReporterName.Trim(),
            ReporterContact = string.IsNullOrWhiteSpace(request.ReporterContact) ? null : request.ReporterContact.Trim(),
            IpAddress = request.IpAddress,
            CustomFieldsJson = customFieldsJson,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        var id = await feedbackRepository.CreateAsync(feedback, cancellationToken);

        var owner = await userRepository.GetByIdAsync(app.OwnerUserId, cancellationToken);
        if (owner is not null)
        {
            emailQueue.QueueFeedbackNotification(new FeedbackNotificationMessage(
                owner.Email, app.Name, feedback.Title, feedback.Comment, feedback.StarRating,
                feedback.AppVersion, feedback.CreatedAtUtc));
        }

        return id;
    }

    private static string ValidateAndSerializeCustomFields(
        IReadOnlyList<FeedbackFieldDefinition> definitions, IReadOnlyDictionary<string, string>? provided)
    {
        provided ??= new Dictionary<string, string>();
        var failures = new List<ValidationFailure>();
        var result = new Dictionary<string, string>();

        foreach (var definition in definitions)
        {
            var hasValue = provided.TryGetValue(definition.FieldKey, out var value) && !string.IsNullOrWhiteSpace(value);

            if (definition.IsRequired && !hasValue)
            {
                failures.Add(new ValidationFailure(
                    $"customFields.{definition.FieldKey}", $"'{definition.Label}' is required."));
                continue;
            }

            if (hasValue)
            {
                result[definition.FieldKey] = value!;
            }
        }

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return JsonSerializer.Serialize(result);
    }
}
