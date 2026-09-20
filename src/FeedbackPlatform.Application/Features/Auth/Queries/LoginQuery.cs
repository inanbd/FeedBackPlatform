using FeedbackPlatform.Application.Common.Exceptions;
using FeedbackPlatform.Application.Common.Interfaces;
using FeedbackPlatform.Domain.Enums;
using FluentValidation;
using MediatR;

namespace FeedbackPlatform.Application.Features.Auth.Queries;

public sealed record LoginResult(Guid UserId, string Email, string DisplayName, UserRole Role);

public sealed record LoginQuery(string Email, string Password) : IRequest<LoginResult>;

public sealed class LoginQueryValidator : AbstractValidator<LoginQuery>
{
    public LoginQueryValidator()
    {
        RuleFor(x => x.Email).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class LoginQueryHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    : IRequestHandler<LoginQuery, LoginResult>
{
    public async Task<LoginResult> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken);
        if (user is null || !user.IsActive || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        return new LoginResult(user.Id, user.Email, user.DisplayName, user.Role);
    }
}
