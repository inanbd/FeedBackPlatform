using FeedbackPlatform.Application.Common.Exceptions;
using FeedbackPlatform.Application.Common.Interfaces;
using FeedbackPlatform.Application.Features.ApiKeys.Commands;
using FeedbackPlatform.Domain.Entities;
using FeedbackPlatform.Domain.Enums;
using Moq;
using Xunit;

namespace FeedbackPlatform.Application.Tests.Features.ApiKeys;

public class GenerateApiKeyCommandHandlerTests
{
    private readonly Mock<IFeedbackAppRepository> _feedbackAppRepository = new();
    private readonly Mock<IApiKeyRepository> _apiKeyRepository = new();
    private readonly Mock<IApiKeyGenerator> _apiKeyGenerator = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly Guid AppId = Guid.NewGuid();

    private GenerateApiKeyCommandHandler CreateHandler() => new(
        _feedbackAppRepository.Object, _apiKeyRepository.Object, _apiKeyGenerator.Object, _currentUser.Object);

    private void SetUpApp() =>
        _feedbackAppRepository
            .Setup(r => r.GetByIdAsync(AppId, default))
            .ReturnsAsync(new FeedbackApp { Id = AppId, OwnerUserId = OwnerId, Name = "App", CreatedAtUtc = DateTimeOffset.UtcNow });

    [Fact]
    public async Task Handle_AsOwner_GeneratesAndPersistsKey()
    {
        SetUpApp();
        _currentUser.SetupGet(c => c.UserId).Returns(OwnerId);
        _currentUser.SetupGet(c => c.Role).Returns(UserRole.User);
        _currentUser.SetupGet(c => c.IsAdmin).Returns(false);
        _apiKeyGenerator.Setup(g => g.Generate()).Returns(new GeneratedApiKey("fbk_raw", "fbk_prefix", "hash"));

        var result = await CreateHandler().Handle(new GenerateApiKeyCommand(AppId), default);

        Assert.Equal("fbk_raw", result.RawKey);
        _apiKeyRepository.Verify(r => r.CreateAsync(
            It.Is<Domain.Entities.ApiKey>(k => k.FeedbackAppId == AppId && k.KeyHash == "hash"), default), Times.Once);
    }

    [Fact]
    public async Task Handle_AsUnrelatedUser_ThrowsForbiddenAccessException()
    {
        SetUpApp();
        _currentUser.SetupGet(c => c.UserId).Returns(Guid.NewGuid());
        _currentUser.SetupGet(c => c.Role).Returns(UserRole.User);
        _currentUser.SetupGet(c => c.IsAdmin).Returns(false);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            CreateHandler().Handle(new GenerateApiKeyCommand(AppId), default));

        _apiKeyRepository.Verify(r => r.CreateAsync(It.IsAny<Domain.Entities.ApiKey>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_AsAdminForSomeoneElsesApp_Succeeds()
    {
        SetUpApp();
        _currentUser.SetupGet(c => c.UserId).Returns(Guid.NewGuid());
        _currentUser.SetupGet(c => c.Role).Returns(UserRole.Admin);
        _currentUser.SetupGet(c => c.IsAdmin).Returns(true);
        _apiKeyGenerator.Setup(g => g.Generate()).Returns(new GeneratedApiKey("fbk_raw", "fbk_prefix", "hash"));

        var result = await CreateHandler().Handle(new GenerateApiKeyCommand(AppId), default);

        Assert.Equal("fbk_raw", result.RawKey);
    }
}
