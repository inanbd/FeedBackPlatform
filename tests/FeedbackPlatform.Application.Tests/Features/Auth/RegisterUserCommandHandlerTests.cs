using FeedbackPlatform.Application.Common.Exceptions;
using FeedbackPlatform.Application.Common.Interfaces;
using FeedbackPlatform.Application.Features.Auth.Commands;
using FeedbackPlatform.Domain.Entities;
using FeedbackPlatform.Domain.Enums;
using Moq;
using Xunit;

namespace FeedbackPlatform.Application.Tests.Features.Auth;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();

    private RegisterUserCommandHandler CreateHandler() =>
        new(_userRepository.Object, _passwordHasher.Object);

    [Fact]
    public async Task Handle_WithNewEmail_CreatesUserWithHashedPasswordAndUserRole()
    {
        _userRepository.Setup(r => r.EmailExistsAsync("new@example.com", default)).ReturnsAsync(false);
        _passwordHasher.Setup(h => h.Hash("secret123")).Returns("hashed-secret");

        User? created = null;
        _userRepository
            .Setup(r => r.CreateAsync(It.IsAny<User>(), default))
            .Callback<User, CancellationToken>((u, _) => created = u)
            .Returns(Task.CompletedTask);

        var id = await CreateHandler().Handle(
            new RegisterUserCommand("New@Example.com", "New User", "secret123"), default);

        Assert.NotEqual(Guid.Empty, id);
        Assert.NotNull(created);
        Assert.Equal("new@example.com", created!.Email);
        Assert.Equal("hashed-secret", created.PasswordHash);
        Assert.Equal(UserRole.User, created.Role);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ThrowsConflictException()
    {
        _userRepository.Setup(r => r.EmailExistsAsync("taken@example.com", default)).ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => CreateHandler().Handle(
            new RegisterUserCommand("taken@example.com", "Someone", "secret123"), default));

        _userRepository.Verify(r => r.CreateAsync(It.IsAny<User>(), default), Times.Never);
    }
}
