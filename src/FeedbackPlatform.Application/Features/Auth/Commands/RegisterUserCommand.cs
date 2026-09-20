using FeedbackPlatform.Application.Common.Exceptions;
using FeedbackPlatform.Application.Common.Interfaces;
using FeedbackPlatform.Domain.Entities;
using FeedbackPlatform.Domain.Enums;
using FluentValidation;
using MediatR;

namespace FeedbackPlatform.Application.Features.Auth.Commands;

public sealed record RegisterUserCommand(string Email, string DisplayName, string Password) : IRequest<Guid>;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).WithMessage("Password must be at least 8 characters long.");
    }
}

public sealed class RegisterUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    : IRequestHandler<RegisterUserCommand, Guid>
{
    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        if (await userRepository.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            throw new ConflictException("An account with this email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            DisplayName = request.DisplayName.Trim(),
            PasswordHash = passwordHasher.Hash(request.Password),
            Role = UserRole.User,
            IsActive = true,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        await userRepository.CreateAsync(user, cancellationToken);
        return user.Id;
    }
}
