using FeedbackPlatform.Application.Common.Interfaces;
using FeedbackPlatform.Application.Features.Feedbacks.Commands;
using FeedbackPlatform.Domain.Entities;
using FeedbackPlatform.Domain.Enums;
using FluentValidation;
using Moq;
using Xunit;

namespace FeedbackPlatform.Application.Tests.Features.Feedbacks;

public class SubmitFeedbackCommandHandlerTests
{
    private readonly Mock<IFeedbackRepository> _feedbackRepository = new();
    private readonly Mock<IFeedbackFieldDefinitionRepository> _fieldDefinitionRepository = new();
    private readonly Mock<IFeedbackAppRepository> _feedbackAppRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IEmailNotificationQueue> _emailQueue = new();

    private static readonly Guid AppId = Guid.NewGuid();
    private static readonly Guid OwnerId = Guid.NewGuid();

    private SubmitFeedbackCommandHandler CreateHandler() => new(
        _feedbackRepository.Object, _fieldDefinitionRepository.Object,
        _feedbackAppRepository.Object, _userRepository.Object, _emailQueue.Object);

    private SubmitFeedbackCommand ValidCommand(IReadOnlyDictionary<string, string>? customFields = null) =>
        new(AppId, "1.0.0", "Title", "Comment", 5, "Reporter", "reporter@example.com", "127.0.0.1", customFields);

    private void SetUpApp() =>
        _feedbackAppRepository
            .Setup(r => r.GetByIdAsync(AppId, default))
            .ReturnsAsync(new FeedbackApp { Id = AppId, OwnerUserId = OwnerId, Name = "App", CreatedAtUtc = DateTimeOffset.UtcNow });

    [Fact]
    public async Task Handle_WithNoCustomFieldDefinitions_SavesFeedbackAndQueuesNotification()
    {
        SetUpApp();
        _fieldDefinitionRepository
            .Setup(r => r.ListByAppAsync(AppId, default))
            .ReturnsAsync(Array.Empty<FeedbackFieldDefinition>());
        _userRepository
            .Setup(r => r.GetByIdAsync(OwnerId, default))
            .ReturnsAsync(new User { Id = OwnerId, Email = "owner@example.com", DisplayName = "Owner", PasswordHash = "x", CreatedAtUtc = DateTimeOffset.UtcNow });

        Feedback? saved = null;
        _feedbackRepository
            .Setup(r => r.CreateAsync(It.IsAny<Feedback>(), default))
            .Callback<Feedback, CancellationToken>((f, _) => saved = f)
            .ReturnsAsync((Feedback f, CancellationToken _) => f.Id);

        await CreateHandler().Handle(ValidCommand(), default);

        Assert.NotNull(saved);
        Assert.Equal("Title", saved!.Title);
        Assert.Null(saved.CustomFieldsJson);
        _emailQueue.Verify(q => q.QueueFeedbackNotification(
            It.Is<FeedbackNotificationMessage>(m => m.OwnerEmail == "owner@example.com")), Times.Once);
    }

    [Fact]
    public async Task Handle_MissingRequiredCustomField_ThrowsValidationException()
    {
        SetUpApp();
        _fieldDefinitionRepository
            .Setup(r => r.ListByAppAsync(AppId, default))
            .ReturnsAsync(new[]
            {
                new FeedbackFieldDefinition
                {
                    Id = Guid.NewGuid(), FeedbackAppId = AppId, FieldKey = "browser_name",
                    Label = "Browser", FieldType = FeedbackFieldType.Text, IsRequired = true, DisplayOrder = 1
                }
            });

        await Assert.ThrowsAsync<ValidationException>(() => CreateHandler().Handle(ValidCommand(), default));

        _feedbackRepository.Verify(r => r.CreateAsync(It.IsAny<Feedback>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_WithRequiredCustomFieldProvided_SerializesCustomFieldsJson()
    {
        SetUpApp();
        _fieldDefinitionRepository
            .Setup(r => r.ListByAppAsync(AppId, default))
            .ReturnsAsync(new[]
            {
                new FeedbackFieldDefinition
                {
                    Id = Guid.NewGuid(), FeedbackAppId = AppId, FieldKey = "browser_name",
                    Label = "Browser", FieldType = FeedbackFieldType.Text, IsRequired = true, DisplayOrder = 1
                }
            });
        _userRepository.Setup(r => r.GetByIdAsync(OwnerId, default)).ReturnsAsync((User?)null);

        Feedback? saved = null;
        _feedbackRepository
            .Setup(r => r.CreateAsync(It.IsAny<Feedback>(), default))
            .Callback<Feedback, CancellationToken>((f, _) => saved = f)
            .ReturnsAsync((Feedback f, CancellationToken _) => f.Id);

        await CreateHandler().Handle(
            ValidCommand(new Dictionary<string, string> { ["browser_name"] = "Chrome" }), default);

        Assert.NotNull(saved);
        Assert.Contains("browser_name", saved!.CustomFieldsJson);
        Assert.Contains("Chrome", saved.CustomFieldsJson);
    }
}
